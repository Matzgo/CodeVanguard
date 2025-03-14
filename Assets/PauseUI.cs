using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField]

    Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        Application.Quit();
    }
}
