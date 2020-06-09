using UnityEngine;
using System.Collections;
using TMPro;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CarBehaviourNetwork : NetworkBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;
    public WheelBehaviour[] wheelBehaviours = new WheelBehaviour[4];

    public GearType gearType;
    public float maxSteerAngle = 45;
    public float forwardFriction = 2;
    public float sidewaysFriction = 1;
    public Transform centerOfMass;
    public float engineVolume;
    public float maxTachoAngle = 30;

    public bool thrustEnabled = true;

    public RectTransform speedPointerTransform;
    public TMP_Text speedText;
    public TMP_Text gearText;

    public AudioClip engineSingleRPMSoundClip;
    private AudioSource _engineAudioSource;


    private Rigidbody _rigidBody;
    private float _currentSpeedKMH;
    private int _currentGear;
    private float _maxTorque;
    private float _maxBreakTorqueFront;
    private float _maxBreakTorqueRear;
    private float _torqueReduction;
    private float _maxSpeedKMH;
    private float _maxSpeedBackwardKMH;
    private bool _canChangeGearType;

    public ParticleSystem smokeL;
    public ParticleSystem smokeR;

    public ParticleSystem dustFL;
    public ParticleSystem dustFR; 
    public ParticleSystem dustRL;
    public ParticleSystem dustRR;

    private ParticleSystem.EmissionModule _smokeLEmission;
    private ParticleSystem.EmissionModule _smokeREmission;

    private ParticleSystem.EmissionModule _dustFLEmission;
    private ParticleSystem.EmissionModule _dustFREmission;
    private ParticleSystem.EmissionModule _dustRLEmission;
    private ParticleSystem.EmissionModule _dustRREmission;

    private bool _carIsOnDrySand;
    private string _groundTagRL;
    private string _groundTagRR;
    private int _groundTextureRL;
    private int _groundTextureRR;

    public float fullBrakeTorque = 5000;
    public AudioClip brakeAudioClip;

    [SyncVar]
    private bool _doSkidmarking;

    private bool _carIsNotOnSand;
    private AudioSource _brakeAudioSource;
    private bool _isInitialized = false;
    [SyncVar(hook = "OnPrefsChanged")]
    private Prefs _prefs;
    public MeshRenderer bodyBuggy;
    public MeshRenderer canisters;
    public MeshRenderer cannon;
    public MeshRenderer rocketL;
    public MeshRenderer rocketR;
    public Collider collider;
    private CarBehaviourNetwork carBehaviour;
    public TimingNetworkBehaviour timingNetworkBehaviour;

    public TMP_Text playerNameText;
    [SyncVar(hook="OnPlayerNameChanged")]
    public string playerName;


    ////////////////////////////
    // Network Syncronisation //
    ////////////////////////////
    // Command functions all called on clients and executed on the server
    [Command] void CmdSetMotorTorque(float amount) { RpcSetMotorTorque(amount); }
    [Command] void CmdSetBrakeTorque(float amount) { RpcSetBrakeTorque(amount); }
    [Command] void CmdSetSteerAngle(float angle) { RpcSetSteerAngle(angle); }
    [Command] void CmdSetBrakeSound(bool doSound) { RpcSetBrakeSound(doSound); }
    [Command] void CmdSetRPMEffects(float rpm) { RpcSetRPMEffects(rpm); }
    [Command] void CmdSyncSkidmarks(bool value) {
        RpcSetSkidmarks(value);
    }
    [Command] void CmdSyncPrefs(Prefs prefs) { _prefs = prefs; }
    [Command] public void CmdActive(bool value) { RpcActive(value); }

    [Command]
    public void CmdChangeText(string value)
    {
        RpcChangeText(value);
    }

    [Command]
    public void CmdIncreaseRank(int value)
    {
        RpcIncreaseRank(value);
    }


    // Remote Procedure calls are called on the server and executed on the clients
    [ClientRpc]
    void RpcChangeText(string value)
    {
        if (timingNetworkBehaviour == null)
        {
            timingNetworkBehaviour = GameObject.Find("GateCollider").GetComponent<TimingNetworkBehaviour>();
        }

        if (!isLocalPlayer)
            timingNetworkBehaviour.SetRankingsText(value);
    }
    [ClientRpc] void RpcSetMotorTorque(float amount) { if (!isLocalPlayer) SetMotorTorque(amount); }
    [ClientRpc] void RpcSetBrakeTorque(float amount) { if (!isLocalPlayer) SetBrakeTorque(amount); }
    [ClientRpc] void RpcSetSteerAngle(float angle) { if (!isLocalPlayer) SetSteerAngle(angle); }
    [ClientRpc] void RpcSetBrakeSound(bool doSound) { if (!isLocalPlayer)
        {
            SetBrakeSound(doSound);
            //Debug.Log("Playing brake sound: "+ doSound);
        }
    }
    [ClientRpc] void RpcSetRPMEffects(float rpm) { if (!isLocalPlayer) SetRPMEffects(rpm); }
    [ClientRpc] void RpcActive(bool active) { if (!isLocalPlayer) gameObject.active = active; }
    [ClientRpc] void RpcSetSkidmarks(bool value) { if (!isLocalPlayer)
            SetSkidmarking(value);
    }

    [Command]
    void CmdSetPlayerText(string value)
    {
        playerName = value;
    }
    [ClientRpc]
    public void RpcIncreaseRank(int value)
    {
        if (timingNetworkBehaviour == null)
        {
            timingNetworkBehaviour = GameObject.Find("GateCollider").GetComponent<TimingNetworkBehaviour>();
        }

        if (!isLocalPlayer)
            timingNetworkBehaviour.SetIncreaseRank(value);
    }

    void OnPlayerNameChanged(string value)
    {
        SetPlayerText(value);
    }

    private Gear[] sportGears = new Gear[]
                {
                    new Gear(1, 700, 40, 5200),
                    new Gear(40, 3000, 72, 5600),
                    new Gear(72, 3800, 110, 6100),
                    new Gear(110, 4700, 151, 6500),
                    new Gear(151, 5300, 197, 7100),
                    new Gear(197, 5400, 180, 8000)
                };
    private Gear[] comfortGears = new Gear[]
                {
                    new Gear(1, 900, 12, 1400),
                    new Gear(12, 900, 25, 2000),
                    new Gear(25, 1350, 45, 2500),
                    new Gear(45, 1950, 70, 3500),
                    new Gear(70, 2500, 112, 4000),
                    new Gear(112, 3100, 180, 5000)
                };

    void Start()
    {
        carBehaviour = ClientScene.localPlayers[0].gameObject.GetComponent<CarBehaviourNetwork>();
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.centerOfMass = new Vector3(centerOfMass.localPosition.x,
                                              centerOfMass.localPosition.y,
                                              centerOfMass.localPosition.z);      

        // Configure AudioSource component by program
        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        _engineAudioSource.clip = engineSingleRPMSoundClip;
        _engineAudioSource.loop = true;
        _engineAudioSource.volume = engineVolume;
        _engineAudioSource.playOnAwake = true;
        _engineAudioSource.enabled = false; // Bugfix
        _engineAudioSource.enabled = true; // Bugfix

        _smokeLEmission = smokeL.emission;
        _smokeREmission = smokeR.emission;
        _smokeLEmission.enabled = true;
        _smokeREmission.enabled = true;

        _dustFLEmission = dustFL.emission;
        _dustFREmission = dustFR.emission;
        _dustRLEmission = dustRL.emission;
        _dustRREmission = dustRR.emission;
        _dustFLEmission.enabled = true;
        _dustFREmission.enabled = true;
        _dustRLEmission.enabled = true;
        _dustRREmission.enabled = true;

        _brakeAudioSource = (AudioSource)gameObject.AddComponent<AudioSource>();
        _brakeAudioSource.clip = brakeAudioClip;
        _brakeAudioSource.loop = true;
        _brakeAudioSource.volume = 0.7f;
        _brakeAudioSource.playOnAwake = false;

        if (SceneManager.GetActiveScene().name != "SceneLobby")
        {
            speedPointerTransform = GameObject.Find("SpeedPointer").GetComponent<RectTransform>();
            speedText = GameObject.Find("SpeedText").GetComponent<TMPro.TMP_Text>();
            gearText = GameObject.Find("GearText").GetComponent<TMPro.TMP_Text>();
        }

        _isInitialized = true;
        ReapplyPrefs();

        if (isServer)
        {
            string value = LobbyManager.instance.playersListBehaviour
                .LobbyPlayerList[connectionToClient.connectionId].playerName;

            SetPlayerText(value);
            CmdSetPlayerText(value);
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        // Set torque and speed values according to gear type
        switch (gearType)
        {
            case GearType.Sport:
                _maxTorque = 1300;
                _maxBreakTorqueFront = 2000;
                _maxBreakTorqueRear = 1500;
                _torqueReduction = 80;
                _maxSpeedKMH = 180;
                _maxSpeedBackwardKMH = 20;
                break;
            case GearType.Comfort:
                _maxTorque = 800;
                _maxBreakTorqueFront = 1300;
                _maxBreakTorqueRear = 1000;
                _torqueReduction = 120;
                _maxSpeedKMH = 150;
                _maxSpeedBackwardKMH = 20;
                break;
            default:
                _maxTorque = 900;
                _maxBreakTorqueFront = 1300;
                _maxBreakTorqueRear = 1000;
                _torqueReduction = 120;
                _maxSpeedKMH = 150;
                _maxSpeedBackwardKMH = 20;
                break;

        }

        _currentSpeedKMH = _rigidBody.velocity.magnitude * 3.6f;

        // Evaluate ground under front wheels
        WheelHit hitFL = GetGroundInfos(ref wheelRL, ref _groundTagRL, ref _groundTextureRL);
        WheelHit hitFR = GetGroundInfos(ref wheelRR, ref _groundTagRR, ref _groundTextureRR);
        _carIsOnDrySand = _groundTagRL.CompareTo("Terrain") == 0 && _groundTextureRL == 1;
        _carIsNotOnSand = !(_groundTagRL.CompareTo("Terrain") == 0 && (_groundTextureRL <= 1));

        // Determine if the car is driving forwards or backwards
        bool velocityIsForeward = Vector3.Angle(transform.forward,
        _rigidBody.velocity) < 50f;

        // Determine if the cursor key input means braking
        bool doBraking = _currentSpeedKMH > 0.5f &&
        (Input.GetAxis("Vertical") < 0 && velocityIsForeward ||
        Input.GetAxis("Vertical") > 0 && !velocityIsForeward);

        bool changeGearType = Input.GetButton("GearTypeChange");

        if (!changeGearType)
        {
            _canChangeGearType = true;
        }

        if(changeGearType && _canChangeGearType)
        {
            if(gearType == GearType.Comfort)
            {
                gearType = GearType.Sport;
            } 
            else if(gearType == GearType.Sport)
            {
                gearType = GearType.Comfort;
            }

            _canChangeGearType = false;
        }

        bool carIsSliding = Vector3.Dot(Vector3.Normalize(_rigidBody.velocity), Vector3.Normalize(transform.forward)) < .85f;
        bool doFullBrake = Input.GetKey("space");
        _doSkidmarking = _carIsNotOnSand && (doFullBrake || carIsSliding) && _currentSpeedKMH > 20f;

        SetBrakeSound(_doSkidmarking);
        CmdSetBrakeSound(_doSkidmarking);
        SetSkidmarking(_doSkidmarking);
        CmdSyncSkidmarks(_doSkidmarking);

        if (doBraking || doFullBrake)
        {

            if (doFullBrake)
            {
                bool doSound = doFullBrake && _currentSpeedKMH > 5.0f && _carIsNotOnSand;
                SetBrakeSound(doSound);
                CmdSetBrakeSound(doSound);
                SetBrakeTorque(fullBrakeTorque);
                CmdSetBrakeTorque(fullBrakeTorque);
            }
            else
            {
                if (velocityIsForeward)
                {
                    wheelFL.brakeTorque = -_maxBreakTorqueFront * Input.GetAxis("Vertical");
                    wheelFR.brakeTorque = wheelFL.brakeTorque;
                    wheelRL.brakeTorque = -_maxBreakTorqueRear * Input.GetAxis("Vertical");
                    wheelRR.brakeTorque = wheelRL.brakeTorque;
                }
                else
                {
                    wheelFL.brakeTorque = _maxBreakTorqueFront * Input.GetAxis("Vertical");
                    wheelFR.brakeTorque = wheelFL.brakeTorque;
                    wheelRL.brakeTorque = _maxBreakTorqueRear * Input.GetAxis("Vertical");
                    wheelRR.brakeTorque = wheelRL.brakeTorque;
                }
            }

            SetMotorTorque(0);
            CmdSetMotorTorque(0);
        }
        else
        {
            SetBrakeTorque(0);
            CmdSetBrakeTorque(0);
            float torque = _maxTorque - (_currentGear * _torqueReduction);

            if (thrustEnabled)
            {
                if (velocityIsForeward && _currentSpeedKMH < _maxSpeedKMH)
                {
                    SetMotorTorque(torque * Input.GetAxis("Vertical"));
                    CmdSetMotorTorque(torque * Input.GetAxis("Vertical"));
                }
                else if (!velocityIsForeward && _currentSpeedKMH < _maxSpeedBackwardKMH)
                {
                    SetMotorTorque(torque * Input.GetAxis("Vertical"));
                    CmdSetMotorTorque(torque * Input.GetAxis("Vertical"));
                }
                else
                {
                    SetMotorTorque(0);
                    CmdSetMotorTorque(0);
                }
            }
        }

        float steerReduction = Mathf.Max(1f - _currentSpeedKMH / 50f, .3f);
        float steerAngle = maxSteerAngle * steerReduction * Input.GetAxis("Horizontal");

        SetSteerAngle(steerAngle);
        CmdSetSteerAngle(steerAngle);

        int gearNum = 0;
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum, gearType);
        _currentGear = gearNum;
        SetEngineSound(engineRPM);
        SetRPMEffects(engineRPM);
        CmdSetRPMEffects(engineRPM);

        playerNameText.transform.LookAt(Camera.main.transform);
        playerNameText.transform.Rotate(0,180,0);
    }

    // Gets called from network when local player starts
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        SmoothFollow cam = Camera.main.gameObject.GetComponent<SmoothFollow>();
        if (cam != null)
            cam.target = transform;

        // Set the prefs locally
        Prefs prefs = new Prefs();
        prefs.Load();
        CmdSyncPrefs(prefs);
    }

    void SetMotorTorque(float amount)
    {
        if (!_isInitialized) return;
        wheelFL.motorTorque = amount;
        wheelFR.motorTorque = amount;
    }
    void SetBrakeTorque(float amount)
    {
        if (!_isInitialized) return;
        wheelFL.brakeTorque = amount;
        wheelFR.brakeTorque = amount;
        wheelRL.brakeTorque = amount;
        wheelRR.brakeTorque = amount;
    }
    void SetSteerAngle(float angle)
    {
        if (!_isInitialized) return;
        wheelFL.steerAngle = angle;
        wheelFR.steerAngle = angle;
    }
    void SetBrakeSound(bool doSound)
    {
        if (!_isInitialized) return;
        if (doSound)
        {
            _brakeAudioSource.volume = _currentSpeedKMH / 100.0f;
            _brakeAudioSource.Play();
        }
        else
            _brakeAudioSource.Stop();
    }

    void SetRPMEffects(float engineRPM)
    {
        if (!_isInitialized) return;
        SetParticleSystems(engineRPM);
        SetEngineSound(engineRPM);
    }

    void SetParticleSystems(float engineRMP)
    {
        float smokeRate = engineRMP / 80f;
        _smokeLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);
        _smokeREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);

        float dustRate = 0f;

        if (_currentSpeedKMH > 10f && _carIsOnDrySand)
        {
            dustRate = _currentSpeedKMH * 10f;
        }

        _dustFLEmission.rateOverTime = new ParticleSystem.MinMaxCurve(dustRate);
        _dustFREmission.rateOverTime = new ParticleSystem.MinMaxCurve(dustRate);
        _dustRLEmission.rateOverTime = new ParticleSystem.MinMaxCurve(dustRate);
        _dustRREmission.rateOverTime = new ParticleSystem.MinMaxCurve(dustRate);
    }

    public void SetFriction(float forwardFriction, float sidewaysFriction)
    {
        WheelFrictionCurve f_fwWFC = wheelFL.forwardFriction;
        WheelFrictionCurve f_swWFC = wheelFL.sidewaysFriction;
        f_fwWFC.stiffness = forwardFriction;
        f_swWFC.stiffness = sidewaysFriction;
        wheelFL.forwardFriction = f_fwWFC;
        wheelFL.sidewaysFriction = f_swWFC;
        wheelFR.forwardFriction = f_fwWFC;
        wheelFR.sidewaysFriction = f_fwWFC;
        wheelRL.forwardFriction = f_fwWFC;
        wheelRL.sidewaysFriction = f_swWFC;
        wheelRR.forwardFriction = f_fwWFC;
        wheelRR.sidewaysFriction = f_swWFC;
    }

    void OnGUI()
    {
        playerNameText.text = playerName;

        if (!isLocalPlayer)
            return;

        // Speedpointer rotation
        double degAroundZ = Math.Max(Math.Ceiling(326f - (_currentSpeedKMH * 292 / 140)), maxTachoAngle);
        speedPointerTransform.rotation = Quaternion.Euler(0, 0, (float)degAroundZ);
        // SpeedText show current KMH
        speedText.text = _currentSpeedKMH.ToString("0");
        gearText.text = _currentGear.ToString("0");
    }

    void SetPlayerText(string value)
    {
        playerName = value;
        playerNameText.text = playerName;
    }

    class Gear
    {
        private float _minRPM;
        private float _minKMH;
        private float _maxRPM;
        private float _maxKMH;

        public Gear(float minKMH, float minRPM, float maxKMH, float maxRPM)
        {
            _minRPM = minRPM;
            _minKMH = minKMH;
            _maxRPM = maxRPM;
            _maxKMH = maxKMH;

        }

        public bool speedFits(float kmh)
        {
            return kmh >= _minKMH && kmh <= _maxKMH;
        }

        public float interpolate(float kmh)
        {
            return ((kmh - _minKMH) * (_maxRPM - _minRPM) / (_maxKMH - _minKMH)) + _minRPM;
        }
    }

    float kmh2rpm(float kmh, out int gearNum, GearType gearType)
    {
        Gear[] gears;

        switch (gearType)
        {
            case GearType.Sport:
                gears = sportGears;
                break;
            case GearType.Comfort:
                gears = comfortGears;
                break;
            default:
                gears = comfortGears;
                break;
        }

        for (int i = 0; i < gears.Length; ++i)
        {
            if (gears[i].speedFits(kmh))
            {
                gearNum = i + 1;
                return gears[i].interpolate(kmh);
            }
        }
        gearNum = 1;
        return 800;
    }

    void SetEngineSound(float engineRPM)
    {
        if (_engineAudioSource == null) return;
        float minRPM = 800;
        float maxRPM = 8000;
        float minPitch = 0.3f;
        float maxPitch = 3.0f;
        float pitch = ((engineRPM - minRPM) * (maxPitch - minPitch) / (maxRPM - minRPM)) + minPitch;
        _engineAudioSource.pitch = pitch;
    }

    WheelHit GetGroundInfos(ref WheelCollider wheelCol,
        ref string groundTag,
        ref int groundTextureIndex)
    {
        // Default values
        groundTag = "InTheAir";
        groundTextureIndex = -1;
        // Query ground by ray shoot on the front left wheel collider
        WheelHit wheelHit;
        wheelCol.GetGroundHit(out wheelHit);
        // If not in the air query collider
        if (wheelHit.collider)
        {
            groundTag = wheelHit.collider.tag;
            if (wheelHit.collider.CompareTag("Terrain"))
                groundTextureIndex = TerrainSurface.GetMainTexture(transform.position);
        }

        return wheelHit;
    }

    // Turns skidmarking on or off on all wheels
    void SetSkidmarking(bool doSkidmarking)
    {
        foreach (var wheel in wheelBehaviours)
            wheel.DoSkidmarking(doSkidmarking);
    }

    void OnPrefsChanged(Prefs prefs)
    {
        _prefs = prefs;
        ReapplyPrefs();
    }
    public void ReapplyPrefs()
    {
        _prefs.SetAll(
        ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR, ref bodyBuggy, ref carBehaviour, ref canisters, ref cannon, ref rocketL, ref rocketR);
    }
}
