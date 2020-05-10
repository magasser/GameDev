using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs
{
    public float suspensionDistance;
    public void Load()
    {
        suspensionDistance = PlayerPrefs.GetFloat("suspensionDistance", 0.2f);
    }
    public void Save()
    {
        PlayerPrefs.SetFloat("suspensionDistance", suspensionDistance);
    }
    public void SetAll(ref WheelCollider wheelFL, ref WheelCollider wheelFR,
        ref WheelCollider wheelRL, ref WheelCollider wheelRR)
    {
        SetWheelColliderSuspension(ref wheelFL, ref wheelFR,
            ref wheelRL, ref wheelRR);
    }
    public void SetWheelColliderSuspension(ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        wheelFL.suspensionDistance = suspensionDistance;
        wheelFR.suspensionDistance = suspensionDistance;
        wheelRL.suspensionDistance = suspensionDistance; 
        wheelRR.suspensionDistance = suspensionDistance;
    }
}
