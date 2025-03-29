using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool _useStarSystem;
    public bool UseStarSystem => _useStarSystem;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUseStarSystem(bool b)
    {
        _useStarSystem = b;
    }
}
