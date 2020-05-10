using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimingBehaviour : MonoBehaviour
{
    public int countMax = 3;
    private int _countDown;

    public TMP_Text timeText;

    private CarBehaviour _carScript;

    public AudioClip countdownSoundClip;
    private AudioSource _countdownAudioSource;
    public float countdownVolume;

    private float _pastTime = 0;
    private bool _isFinished = false;
    private bool _isStarted = false;

    // Use this for initialization
    void Start()
    {
        _carScript = GameObject.Find("Buggy").GetComponent<CarBehaviour>();
        _carScript.thrustEnabled = false;


        _countdownAudioSource = gameObject.AddComponent<AudioSource>();
        _countdownAudioSource.clip = countdownSoundClip;
        _countdownAudioSource.volume = countdownVolume;
        _countdownAudioSource.pitch = .8f;
        _countdownAudioSource.enabled = false; // Bugfix
        _countdownAudioSource.enabled = true; // Bugfix

        StartCoroutine(GameStart());
    }
    // GameStart CoRoutine
    IEnumerator GameStart()
    {
        for (_countDown = countMax; _countDown > 0; _countDown--)
        {
            _countdownAudioSource.Play();
            yield return new WaitForSeconds(1);
        }

        _countdownAudioSource.pitch = 1.5f;
        _countdownAudioSource.Play();
        timeText.text = "0";
        _carScript.thrustEnabled = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        {
            if (!_isStarted)
                _isStarted = true;
            else _isFinished = true;
        }
    }
    void OnGUI()
    {
        if (_carScript.thrustEnabled)
        {
            if (_isStarted && !_isFinished)
                _pastTime += Time.deltaTime;
            timeText.text = _pastTime.ToString("0.0");
        }
        else
            timeText.text = _countDown.ToString();
    }
}
