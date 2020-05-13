using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;

    public MeshRenderer canisters;
    public MeshRenderer cannon;
    public MeshRenderer rocketL;
    public MeshRenderer rocketR;

    public MeshRenderer meshRenderer;

    public CarBehaviour carBehaviour;

    // Suspension
    public Slider suspDistanceSlider;
    public Text distanceText;
    public Slider springSlider;
    public Text springText;
    public Slider damperSlider;
    public Text damperText;

    // Friction
    public Slider forwardFrictionSlider;
    public Text forwardFrictionText;
    public Slider sidewaysFrictionSlider;
    public Text sidewaysFrictionText;

    // Color
    public Slider hueSlider;
    public Text hueText;
    public Slider saturationSlider;
    public Text saturationText;
    public Slider valueSlider;
    public Text valueText;

    public Toggle canistersToggle;
    public Toggle cannonToggle;
    public Toggle rocketLToggle;
    public Toggle rocketRToggle;

    private Prefs _prefs;

    void Start()
    {
        _prefs = new Prefs();
        _prefs.Load();
        _prefs.SetAll(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR, ref meshRenderer, ref carBehaviour, ref canisters, ref cannon, ref rocketL, ref rocketR);

        if (SceneManager.GetActiveScene().name == "SceneMenu")
        {
            suspDistanceSlider.value = _prefs.suspensionDistance;
            distanceText.text = suspDistanceSlider.value.ToString("0.00");
            springSlider.value = _prefs.spring;
            springText.text = springSlider.value.ToString("0");
            damperSlider.value = _prefs.damper;
            damperText.text = damperSlider.value.ToString("0");

            forwardFrictionSlider.value = _prefs.forwardFriction;
            forwardFrictionText.text = forwardFrictionSlider.value.ToString("0.00");
            sidewaysFrictionSlider.value = _prefs.sidewaysFriction;
            sidewaysFrictionText.text = sidewaysFrictionSlider.value.ToString("0.00");

            hueSlider.value = _prefs.hue;
            hueText.text = hueSlider.value.ToString("0.00");
            saturationSlider.value = _prefs.saturation;
            saturationText.text = saturationSlider.value.ToString("0.00");
            valueSlider.value = _prefs.value;
            valueText.text = valueSlider.value.ToString("0.00");

            cannonToggle.isOn = Convert.ToBoolean(_prefs.cannonEnabled);
            canistersToggle.isOn = Convert.ToBoolean(_prefs.canistersEnabled);
            rocketLToggle.isOn = Convert.ToBoolean(_prefs.rocketLEnabled);
            rocketRToggle.isOn = Convert.ToBoolean(_prefs.rocketREnabled);
        }
    }

    void OnApplicationQuit()
    {
        _prefs.Save();
    }

    public void OnStartClick()
    {
        _prefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    public void OnMenuClick()
    {
        SceneManager.LoadScene("SceneMenu");
    }

    public void OnSliderChangedSuspDistance(float distance)
    {
        distanceText.text = suspDistanceSlider.value.ToString("0.00");
        _prefs.suspensionDistance = suspDistanceSlider.value;
        _prefs.SetWheelColliderSuspension(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
    }

    public void OnSliderChangedSpring(float spring)
    {
        springText.text = springSlider.value.ToString("0");
        _prefs.spring = (int)springSlider.value;
        _prefs.SetWheelColliderSuspension(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
    }

    public void OnSliderChangedDamper(float damper)
    {
        damperText.text = damperSlider.value.ToString("0");
        _prefs.damper = (int)damperSlider.value;
        _prefs.SetWheelColliderSuspension(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
    }

    public void OnSliderChangedForwardFriction(float forwardFriction)
    {
        forwardFrictionText.text = forwardFrictionSlider.value.ToString("0.00");
        _prefs.forwardFriction = forwardFrictionSlider.value;
        _prefs.SetFriction(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR, ref carBehaviour);
    }

    public void OnSliderChangedSidewaysFriction(float sidewaysFriction)
    {
        sidewaysFrictionText.text = sidewaysFrictionSlider.value.ToString("0.00");
        _prefs.sidewaysFriction = sidewaysFrictionSlider.value;
        _prefs.SetFriction(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR, ref carBehaviour);
    }

    public void OnSliderChangedHue(float hue)
    {
        hueText.text = hueSlider.value.ToString("0.00");
        _prefs.hue = hueSlider.value;
        _prefs.SetMaterialColor(ref meshRenderer);
    }

    public void OnSliderChangedSaturation(float saturation)
    {
        saturationText.text = saturationSlider.value.ToString("0.00");
        _prefs.saturation = saturationSlider.value;
        _prefs.SetMaterialColor(ref meshRenderer);
    }

    public void OnSliderChangedValue(float value)
    {
        valueText.text = valueSlider.value.ToString("0.00");
        _prefs.value = valueSlider.value;
        _prefs.SetMaterialColor(ref meshRenderer);
    }

    public void OnToggleCanisters(bool canistersB)
    {
        _prefs.canistersEnabled = Convert.ToInt32(canistersB);
        _prefs.SetVisibleComponents(ref this.canisters, ref cannon, ref rocketL, ref rocketR);
    }

    public void OnToggleCannon(bool cannonB)
    {
        _prefs.cannonEnabled = Convert.ToInt32(cannonB);
        _prefs.SetVisibleComponents(ref canisters, ref cannon, ref rocketL, ref rocketR);
    }

    public void OnToggleRocketL(bool rocketLB)
    {
        _prefs.rocketLEnabled = Convert.ToInt32(rocketLB);
        _prefs.SetVisibleComponents(ref canisters, ref cannon, ref rocketL, ref rocketR);
    }

    public void OnToggleRocketR(bool rocketRB)
    {
        _prefs.rocketREnabled = Convert.ToInt32(rocketRB);
        _prefs.SetVisibleComponents(ref canisters, ref cannon, ref rocketL, ref rocketR);
    }
}
