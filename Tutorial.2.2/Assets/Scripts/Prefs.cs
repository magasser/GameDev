using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Prefs
{
    public float suspensionDistance;
    public int spring;
    public int damper;

    public float forwardFriction;
    public float sidewaysFriction;

    public float hue;
    public float saturation;
    public float value;

    public int canistersEnabled;
    public int cannonEnabled;
    public int rocketLEnabled;
    public int rocketREnabled;

    public void Load()
    {
        suspensionDistance = PlayerPrefs.GetFloat("suspensionDistance", 0.2f);
        spring = PlayerPrefs.GetInt("spring", 35000);
        damper = PlayerPrefs.GetInt("damper", 4500);

        forwardFriction = PlayerPrefs.GetFloat("forwardFriction", 2f);
        sidewaysFriction = PlayerPrefs.GetFloat("sidewaysFriction", .8f);


        hue = PlayerPrefs.GetFloat("hue", 0f);
        saturation = PlayerPrefs.GetFloat("saturation", 0f);
        value = PlayerPrefs.GetFloat("value", 0.91f);

        canistersEnabled = PlayerPrefs.GetInt("canistersEnabled", 0);
        cannonEnabled = PlayerPrefs.GetInt("cannonEnabled", 0);
        rocketLEnabled = PlayerPrefs.GetInt("rocketLEnabled", 0);
        rocketREnabled = PlayerPrefs.GetInt("rocketREnabled", 0);

    }
    public void Save()
    {
        PlayerPrefs.SetFloat("suspensionDistance", suspensionDistance);
        PlayerPrefs.SetInt("spring", spring);
        PlayerPrefs.SetInt("damper", damper);

        PlayerPrefs.SetFloat("forwardFriction", forwardFriction);
        PlayerPrefs.SetFloat("sidewaysFriction", sidewaysFriction);

        PlayerPrefs.SetFloat("hue", hue);
        PlayerPrefs.SetFloat("saturation", saturation);
        PlayerPrefs.SetFloat("value", value);

        PlayerPrefs.SetInt("canistersEnabled", canistersEnabled);
        PlayerPrefs.SetInt("cannonEnabled", cannonEnabled);
        PlayerPrefs.SetInt("rocketLEnabled", rocketLEnabled);
        PlayerPrefs.SetInt("rocketREnabled", rocketREnabled);
    }

    public void SetAll(ref WheelCollider wheelFL, ref WheelCollider wheelFR,
        ref WheelCollider wheelRL, ref WheelCollider wheelRR, ref MeshRenderer mr,
        ref CarBehaviour cb, ref MeshRenderer canisters, ref MeshRenderer cannon,
        ref MeshRenderer rocketL, ref MeshRenderer rocketR)
    {
        SetMaterialColor(ref mr);
        SetWheelColliderSuspension(ref wheelFL, ref wheelFR,
            ref wheelRL, ref wheelRR);
        SetFriction(ref wheelFL, ref wheelFR,
            ref wheelRL, ref wheelRR, ref cb);
        SetVisibleComponents(ref canisters, ref cannon, ref rocketL, ref rocketR);
    }

    public void SetAll(ref WheelCollider wheelFL, ref WheelCollider wheelFR,
        ref WheelCollider wheelRL, ref WheelCollider wheelRR, ref MeshRenderer mr,
        ref CarBehaviourNetwork cb, ref MeshRenderer canisters, ref MeshRenderer cannon,
        ref MeshRenderer rocketL, ref MeshRenderer rocketR)
    {
        SetMaterialColor(ref mr);
        SetWheelColliderSuspension(ref wheelFL, ref wheelFR,
            ref wheelRL, ref wheelRR);
        SetFriction(ref wheelFL, ref wheelFR,
            ref wheelRL, ref wheelRR, ref cb);
        SetVisibleComponents(ref canisters, ref cannon, ref rocketL, ref rocketR);
    }

    public void SetMaterialColor(ref MeshRenderer mr)
    {
        mr.materials[0].SetColor("_Color", Color.HSVToRGB(hue, saturation, value, false));
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

        JointSpring js = new JointSpring {spring = spring, damper = damper, targetPosition = .5f};

        wheelFL.suspensionSpring = js;
        wheelFR.suspensionSpring = js;
        wheelRL.suspensionSpring = js;
        wheelRR.suspensionSpring = js;
    }

    public void SetFriction(ref WheelCollider wheelFL, ref WheelCollider wheelFR, ref WheelCollider wheelRL, ref WheelCollider wheelRR, ref CarBehaviour cb)
    {
        cb.SetFriction(forwardFriction, sidewaysFriction);
    }

    public void SetFriction(ref WheelCollider wheelFL, ref WheelCollider wheelFR, ref WheelCollider wheelRL, ref WheelCollider wheelRR, ref CarBehaviourNetwork cb)
    {
        cb.SetFriction(forwardFriction, sidewaysFriction);
    }

    public void SetVisibleComponents(ref MeshRenderer canisters, ref MeshRenderer cannon, ref MeshRenderer rocketL,
        ref MeshRenderer rocketR)
    {
        canisters.enabled = Convert.ToBoolean(canistersEnabled);
        cannon.enabled = Convert.ToBoolean(cannonEnabled);
        rocketL.enabled = Convert.ToBoolean(rocketLEnabled);
        rocketR.enabled = Convert.ToBoolean(rocketREnabled);
    }
}
