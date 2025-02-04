using System.Collections.Generic;
using UnityEngine;

public class PowerScenario : Scenario
{
    PowerScenarioSimulator _sim;
    [SerializeField]
    GameObject _blueBeam;
    [SerializeField]
    GameObject _blueBeam2;
    bool _isBeamReady = true;
    bool _wasBeamRedirected = false;
    bool _valid = true;
    int cntr;
    // Flag to track the detected redirection pattern
    bool _redirectEvenBeams;
    // Flag to track if the pattern has been determined
    bool _patternDetermined = false;
    private int _redirectCounter;
    private int _nonRedirectCounter;
    private bool isFirstBeam;
    [SerializeField]
    AudioSource _audioSrc;
    [SerializeField]
    AudioClip _beamSfx;
    [SerializeField]
    AudioClip _redirectSfx;
    [SerializeField]
    AudioClip _startClip;
    [SerializeField]
    AudioClip _stallClip;

    protected override void Start()
    {
        base.Start();
        Initialize();
    }

    public void ResetScenario()
    {
        Initialize();
    }

    public void Initialize()
    {
        cntr = 0;
        _sim = new PowerScenarioSimulator();
        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);
        _isBeamReady = true;
        _wasBeamRedirected = false;
        _valid = true;
        _patternDetermined = false;
        isFirstBeam = true;
        _redirectCounter = 0;
        _nonRedirectCounter = 0;
    }

    public void FireBeam()
    {
        if (_simulate)
        {
            _sim.FireBeam();
            return;
        }

        // Invalid if two consecutive beam redirections 
        if (_redirectCounter > 1)
        {
            _valid = false;
        }

        // Invalid if more than two consecutive non-redirected beams
        if (_nonRedirectCounter > 1)
        {
            _valid = false;
        }

        if (!isFirstBeam)
        {
            // Reset counters based on previous beam state
            if (_wasBeamRedirected)
            {
                _nonRedirectCounter = 0;
                _redirectCounter++;
            }
            else
            {
                _nonRedirectCounter++;
                _redirectCounter = 0;
            }
        }
        isFirstBeam = false;
        // Reset beam states
        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);
        _audioSrc.PlayOneShot(_beamSfx);
        // Activate the blue beam
        _blueBeam.SetActive(true);
        // Reset redirect state for the new beam
        _wasBeamRedirected = false;
        cntr++;
    }

    public void RedirectBeam()
    {
        if (_simulate)
        {
            _sim.RedirectBeam();
            return;
        }

        _audioSrc.PlayOneShot(_redirectSfx);

        // Mark the current beam as redirected
        _wasBeamRedirected = true;
        _blueBeam2.SetActive(true);
    }

    public void GeneratorStart()
    {
        if (_simulate)
        {
            _sim.GeneratorStart();
            return;
        }


        // Invalid if two consecutive beam redirections 
        if (_redirectCounter > 1)
        {
            _valid = false;
        }

        // Invalid if more than two consecutive non-redirected beams
        if (_nonRedirectCounter > 1)
        {
            _valid = false;
        }


        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);
        // Ensure pattern was determined before allowing generator to start
        if (_valid)
        {
            _audioSrc.PlayOneShot(_startClip);

        }
        else
        {
            _audioSrc.PlayOneShot(_stallClip);

        }
    }

    public override (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness()
    {
        return _sim.CheckCorrectness();
    }

    protected override void ResetSimulator()
    {
        _sim.Reset();
    }
}