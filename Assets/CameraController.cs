using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float sensitivity = 100f;        // Mouse sensitivity
    [SerializeField] private float zoomSensitivity = 10f;     // Zoom sensitivity
    [SerializeField] private float minFOV = 30f;              // Minimum Field of View (zoomed in)
    [SerializeField] private float maxFOV = 90f;              // Maximum Field of View (zoomed out)
    [SerializeField] private float zoomLerpSpeed = 5f;        // Speed of zoom interpolation
    [SerializeField] private float maxXAngle = 80f;           // Maximum vertical rotation angle (up and down)
    [SerializeField] private float maxYAngle = 80f;           // Maximum horizontal rotation angle (left and right)

    private float xRotation = 0f; // Tracks the current vertical rotation
    private float yRotation = 0f; // Tracks the current horizontal rotation
    private Camera _camera;       // Reference to the camera component
    private float targetFOV;      // Target FOV for smooth zooming

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the screen
        Cursor.visible = false;                  // Hides the cursor
        _camera = GetComponent<Camera>();        // Get the Camera component
        targetFOV = _camera.fieldOfView;         // Initialize target FOV
    }

    private void Update()
    {
        // Read the mouse delta from the Look action
        Vector2 mouseDelta = InputManager.Instance.Input.Player.Look.ReadValue<Vector2>();

        // Adjust rotation values based on mouse input
        yRotation += mouseDelta.x * sensitivity * Time.deltaTime;
        xRotation -= mouseDelta.y * sensitivity * Time.deltaTime;

        // Clamp the angles
        xRotation = Mathf.Clamp(xRotation, -maxXAngle, maxXAngle);
        yRotation = Mathf.Clamp(yRotation, -maxYAngle, maxYAngle);

        // Apply the rotation
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Read the zoom value from the Zoom action
        var zoomInput = InputManager.Instance.Input.Player.Zoom.ReadValue<Vector2>();

        // Calculate the target FOV based on zoom input
        targetFOV -= zoomInput.y * zoomSensitivity * Time.deltaTime;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

        // Smoothly interpolate the camera's FOV towards the target FOV
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, zoomLerpSpeed * Time.deltaTime);
    }
}
