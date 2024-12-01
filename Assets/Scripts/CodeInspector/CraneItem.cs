using TMPro;
using UnityEngine;

public class CraneItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;
    public void SetCount(int i)
    {
        if (i == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            _text.text = i.ToString();
        }
    }
}
