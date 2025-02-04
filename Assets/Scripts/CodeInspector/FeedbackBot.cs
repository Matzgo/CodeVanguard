using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackBot : MonoBehaviour
{
    [SerializeField]
    FeedbackDatabase _feedbackDb;
    [SerializeField]
    AudioSource _audioSource;
    [SerializeField]
    GameObject _dialogueBox;
    [SerializeField]
    TMPro.TextMeshProUGUI _dialogueText;

    [SerializeField]
    List<string> _onStartMessages;

    private Queue<List<string>> _feedbackQueue = new Queue<List<string>>();
    private bool _isPlayingFeedback = false;

    [SerializeField]
    bool _tutorialPlayback = true;

    private void Awake()
    {
        _dialogueBox.SetActive(false);
    }
    private void Start()
    {
        if (_tutorialPlayback)
            StartCoroutine(LoadWelcomeAfterDelay(3f));
    }

    private IEnumerator LoadWelcomeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadFeedback(_onStartMessages);
    }


    // Call this method to load feedback
    public void LoadFeedback(List<string> feedbackKey)
    {
        _feedbackQueue.Enqueue(feedbackKey);
        if (!_isPlayingFeedback)
        {
            StartCoroutine(PlayFeedback());
        }
    }

    private IEnumerator PlayFeedback()
    {
        _isPlayingFeedback = true;

        while (_feedbackQueue.Count > 0)
        {
            List<string> feedback = _feedbackQueue.Dequeue();

            foreach (string key in feedback)
            {
                FeedbackData feedbackData = _feedbackDb.GetFeedback(key);
                if (feedbackData != null)
                {
                    // Set the dialogue text
                    _dialogueText.text = feedbackData.feedbackText;

                    // Play the audio clip
                    if (feedbackData.audioClip != null)
                    {
                        _audioSource.PlayOneShot(feedbackData.audioClip);
                    }

                    // Show the dialogue box
                    _dialogueBox.SetActive(true);

                    // Wait for the audio to finish playing
                    yield return new WaitForSeconds(feedbackData.audioClip.length + feedbackData.postDelay + .25f); // Wait for the clip duration plus extra seconds

                    // Hide the dialogue box
                    _dialogueBox.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"No feedback entry found for key: {key}");
                }
            }
        }

        _isPlayingFeedback = false;
    }
}
