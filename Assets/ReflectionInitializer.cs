using Game;
using RoslynCSharp;
using System;
using UnityEngine;

public class ReflectionInitializer : MonoBehaviour
{
    [SerializeField]
    RuntimeCodeSystem _runtimeCodeSystem;

    private void Awake()
    {
        _runtimeCodeSystem.OnReflectionInitialize += HandleReflectionInitialization;
    }

    private void OnDestroy()
    {
        _runtimeCodeSystem.OnReflectionInitialize -= HandleReflectionInitialization;

    }


    private void HandleReflectionInitialization(ScriptProxy scriptProxy, ScenarioType scenarioType, Scenario scenario)
    {
        switch (scenarioType)
        {
            case ScenarioType.Crane:
                HandleCraneReflection(scriptProxy);
                break;
            case ScenarioType.Safe:
                HandleSafeReflection(scriptProxy);
                break;
            case ScenarioType.Power:
                HandlePowerReflection(scriptProxy);
                break;
            case ScenarioType.Antenna:
                break;
        }
    }

    internal void HandleCraneReflection(ScriptProxy scriptProxy)
    {
        try
        {
            // Verify the script type first
            Debug.Log($"Script Type: {scriptProxy.ScriptType}");

            // Try setting the field using the proxy's field access
            var crane = new Crane();
            scriptProxy.Fields["crane"] = crane;

            Debug.Log("Successfully set crane field!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting crane field: {ex.Message}");
        }
    }



    internal void HandleSafeReflection(ScriptProxy scriptProxy)
    {
        try
        {
            // Verify the script type first
            Debug.Log($"Script Type: {scriptProxy.ScriptType}");

            // Try setting the field using the proxy's field access
            var safe = new Safe();
            scriptProxy.Fields["safe"] = safe;

            Debug.Log("Successfully set safe field!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting safe field: {ex.Message}");
        }
    }

    internal void HandlePowerReflection(ScriptProxy scriptProxy)
    {
        try
        {
            // Verify the script type first
            Debug.Log($"Script Type: {scriptProxy.ScriptType}");

            // Try setting the field using the proxy's field access
            var generator = new Generator();
            scriptProxy.Fields["generator"] = generator;

            Debug.Log("Successfully set generator field!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting generator field: {ex.Message}");
        }
    }
}
