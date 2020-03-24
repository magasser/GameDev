using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class CarBehaviour : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;
    public float maxTorque = 500;
    public float maxSteerAngle = 45;
    public float maxSpeedKMH = 150;
    public float maxSpeedBackwardKMH = 30;
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
            wheelFL.brakeTorque = 1500;
            wheelFR.brakeTorque = 1500;
            wheelRL.brakeTorque = 1000;
            wheelRR.brakeTorque = 1000;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
        else
        {
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;

            if (velocityIsForeward && _currentSpeedKMH < maxSpeedKMH)
            {
                SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
            }
            else if (!velocityIsForeward && _currentSpeedKMH < maxSpeedBackwardKMH)
            {
                SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
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
        float engineRPM = kmh2rpm(_currentSpeedKMH, out gearNum);
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
            return Math.Max(kmh * _maxRPM / _maxKMH, _minRPM);
        }
    }

    float kmh2rpm(float kmh, out int gearNum)
    {
        Gear[] gears =
        {
            new Gear( 1, 900, 12, 1400),
            new Gear( 12, 900, 25, 2000),
            new Gear( 25, 1350, 45, 2500),
            new Gear( 45, 1950, 70, 3500),
            new Gear( 70, 2500, 112, 4000),
            new Gear(112, 3100, 180, 5000)
        };
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
        float pitch = Math.Max(engineRPM * maxPitch / maxRPM, minPitch);
        _engineAudioSource.pitch = pitch;
    }
}
