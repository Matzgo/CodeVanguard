using CodeInspector;
using System.Collections.Generic;
using UnityEngine;

public enum ScenarioType
{
    Crane = 0,
    Safe = 1,
    Power = 2,
}

public abstract class Scenario : MonoBehaviour
{
    protected bool _simulate;
    [SerializeField]
    ButtonInteractable _runButton;

    [SerializeField]
    float _scenarioTickTime;
    public float ScenarioTickTime => _scenarioTickTime;

    public ButtonInteractable RunButton => _runButton;


    protected virtual void Start()
    {
        RuntimeManager.Instance.WorldGameSimulation += OnEval;
    }
    protected virtual void OnDestroy()
    {
        RuntimeManager.Instance.WorldGameSimulation -= OnEval;
    }

    protected virtual void OnEval(bool b)
    {
        _simulate = b;
        ResetSimulator();
    }

    protected abstract void ResetSimulator();

    public abstract (bool b, List<string> feedback) CheckCorrectness();
}
