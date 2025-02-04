using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeedbackDatabase", menuName = "ScriptableObjects/FeedbackDatabase")]
public class FeedbackDatabase : ScriptableObject
{

    [SerializeField]
    private List<FeedbackData> feedbackEntries = new List<FeedbackData>();

    private Dictionary<string, FeedbackData> feedbackDictionary;

    private void OnEnable()
    {
        // Create the dictionary on enable and populate it
        feedbackDictionary = new Dictionary<string, FeedbackData>();
        foreach (var entry in feedbackEntries)
        {
            if (!feedbackDictionary.ContainsKey(entry.key))
            {
                feedbackDictionary[entry.key] = entry;
            }
            else
            {
                Debug.LogWarning($"Duplicate key found: {entry.key}. Entry will be ignored.");
            }
        }
    }

    public FeedbackData GetFeedback(string key)
    {
        feedbackDictionary.TryGetValue(key, out FeedbackData entry);
        return entry;
    }
}
[System.Serializable]
public class FeedbackData
{
    public string key;
    [TextArea] // Allows for multi-line text in the Inspector
    public string feedbackText;
    public AudioClip audioClip;
    public float postDelay;
}
