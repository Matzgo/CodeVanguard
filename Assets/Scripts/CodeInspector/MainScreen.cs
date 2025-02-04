using CodeInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum MainScreenType
{
    Start,
    Fetching,
    PreTask,
    Code,
    PostTask
}
public class MainScreen : MonoBehaviour
{
    [Header("START")]
    [SerializeField]
    private GameObject _startWindow;


    [SerializeField]
    Button _logInButton;

    [Header("FETCH")]
    [SerializeField]
    private GameObject _fetchingWindow;
    [SerializeField]
    TMPro.TextMeshProUGUI _fetchingText

        ;
    [Header("PRE TASK")]

    [SerializeField]
    private GameObject _preTaskWindow;

    [SerializeField]
    private TextMeshProUGUI _preTaskText;

    [SerializeField]
    Button _beginTaskButton;

    [Header("CODE")]

    [SerializeField]
    private GameObject _codeWindow;
    [SerializeField]
    CustomCodeEditor _customCodeEditor;

    [Header("POST TASK")]

    [SerializeField]
    private GameObject _postTaskWindow;
    [SerializeField]
    private TextMeshProUGUI _postTaskCorrectness;
    [SerializeField]
    private TextMeshProUGUI _postTaskPerformance;
    [SerializeField]
    private TextMeshProUGUI _postTaskMemory;
    [SerializeField]
    private TextMeshProUGUI _postTaskMaintainability;
    [SerializeField]
    private TextMeshProUGUI _postTaskReward;
    [SerializeField]
    private Button _postTaskContinue;
    [Header("------")]

    [SerializeField]
    AudioSource _audioSource;
    private Coroutine _coroutine;

    private void Awake()
    {
        SetActiveWindow(MainScreenType.Start);
    }

    public void DisableAll()
    {
        _startWindow.SetActive(false);
        _fetchingWindow.SetActive(false);
        _preTaskWindow.SetActive(false);
        _codeWindow.SetActive(false);
        _postTaskWindow.SetActive(false);
    }

    private void SetActiveWindow(MainScreenType windowType)
    {
        // Turn off all windows initially
        DisableAll();

        // Activate the specific window based on the enum value
        switch (windowType)
        {
            case MainScreenType.Start:
                _startWindow.SetActive(true);
                break;
            case MainScreenType.Fetching:
                _fetchingWindow.SetActive(true);
                break;
            case MainScreenType.PreTask:
                _preTaskWindow.SetActive(true);
                break;
            case MainScreenType.Code:
                _codeWindow.SetActive(true);
                break;
            case MainScreenType.PostTask:
                _postTaskWindow.SetActive(true);
                break;
        }
    }
    private void Start()
    {
        _logInButton.onClick.AddListener(() => OnLogIn());
        _beginTaskButton.onClick.AddListener(() => StartTask());
        _postTaskContinue.onClick.AddListener(() => PostTaskContinue());
    }

    private void PostTaskContinue()
    {
        CodeVanguardManager.Instance.StartPreTask();

    }

    public void ShowPreTaskWindow()
    {

        SetActiveWindow(MainScreenType.PreTask);
    }

    private void StartTask()
    {
        CodeVanguardManager.Instance.StartTask();
        _audioSource.Play();
    }

    public void OnLogIn()
    {
        SetActiveWindow(MainScreenType.Fetching);
        _audioSource.Play();
        if (CodeVanguardManager.Instance.CurrentTask != null)
        {
            _coroutine = StartCoroutine(FetchForSeconds(2f));
        }
        else
        {

            _coroutine = StartCoroutine(ReturnToMainAfterSeconds(5f));

        }
    }

    private IEnumerator FetchForSeconds(float v)
    {
        _fetchingText.text = "Connecting...";
        yield return new WaitForSeconds(v);
        DisableAll();
        CodeVanguardManager.Instance.StartPreTask();
        _audioSource.Play();

    }
    private IEnumerator ReturnToMainAfterSeconds(float v)
    {
        _fetchingText.text = "No Connection\n\nPlug in a Cable";
        yield return new WaitForSeconds(v);
        DisableAll();
        CodeVanguardManager.Instance.ResetToLogInScreen();
    }


    internal void LoadTask(CSFileSet task)
    {
        _preTaskText.text = task.Title;
    }

    public void LoadResult(GradingResult result)
    {
        if (result == null || result.Correct == false)
        {
            _postTaskCorrectness.text = "INVALID";
            _postTaskPerformance.text = "";
            _postTaskMemory.text = "";
            _postTaskMaintainability.text = "";
            _postTaskReward.text = "";
        }
        else
        {
            _postTaskCorrectness.text = "VALID";
            _postTaskPerformance.text = "PERFORMANCE: " + result.PerformanceScore.ToString("F0");
            _postTaskMemory.text = "MEMORY: " + result.MemoryScore.ToString("F0");
            _postTaskMaintainability.text = "MAINTAINABILITY: " + result.NamingScore.ToString("F0");
            _postTaskReward.text = "100 Credits";
        }
    }

    internal void ShowCodeWindow()
    {
        SetActiveWindow(MainScreenType.Code);
        _customCodeEditor.InputField.Select();
        //_customCodeEditor.InputField.ActivateInputField();
    }

    internal void ShowPostTaskWindow()
    {
        SetActiveWindow(MainScreenType.PostTask);
    }

    internal void ShowLogInWindow()
    {
        SetActiveWindow(MainScreenType.Start);
    }
}
