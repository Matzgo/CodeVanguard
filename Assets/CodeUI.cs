using UnityEngine;

public class CodeUI : MonoBehaviour
{
    [SerializeField]
    GameObject _codeUIRoot;

    [SerializeField]

    PlayerController _playerController;

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.Input.Player.ToggleCode.performed += ToggleCode;
    }

    private void ToggleCode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        bool b = !_codeUIRoot.activeSelf;
        _codeUIRoot.SetActive(b);
        if (b)
        {
            InputManager.Instance.Input.Player.Move.Disable();
            InputManager.Instance.Input.Player.Look.Disable();
            _playerController.UnlockAndShowCursor();

        }
        else
        {
            InputManager.Instance.Input.Player.Move.Enable();
            InputManager.Instance.Input.Player.Look.Enable();
            _playerController.LockAndHideCursor();

        }
    }

    private void OnDestroy()
    {
        InputManager.Instance.Input.Player.ToggleCode.performed -= ToggleCode;

    }
}
