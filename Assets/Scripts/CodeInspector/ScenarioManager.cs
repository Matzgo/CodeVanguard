using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }

    [SerializeField]
    private CraneScenario _craneScenario;

    [SerializeField]
    private SafeScenario _safeScenario;
    [SerializeField]
    private PowerScenario _powerScenario;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ScenarioManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Ensures the singleton persists across scenes if needed
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Scenario GetScenario(ScenarioType type)
    {
        switch (type)
        {
            case ScenarioType.Crane:
                return _craneScenario;

            case ScenarioType.Safe:
                return _safeScenario;
            case ScenarioType.Power:
                return _powerScenario;
        }
        Debug.LogError("INVALID ScenarioType");
        return null;
    }

}
