using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerModeToggler : MonoBehaviour
{

    bool _active;

    [SerializeField]
    GameObject _crosshair;

    private void Awake()
    {
        _active = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.Input.Code.ToggleCode.performed += ToggleCode;

    }

    private void ToggleCode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        bool b = !_active;
        if (b)
        {
            InputManager.Instance.Input.Player.Disable();
            InputManager.Instance.Input.Player.Disable();
            _crosshair.SetActive(false);
            UnlockAndShowCursor();
            _active = true;

        }
        else
        {
            InputManager.Instance.Input.Player.Enable();
            InputManager.Instance.Input.Player.Enable();
            _crosshair.SetActive(true);

            LockAndHideCursor();

            UnfocusAllUIElements();
            _active = false;

        }
    }
    private void UnfocusAllUIElements()
    {
        // Use the EventSystem to unfocus any currently selected UI elements
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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



    private void OnDestroy()
    {
        InputManager.Instance.Input.Code.ToggleCode.performed -= ToggleCode;

    }
}
