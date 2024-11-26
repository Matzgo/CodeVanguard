using UnityEngine;

public class CodeUI : MonoBehaviour
{
    [SerializeField]
    GameObject _codeUIRoot;

    bool _active;

    private void Awake()
    {
        //_codeUIRoot.SetActive(false);
        _active = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.Input.Player.ToggleCode.performed += ToggleCode;

    }

    private void ToggleCode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        bool b = !_active;
        if (b)
        {
            InputManager.Instance.Input.Player.Move.Disable();
            InputManager.Instance.Input.Player.Look.Disable();
            UnlockAndShowCursor();
            _active = true;

        }
        else
        {
            InputManager.Instance.Input.Player.Move.Enable();
            InputManager.Instance.Input.Player.Look.Enable();
            LockAndHideCursor();
            _active = false;

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
        InputManager.Instance.Input.Player.ToggleCode.performed -= ToggleCode;

    }
}
