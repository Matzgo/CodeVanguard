using CodeInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class FeedbackBotBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _player; // The player GameObject to follow
    [SerializeField] private float stopDistance = 1f; // Distance to stop from the player
    private NavMeshAgent _agent; // Reference to the NavMeshAgent


    List<List<string>> _feedbackQueue;

    bool _isShowingFeedback;

    [SerializeField]
    CSFileSet _currentTask;

    [SerializeField]
    TextMeshProUGUI _taskType;

    [SerializeField]
    TextMeshProUGUI _taskDescription;

    [SerializeField]
    GameObject _robotFace;

    [SerializeField]
    TMPro.TextMeshProUGUI _validText;

    [SerializeField]
    TMPro.TextMeshProUGUI _perfScore;

    [SerializeField]
    TMPro.TextMeshProUGUI _memScore;


    [SerializeField]
    TMPro.TextMeshProUGUI _maintainScore;

    [SerializeField]
    GameObject _taskInfo;
    private bool _isShowingTask;
    [SerializeField]
    private GameObject _feedbackIndicator;
    [SerializeField]
    AudioSource _audio;

    [SerializeField]
    FeedbackBot _feedbackBot;
    public void LoadTask(CSFileSet task)
    {
        _currentTask = task;
        //_feedbackIndicator.SetActive(true);
        //_audio.Play();
    }




    void Start()
    {
        _feedbackIndicator.SetActive(false);
        _feedbackQueue = new List<List<string>>();
        // Get the NavMeshAgent component on this GameObject
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("No NavMeshAgent component found on the FeedbackBot!");
        }

        if (_player == null)
        {
            Debug.LogError("Player GameObject not assigned in the inspector!");
        }
    }

    public void QueueFeedback(GradingResult res)
    {
        _feedbackIndicator.SetActive(true);
        _feedbackQueue.Add(res.Feedback);
        _validText.text = res.Correct ? "VALID" : "INVALID";
        _perfScore.text = res.PerformanceScore.ToString("F0");
        _memScore.text = res.MemoryScore.ToString("F0");
        _maintainScore.text = res.NamingScore.ToString("F0");
        _feedbackBot.LoadFeedback(res.FeedbackKeys);
        _audio.Play();

    }

    public bool CanInteract()
    {
        return _feedbackQueue.Count > 0 || _isShowingFeedback;
    }

    public void OnInteract()
    {
        if (_feedbackQueue.Count > 0)
        {

            if (!_isShowingFeedback)
            {
                ShowNextFeedback();
                _isShowingFeedback = true;
            }
            else
            {
                HideFeedback();
                ShowNextFeedback();
                _isShowingFeedback = true;
            }
        }
        else if (_isShowingFeedback)
        {
            HideFeedback();
            _isShowingFeedback = false;
        }
        _feedbackIndicator.SetActive(false);

    }


    private void HideFeedback()
    {
        _taskInfo.SetActive(false);
        _robotFace.SetActive(true);
    }

    private void ShowNextFeedback()
    {
        if (_feedbackQueue.Count == 0)
            return;


        _taskInfo.SetActive(true);
        _robotFace.SetActive(false);

        var feedback = _feedbackQueue[0];
        _taskType.text = $"{_currentTask.Title} FEEDBACK";
        if (feedback.Count > 0)
        {
            // Join feedback strings with double newlines
            string feedbackText = string.Join("\n\n", feedback);
            // Update the task description with feedback
            _taskDescription.text = feedbackText;
        }
        else
        {
            // Handle case where there is no feedback
            _taskDescription.text = "No feedback provided.";
        }
        _feedbackQueue.RemoveAt(0);
    }

    void Update()
    {
        if (_player == null || _agent == null) return;

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer > stopDistance)
        {
            // Move towards the player
            _agent.SetDestination(_player.transform.position);
        }
        else
        {
            // Stop moving
            _agent.ResetPath();
        }
    }


}
