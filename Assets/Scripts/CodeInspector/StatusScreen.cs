using UnityEngine;
using UnityEngine.UI;

public class StatusScreen : MonoBehaviour
{
    [SerializeField]
    string _taskId;
    public string TaskId => _taskId;


    [SerializeField]
    TMPro.TextMeshProUGUI statusText;
    [SerializeField]
    Image bg;
    private void Awake()
    {
        SetInvalid();

    }

    private void Start()
    {
        CodeVanguardManager.Instance.OnTaskEnd += OnTaskEnd;
    }

    private void OnTaskEnd(string id, GradingResult result)
    {
        if (id.Equals(_taskId))
        {
            UpdateStatus(result);
        }
    }

    public void SetSuboptimal()
    {
        statusText.text = "SUBOPTIMAL";
        bg.color = Color.yellow;
    }
    public void SetOptimal()
    {
        statusText.text = "OPTIMAL";
        bg.color = Color.green;

    }
    public void SetInvalid()
    {
        statusText.text = "INVALID";
        bg.color = Color.red;
    }
    public void UpdateStatus(GradingResult res)
    {
        if (!res.Correct)
        {
            SetInvalid();
        }
        else
        {
            if (res.NamingScore < 100f || res.PerformanceScore < 100f || res.MemoryScore < 100f)
            {
                SetSuboptimal();
            }
            else
            {
                SetOptimal();
            }

        }

    }
}
