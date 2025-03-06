using UnityEngine;

public class TerminalPlugInScreen : MonoBehaviour
{
    private RuntimeCodeSystem _rtCodeSystem;
    [SerializeField] private GameObject screen; // Assign this in Inspector

    [SerializeField] private ScenarioType targetTaskType; // Assign this in Inspector

    void Start()
    {
        _rtCodeSystem = CodeVanguardManager.Instance.RuntimeCodeSystem;
        InvokeRepeating(nameof(CheckTaskType), 1f, 1f); // Runs every second
    }

    void CheckTaskType()
    {
        if (CodeVanguardManager.Instance.CurrentState == CodeVanguardState.Task)
        {
            if (_rtCodeSystem != null && _rtCodeSystem.CurrentTask != null)
            {
                if (_rtCodeSystem.CurrentTask.ScenarioType == targetTaskType)
                {


                    //Debug.Log("Matching TaskType found!");
                    screen.SetActive(false);
                    return;
                }
            }
        }

        screen.SetActive(true);

    }
}
