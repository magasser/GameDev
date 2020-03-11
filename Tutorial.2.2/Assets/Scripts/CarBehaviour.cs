using UnityEngine;
using System.Collections;
public class CarBehaviour : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public float maxTorque = 500;
    public float maxSteerAngle = 45;
    void Start() { }
    void FixedUpdate()
    {
        SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        SetSteerAngle(maxSteerAngle * Input.GetAxis("Horizontal"));
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
