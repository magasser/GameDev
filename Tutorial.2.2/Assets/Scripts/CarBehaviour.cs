using UnityEngine;
using System.Collections;
using TMPro;
using System;

public enum GearType
{
    Comfort,
    Sport
}

public class CarBehaviour : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;
    public GearType gearType;
    public float maxSteerAngle = 45;
    public float forwardFriction;
    public float sidewaysFriction;
    public Transform centerOfMass;
    public float engineVolume;

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

    void Start()
    {
        
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.centerOfMass = new Vector3(centerOfMass.localPosition.x,
                                              centerOfMass.localPosition.y,
                                              centerOfMass.localPosition.z);      

        SetFriction(forwardFriction, sidewaysFriction);

        // Configure AudioSource component by program
        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        _engineAudioSource.clip = engineSingleRPMSoundClip;
        _engineAudioSource.loop = true;
        _engineAudioSource.volume = engineVolume;
        _engineAudioSource.playOnAwake = true;
        _engineAudioSource.enabled = false; // Bugfix
        _engineAudioSource.enabled = true; // Bugfix

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
    }
    void FixedUpdate()
    {
        _currentSpeedKMH = _rigidBody.velocity.magnitude * 3.6f;

        // Determine if the car is driving forwards or backwards
        bool velocityIsForeward = Vector3.Angle(transform.forward,
        _rigidBody.velocity) < 50f;

        // Determine if the cursor key input means braking
        bool doBraking = _currentSpeedKMH > 0.5f &&
        (Input.GetAxis("Vertical") < 0 && velocityIsForeward ||
        Input.GetAxis("Vertical") > 0 && !velocityIsForeward);

        if (doBraking)
        {
            if(velocityIsForeward)
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

            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
        else
        {
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            float torque = _maxTorque - (_currentGear * _torqueReduction);
            if (velocityIsForeward && _currentSpeedKMH < _maxSpeedKMH)
            {
                SetMotorTorque(torque * Input.GetAxis("Vertical"));
            }
            else if (!velocityIsForeward && _currentSpeedKMH < _maxSpeedBackwardKMH)
            {
                SetMotorTorque(torque * Input.GetAxis("Vertical"));
            }
            else
            {
                SetMotorTorque(0);
            }
        }

        float steerReduction = Mathf.Max(1f - _currentSpeedKMH / 50f, .3f);
        float steerAngle = maxSteerAngle * steerReduction * Input.GetAxis("Horizontal");

        SetSteerAngle(steerAngle);

        int gearNum = 0;
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum, gearType);
        _currentGear = gearNum;
        SetEngineSound(engineRPM);
    }

    void SetSteerAngle(float angle)
    {
        wheelFL.steerAngle = angle;
        wheelFR.steerAngle = angle;
    }

    void SetMotorTorque(float amount)
    {
        wheelFL.motorTorque = amount;
        wheelFR.motorTorque = amount;
    }

    void SetFriction(float forewardFriction, float sidewaysFriction)
    {
        WheelFrictionCurve f_fwWFC = wheelFL.forwardFriction;
        WheelFrictionCurve f_swWFC = wheelFL.sidewaysFriction;
        f_fwWFC.stiffness = forewardFriction;
        f_swWFC.stiffness = sidewaysFriction;
        wheelFL.forwardFriction = f_fwWFC;
        wheelFL.sidewaysFriction = f_swWFC;
        wheelFR.forwardFriction = f_fwWFC;
        wheelRL.forwardFriction = f_fwWFC;
        wheelRL.sidewaysFriction = f_swWFC;
        wheelRR.forwardFriction = f_fwWFC;
        wheelRR.sidewaysFriction = f_swWFC;
    }

    void OnGUI()
    {
        // Speedpointer rotation
        double degAroundZ = Math.Ceiling(326f - (_currentSpeedKMH * 292 / 140));
        speedPointerTransform.rotation = Quaternion.Euler(0, 0, (float)degAroundZ);
        // SpeedText show current KMH
        speedText.text = _currentSpeedKMH.ToString("0");
        gearText.text = _currentGear.ToString("0");
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
                gears = new Gear[]
                {
                    new Gear(1, 700, 40, 5200),
                    new Gear(40, 3000, 72, 5600),
                    new Gear(72, 3800, 110, 6100),
                    new Gear(110, 4700, 151, 6500),
                    new Gear(151, 5300, 197, 7100),
                    new Gear(197, 5400, 180, 8000)
                };
                break;
            case GearType.Comfort:
                gears = new Gear[]
                {
                    new Gear(1, 900, 12, 1400),
                    new Gear(12, 900, 25, 2000),
                    new Gear(25, 1350, 45, 2500),
                    new Gear(45, 1950, 70, 3500),
                    new Gear(70, 2500, 112, 4000),
                    new Gear(112, 3100, 180, 5000)
                };
                break;
            default:
                gears = new Gear[]
                {
                    new Gear( 1, 900, 12, 1400),
                    new Gear( 12, 900, 25, 2000),
                    new Gear( 25, 1350, 45, 2500),
                    new Gear( 45, 1950, 70, 3500),
                    new Gear( 70, 2500, 112, 4000),
                    new Gear(112, 3100, 180, 5000)
                };
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
}
