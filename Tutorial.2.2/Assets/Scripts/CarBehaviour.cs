using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public Rigidbody rigidBody;
    public float maxTorque = 500;
    public float maxSteerAngle = 45;
    public float maxSpeedKMH = 150;
    public float maxSpeedBackwardKMH = 30;
    private float _currentSpeedKMH;

    void Start() { }

    void FixedUpdate()
    {

        _currentSpeedKMH = rigidBody.velocity.magnitude * 3.6f;
        Debug.Log(_currentSpeedKMH);

        if (_currentSpeedKMH <= maxSpeedKMH)
        {
            SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        }
        else
        {
            SetMotorTorque(0);
        }

        SetSteerAngle((maxSteerAngle / maxSpeedKMH * _currentSpeedKMH) * Input.GetAxis("Horizontal"));
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
}