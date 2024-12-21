using CodeInspector;
using TMPro;
using UnityEngine;

public class TaskScreen : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _taskType;

    [SerializeField]
    TextMeshProUGUI _taskDescription;

    public void LoadTask(CSFileSet task)
    {
        _taskType.text = "OBJECTIVE";
        _taskDescription.text = task.Description;

    }

    internal void LoadFeedback(GradingResult res)
    {
        _taskType.text = "FEEDBACK";
        if (res.Feedback.Count > 0)
        {
            // Join feedback strings with double newlines
            string feedbackText = string.Join("\n\n", res.Feedback);
            // Update the task description with feedback
            _taskDescription.text = feedbackText;
        }
        else
        {
            // Handle case where there is no feedback
            _taskDescription.text = "No feedback provided.";
        }
    }
}
