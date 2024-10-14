using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private GameInput _gameInput;
    public GameInput Input => _gameInput;


    // Start is called before the first frame update
    void Awake()
    {
        InputSystem.pollingFrequency = 120;

        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _gameInput = new GameInput();
        _gameInput.Disable();

        _gameInput.Player.Enable();
        //GameManager.OnGameStateChanged += OnGameStateChange;

    }

    public void EnablePlayerControls()
    {
        _gameInput.Player.Enable();
    }
    public void DisablePlayerControls()
    {
        _gameInput.Player.Disable();
    }
}
