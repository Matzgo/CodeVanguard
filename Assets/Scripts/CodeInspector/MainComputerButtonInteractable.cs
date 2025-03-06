using System;
using UnityEngine;

public class MainComputerButtonInteractable : MonoBehaviour, IInteractable
{
    private Action registeredAction;

    [SerializeField] AudioSource _audio;

    public void RegisterAction(Action action)
    {
        registeredAction = action;
    }


    public void OnInteract()
    {

        if (_audio != null)
            _audio.Play();



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
        return gameObject.activeInHierarchy;
    }
}
