using System.Collections.Generic;
using UnityEngine;

public class SafeScenario : Scenario
{
    SafeScenarioSimulator _sim;

    [SerializeField]
    GameObject _safeDoor;

    [SerializeField]
    AudioSource _audioSource;

    [SerializeField]
    AudioClip _denySFX;

    [SerializeField]
    AudioClip _validSFX;
    [SerializeField]
    List<GameObject> _insideItems;

    bool _open = false;

    private void Initialize()
    {
        for (int i = 0; i < _insideItems.Count; i++)
        {
            _insideItems[i].SetActive(false);
        }

        _sim = new SafeScenarioSimulator();
    }

    protected override void ResetSimulator()
    {
        _sim.ResetSafe();
    }

    protected override void Start()
    {
        base.Start();
        Initialize();
    }



    public void ResetSafe()
    {
        //_safeDoor.SetActive(true);
    }


    //i could add seperate eval mode for performance checks, so i can still use audio in simulator
    public void OpenSafe()
    {
        if (_simulate)
        {
            _sim.OpenSafe();
            return;
        }

        _open = true;
        _safeDoor.SetActive(false);
        for (int i = 0; i < _insideItems.Count; i++)
        {
            _insideItems[i].SetActive(true);
        }
        _audioSource.PlayOneShot(_validSFX);

    }

    public void SafeAlarm()
    {
        if (_simulate)
        {
            return;
        }

        _audioSource.PlayOneShot(_denySFX);
    }

    public override (bool b, List<string> feedback) CheckCorrectness()
    {
        return _sim.CheckCorrectness();
    }

}
