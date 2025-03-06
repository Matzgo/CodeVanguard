using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Movement fields
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float gravity = -9.81f;

    // Zoom fields
    [SerializeField] private float zoomSensitivity = 10f;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 90f;
    [SerializeField] private float zoomLerpSpeed = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public Transform cameraTransform;
    private Camera _camera;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool _enabled;

    private float targetFOV;

    [SerializeField]
    LayerMask _interactLayerMask;

    [SerializeField]
    ObjectInspector _objectInspector;

    [SerializeField]
    PlayerCarry _playerCarry;
    [SerializeField]
    private GameObject _interactCrosshair;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        _camera = cameraTransform.GetComponent<Camera>();
        targetFOV = _camera.fieldOfView; // Initialize target FOV

        InputManager.Instance.Input.Player.Interact.performed += OnInteract;
    }
    private void OnDestroy()
    {
        InputManager.Instance.Input.Player.Interact.performed -= OnInteract;

    }
    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_objectInspector.IsInspecting)
        {
            _objectInspector.Hide();
            return;
        }

        if (_playerCarry.IsCarrying)
        {
            if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit socketHit, 2.3f, _interactLayerMask))
            {
                if (socketHit.collider.TryGetComponent<CableSocket>(out var socket))
                {
                    _playerCarry.PlugIn(socket);
                    return;
                }
            }

            _playerCarry.Drop();
            return;
        }

        // Perform the raycast from the camera's position in the forward direction
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, 4f, _interactLayerMask))
        {
            Debug.DrawLine(_camera.transform.position, hit.point, Color.green, 5);
            // If an object is hit, you can interact with it
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnInteract();
                return;
            }

            if (hit.collider.TryGetComponent<PickupableCable>(out var plug))
            {
                _playerCarry.PickUp(plug);
                return;
            }

        }
    }

    private void Start()
    {
        StartCoroutine(SubscribeInputNextFrame());
    }

    private IEnumerator SubscribeInputNextFrame()
    {
        yield return null;
        yield return null;
        _enabled = true;
    }

    void Update()
    {
        if (!_enabled)
            return;

        // Get input from InputManager's GameInput system
        moveInput = InputManager.Instance.Input.Player.Move.ReadValue<Vector2>();
        lookInput = InputManager.Instance.Input.Player.Look.ReadValue<Vector2>();

        HandleInteractCrosshair();

        HandleLook();
        HandleMovement();
        HandleGravityAndJump();
        HandleZoom();
    }

    private void HandleInteractCrosshair()
    {
        float interactionRange = _playerCarry.IsCarrying ? 2.3f : 4f; // Adjust range based on carrying state

        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, interactionRange, _interactLayerMask))
        {
            bool canShowCrosshair = false;

            if (_playerCarry.IsCarrying)
            {
                // Only allow crosshair to appear for CableSocket when carrying
                if (hit.collider.TryGetComponent<CableSocket>(out var socket))
                {
                    canShowCrosshair = true;
                }
            }
            else
            {
                // When NOT carrying, check other interactables in full range
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    canShowCrosshair = interactable.CanInteract();
                }
                else if (hit.collider.GetComponent<PickupableCable>() != null)
                {
                    canShowCrosshair = true;
                }
            }

            _interactCrosshair.SetActive(canShowCrosshair);
            return;
        }

        _interactCrosshair.SetActive(false);
    }




    void HandleLook()
    {
        var lookSpeedAdj = lookSpeed * .01f;
        float mouseX = lookInput.x * lookSpeedAdj;
        float mouseY = lookInput.y * lookSpeedAdj;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        // Check if grounded and reset velocity if grounded
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // Small value to keep grounded
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move the character based on velocity
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleZoom()
    {
        var zoomInput = InputManager.Instance.Input.Camera.Zoom.ReadValue<Vector2>();

        targetFOV -= zoomInput.y * zoomSensitivity * Time.deltaTime;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, zoomLerpSpeed * Time.deltaTime);
    }
}
