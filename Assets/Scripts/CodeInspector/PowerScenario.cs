using System.Collections.Generic;
using UnityEngine;

public class PowerScenario : Scenario
{
    PowerScenarioSimulator _sim;

    [SerializeField]
    GameObject _redBeam;
    [SerializeField]
    GameObject _redBeam2;
    [SerializeField]
    GameObject _blueBeam;
    [SerializeField]
    GameObject _blueBeam2;

    bool isRedirecting;
    bool isRed;

    bool _redirectedAnyRed;
    bool _redirectedAllBlue;

    int cntr;

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
        _redirectedAllBlue = true;
        _redirectedAnyRed = false;
        _sim = new PowerScenarioSimulator();
        _redBeam.SetActive(false);
        _redBeam2.SetActive(false);
        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);
    }



    public void FireBeam()
    {

        if (_simulate)
        {
            _sim.FireBeam();
            return;
        }



        if (!isRedirecting && !isRed)
        {
            _redirectedAllBlue = false;

        }
        _redBeam.SetActive(false);
        _redBeam2.SetActive(false);
        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);

        isRedirecting = false;

        isRed = cntr % 2 == 1;
        if (isRed)
        {
            _redBeam.SetActive(true);
            //_redBeam2.SetActive(true);
        }
        else
        {
            _blueBeam.SetActive(true);
            //_blueBeam2.SetActive(true);
        }



        cntr++;
    }


    public void GeneratorStart()
    {
        if (_simulate)
        {
            _sim.GeneratorStart();
        }

        if (!isRedirecting && !isRed)
        {
            _redirectedAllBlue = false;
        }

        _redBeam.SetActive(false);
        _redBeam2.SetActive(false);
        _blueBeam.SetActive(false);
        _blueBeam2.SetActive(false);

        if (_redirectedAllBlue && !_redirectedAnyRed)
        {
            //TODO: Start Generator
            Debug.Log("SUCC");
        }
        else
        {
            //TODO: Stall Generator
        }
    }

    public void RedirectBeam()
    {
        if (_simulate)
        {
            _sim.BlockBeam();
            return;
        }

        if (isRedirecting)
            return;

        if (isRed)
        {
            _redBeam2.SetActive(true);
            _redirectedAnyRed = true;
        }
        else
        {
            _blueBeam2.SetActive(true);

        }

        isRedirecting = true;
    }


    public override (bool b, List<string> feedback) CheckCorrectness()
    {
        return _sim.CheckCorrectness();
    }

    protected override void ResetSimulator()
    {
        _sim.Reset();
    }
}
