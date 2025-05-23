using CodeInspector;
using System;
using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable
{
    private Action registeredAction;

    [SerializeField] GameObject buttonScreen;
    [SerializeField] AudioSource _audio;

    private void Start()
    {
        buttonScreen.SetActive(true);
        RuntimeManager.Instance.ResetStartButton += OnReset;

    }

    private void OnReset()
    {
        buttonScreen.SetActive(true);
    }
    private void OnDestroy()
    {
        RuntimeManager.Instance.ResetStartButton -= OnReset;

    }

    public void RegisterAction(Action action)
    {
        registeredAction = action;
    }

    public void OnInteract()
    {
        if (CodeVanguardManager.Instance.CurrentState != CodeVanguardState.Task)
        {
            return;
        }

        if (CodeVanguardManager.Instance.RuntimeCodeSystem.CodeRunning)
            return;

        _audio.Play();
        buttonScreen.SetActive(false);



        if (registeredAction != null)
        {
            registeredAction.Invoke();
        }
        else
        {
            Debug.LogWarning("No action registered for this button.");
        }
    }

    // Optional: Method to clear the registered action
    public void ClearRegisteredAction()
    {
        registeredAction = null;
    }

    public bool CanInteract()
    {
        if (CodeVanguardManager.Instance.CurrentState != CodeVanguardState.Task)
        {
            return false;
        }

        if (CodeVanguardManager.Instance.RuntimeCodeSystem.CodeRunning)
            return false;

        return true;
    }
}