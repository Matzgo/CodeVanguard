using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    [SerializeField]
    private List<string> scenarioIds; // List of required task IDs

    private HashSet<string> finishedTasks; // Prevent duplicates

    [SerializeField]
    private GameObject winScreen;

    private void Start()
    {
        finishedTasks = new HashSet<string>(); // Initialize to avoid null errors
        winScreen.SetActive(false);
        CodeVanguardManager.Instance.OnTaskEnd += OnTaskEnd;
    }

    private void OnTaskEnd(string id, GradingResult result)
    {
        if (scenarioIds.Contains(id) && result.Correct) // Check if ID is relevant
        {
            finishedTasks.Add(id); // HashSet prevents duplicates
        }

        if (finishedTasks.Count == scenarioIds.Count) // Check if most tasks are done
        {
            StartCoroutine(ShowWinScreenWithDelay(15f)); // Delay showing win screen by 5 seconds
        }
    }

    private IEnumerator ShowWinScreenWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        winScreen.SetActive(true);
        StartCoroutine(HideWinScreenAfterDelay(6f)); // Hide after 6 seconds
    }

    private IEnumerator HideWinScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        winScreen.SetActive(false);
    }
}
