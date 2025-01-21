using System;
using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable
{
    private Action registeredAction;

    public void RegisterAction(Action action)
    {
        registeredAction = action;
    }

    public void OnInteract()
    {
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
}