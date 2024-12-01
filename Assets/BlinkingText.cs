using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _textMeshPro; // Assign this in the Inspector or programmatically.
    [SerializeField]
    float _blinkRate = 0.5f; // Time interval between blinks (seconds).

    private bool _isTextVisible = true;
    private float _timer;

    void Start()
    {
        if (_textMeshPro == null)
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _blinkRate)
        {
            _isTextVisible = !_isTextVisible;
            _textMeshPro.enabled = _isTextVisible; // Toggle text visibility.
            _timer = 0f; // Reset the _timer.
        }
    }
}
