using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public Transform cameraTransform;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool _enabled;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        LockAndHideCursor();
        StartCoroutine(SubscribeInputNextFrame());
    }

    //Fixes view jumping when pressing play in the editor
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
        HandleLook();
        HandleMovement();
        //HandleGravityAndJump();
    }
    public void LockAndHideCursor()
    {
        // Hides the cursor and locks it to the center of the game window
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor
    }
    public void UnlockAndShowCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Show the cursor
    }
    void HandleLook()
    {
        // Handle the mouse or stick input for looking around
        var lookSpeedAdj = lookSpeed * .01f;

        float mouseX = lookInput.x * lookSpeedAdj;
        float mouseY = lookInput.y * lookSpeedAdj;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Prevent over-rotating on the X axis
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);  // Up/Down look
        transform.Rotate(Vector3.up * mouseX);  // Left/Right look
    }

    void HandleMovement()
    {
        // Move the player based on input
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        // Handle gravity and jumping
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;  // Small negative value to keep grounded
        }

        if (jumpInput && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);  // Jump physics
        }

        velocity.y += gravity * Time.deltaTime;  // Apply gravity
        controller.Move(velocity * Time.deltaTime);  // Move based on gravity
    }
}
