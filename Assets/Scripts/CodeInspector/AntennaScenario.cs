using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct AntennaMiniGameParameters
{
    public List<int> startingArray;
    public List<int> targetArray;

}
public class AntennaScenario : Scenario
{
    AntennaScenarioSimulator _sim;
    AntennaMiniGameParameters _parameters;

    [SerializeField]
    AntennaMiniGameParameters _awakeParams;
    public AntennaMiniGameParameters AwakeParams => _awakeParams;

    [SerializeField]
    TMPro.TextMeshProUGUI _numberText;

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
        _sim = new AntennaScenarioSimulator();
        _sim.Initialize(_awakeParams);
        // Convert startingArray to a space-separated string and assign it to the TextMeshPro UI element
        if (_numberText != null)
        {
            _numberText.text = string.Join(" ", _awakeParams.startingArray);
        }
    }

    public override (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness()
    {
        return (false, null, null);
    }

    public (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness(List<int> result)
    {
        if (_numberText != null)
        {
            _numberText.text = string.Join(" ", result);
        }

        return _sim.CheckCorrectness(result);
    }

    protected override void ResetSimulator()
    {
        _sim.Reset(_awakeParams);
    }

}