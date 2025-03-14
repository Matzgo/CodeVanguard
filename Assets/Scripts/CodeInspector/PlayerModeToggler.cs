using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerModeToggler : MonoBehaviour
{

    bool _active;
    public bool CodeModeActive => _active;

    [SerializeField]
    GameObject _crosshair;
    [SerializeField] private GameObject _pauseMenu; // Reference to the Pause UI
    private void Awake()
    {
        _active = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.Input.Code.ToggleCode.performed += ToggleCode;
        InputManager.Instance.Input.Code.Pause.performed += TogglePauseMenu;
    }

    private void TogglePauseMenu(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_active)
        {
            // If in Code Mode, exit Code Mode instead of opening the Pause Menu
            ToggleCode(context);
        }
        else
        {
            // Otherwise, toggle the Pause UI
            if (_pauseMenu != null)
            {
                bool isPaused = _pauseMenu.activeSelf;
                _pauseMenu.SetActive(!isPaused);

                if (isPaused)
                {
                    Time.timeScale = 1f;
                    LockAndHideCursor();
                    InputManager.Instance.Input.Player.Enable();
                }
                else
                {
                    Time.timeScale = 0f;
                    UnlockAndShowCursor();
                    InputManager.Instance.Input.Player.Disable();
                }
            }
        }
    }

    private void Update()
    {
        if (!_active)
        {
            //UnfocusAllUIElements();
        }
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
        InputManager.Instance.Input.Code.Pause.performed -= TogglePauseMenu;

    }
}
