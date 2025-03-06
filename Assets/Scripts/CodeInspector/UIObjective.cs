using CodeInspector;
using TMPro;
using UnityEngine;

public class UIObjective : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _headerText;
    [SerializeField]
    TextMeshProUGUI _text;

    private void Awake()
    {
        _text.text = "Welcome to Moon Base 256! Grab a cable by pressing E and plug it into the Main Computer to begin.";
    }

    internal void Disable()
    {
        _headerText.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
    }

    internal void LoadTask(CSFileSet task)
    {
        _headerText.gameObject.SetActive(true);
        _text.gameObject.SetActive(true);
        _text.text = task.Description;
    }

}
