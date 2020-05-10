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

    public Slider suspDistanceSlider;
    public Text distanceText;

    private Prefs _prefs;

    void Start()
    {
        _prefs = new Prefs();
        _prefs.Load();
        _prefs.SetAll(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);

        if (SceneManager.GetActiveScene().name == "SceneMenu")
        {
            suspDistanceSlider.value = _prefs.suspensionDistance;
            distanceText.text = suspDistanceSlider.value.ToString("0.00");
        }
    }


    public void OnSliderChangedSuspDistance(float distance)
    {
        distanceText.text = suspDistanceSlider.value.ToString("0.00");
        _prefs.suspensionDistance = suspDistanceSlider.value;
        _prefs.SetWheelColliderSuspension(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
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
}
