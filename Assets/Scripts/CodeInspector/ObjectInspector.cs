using UnityEngine;

public class ObjectInspector : MonoBehaviour
{
    // Reference to the camera that performs the inspection
    [SerializeField] private Camera inspectionCamera;

    // Track the currently inspected object and its original state
    private Transform currentObject;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale; // Added to save original scale

    [SerializeField] float downScaleValue;

    private bool isInspecting = false;
    public bool IsInspecting => isInspecting;

    // Public method to show the object for inspection
    public void Show(PaperInteractable paperInteractable, Vector3 inspectRotate, float distance)
    {
        if (isInspecting)
        {
            Debug.LogWarning("Already inspecting an object.");
            return;
        }

        if (paperInteractable != null)
        {
            // Pause the game
            Time.timeScale = 0;

            currentObject = paperInteractable.transform;

            // Save original position, rotation, and scale
            originalPosition = currentObject.position;
            originalRotation = currentObject.rotation;
            originalScale = currentObject.localScale;

            // Calculate new position in front of the camera
            Vector3 inspectionPosition = inspectionCamera.transform.position +
                                         inspectionCamera.transform.forward * distance;

            // Apply the calculated position
            currentObject.position = inspectionPosition;

            // Combine camera forward rotation with the rotation offset
            Quaternion cameraRotation = Quaternion.LookRotation(inspectionCamera.transform.forward);
            Quaternion offsetRotation = Quaternion.Euler(inspectRotate);
            currentObject.rotation = cameraRotation * offsetRotation;

            // Downscale the object to 0.5x size
            currentObject.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z) * downScaleValue;

            // Set inspecting flag
            isInspecting = true;
        }
    }

    // Public method to place the object back in its original position
    public void Hide()
    {
        Time.timeScale = 1f;

        if (isInspecting && currentObject != null)
        {
            // Restore original position, rotation, and scale
            currentObject.position = originalPosition;
            currentObject.rotation = originalRotation;
            currentObject.localScale = originalScale;

            // Clear state
            currentObject = null;
            isInspecting = false;
        }
    }
}
