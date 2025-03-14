using TMPro;
using UnityEngine;

public class SessionIDText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = CodeVanguardManager.Instance.UniqueId;
    }

}
