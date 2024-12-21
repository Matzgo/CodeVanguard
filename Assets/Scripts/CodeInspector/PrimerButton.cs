using UnityEngine;
using UnityEngine.UI;

public class PrimerButton : MonoBehaviour
{
    public Button primerButton; // Reference to the Button component
    public GameObject primerImage; // The GameObject to rotate

    private bool isToggled = false; // Tracks toggle state

    public Button approveButton; // Button to enable/disable
    public Image approveImage; // Image to change color
    private Color _approveImageColor; // Original color of the approveImage

    void Start()
    {
        // Ensure the Button reference is set
        if (primerButton != null)
        {
            primerButton.onClick.AddListener(ToggleRotation); // Add the listener dynamically
        }
        else
        {
            Debug.LogError("Button reference not set on PrimerButton script.");
        }

        // Store the original color of the approveImage
        if (approveImage != null)
        {
            _approveImageColor = approveImage.color;
        }
        else
        {
            Debug.LogError("ApproveImage reference not set on PrimerButton script.");
        }

        // Ensure approveButton is initially not interactable
        if (approveButton != null)
        {
            approveButton.interactable = false;
        }
        else
        {
            Debug.LogError("ApproveButton reference not set on PrimerButton script.");
        }
    }

    void ToggleRotation()
    {
        // Toggle the state
        isToggled = !isToggled;

        // Apply rotation based on toggle state
        if (isToggled)
        {
            primerImage.transform.localRotation = Quaternion.Euler(0, 0, 90); // Rotate to 90 degrees
            EnableApproveButton(); // EnableMiniGame the approve primerButton and change color
        }
        else
        {
            primerImage.transform.localRotation = Quaternion.Euler(0, 0, 0); // Reset to 0 degrees
            DisableApproveButton(); // DisableMiniGame the approve primerButton and reset color
        }
    }

    void EnableApproveButton()
    {
        // Make the approve primerButton interactable
        if (approveButton != null)
        {
            approveButton.interactable = true;
        }

        // Change approveImage color to green
        if (approveImage != null)
        {
            approveImage.color = Color.green;
        }
    }

    void DisableApproveButton()
    {
        // Make the approve primerButton non-interactable
        if (approveButton != null)
        {
            approveButton.interactable = false;
        }

        // Reset approveImage color to its original color
        if (approveImage != null)
        {
            approveImage.color = _approveImageColor;
        }
    }
}
