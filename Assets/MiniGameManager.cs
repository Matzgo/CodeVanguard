using CodeInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MiniGameManager : MonoBehaviour
{
    [System.Serializable]
    public class GameEventAction
    {
        public string eventKey; // The event key to listen for
        public UnityEvent eventAction; // The UnityEvent to trigger
    }

    [SerializeField]
    public UnityEvent onReset;

    [SerializeField]
    private List<GameEventAction> gameEvents = new List<GameEventAction>(); // List of event-key-action pairs

    private Dictionary<string, UnityEvent> eventDictionary; // Dictionary for quick lookup


    private void Awake()
    {
        // Initialize the event dictionary
        eventDictionary = new Dictionary<string, UnityEvent>();

        // Populate the dictionary with the registered events
        foreach (var gameEvent in gameEvents)
        {
            if (!string.IsNullOrEmpty(gameEvent.eventKey) && gameEvent.eventAction != null)
            {
                eventDictionary[gameEvent.eventKey] = gameEvent.eventAction;
            }
        }

        // Register to the RuntimeManager event system
        RuntimeManager.Instance.GameEvent += OnGameEvent;
        RuntimeManager.Instance.MiniGameReset += OnReset;
    }

    private void OnReset()
    {
        onReset?.Invoke();
    }

    private void OnDestroy()
    {
        // Unregister from the RuntimeManager event system
        RuntimeManager.Instance.GameEvent -= OnGameEvent;
    }
    private void OnGameEvent(string eventKey)
    {
        // Check if the event key exists in the dictionary
        if (eventDictionary != null && eventDictionary.TryGetValue(eventKey, out UnityEvent eventAction))
        {
            // Invoke the associated UnityEvent
            eventAction?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Event key '{eventKey}' is not registered.");
        }
    }
}
