using CodeInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum CodeVanguardState
{
    Offline,
    Startup,
    TaskSelect,
    TaskStart,
    Task,
    TaskEnd,
}

public class CodeVanguardManager : MonoBehaviour
{
    private static CodeVanguardManager _instance;
    public static CodeVanguardManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("CodeVanguardManager is not initialized! Make sure it's present in the scene.");
            }
            return _instance;
        }
    }

    [SerializeField] private CodeVanguardState currentState = CodeVanguardState.Offline;
    public CodeVanguardState CurrentState => currentState;
    [SerializeField] ScreenManager _screenManager;
    [SerializeField] RuntimeCodeSystem _runtimeCodeSystem;
    public RuntimeCodeSystem RuntimeCodeSystem => _runtimeCodeSystem;
    [SerializeField] FeedbackBot _feedbackBot;
    [SerializeField] CSFileSet _task;
    public CSFileSet CurrentTask => _task;

    public Action<string, GradingResult> OnTaskEnd;
    [SerializeField]
    UIObjective _uiObjective;

    [SerializeField] bool _useStarSystem;
    public bool UseStarSystem => _useStarSystem;
    [SerializeField] bool _useFeedbackSystem;
    public bool UseFeedbackSystem => _useFeedbackSystem;

    [SerializeField] PlayerModeToggler _playerModeToggler;
    public PlayerModeToggler PlayerModeToggler => _playerModeToggler;

    public string UniqueId { get; set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        UniqueId = GenerateUniqueId();
    }

    private string GenerateUniqueId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder result = new StringBuilder(5);
        for (int i = 0; i < 5; i++)
        {
            result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }
        return result.ToString();
    }

    private void Start()
    {
        _screenManager.TurnOffAllScreens();

        StartCoroutine(StartSystem());
    }

    private IEnumerator StartSystem()
    {
        yield return new WaitForSeconds(1f);
        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
    }


    public void StartPreTask()
    {
        if (_task == null)
            return;

        currentState = CodeVanguardState.TaskStart;
        LoadTask(_task);

        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowPreTaskWindow();
        //_screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Task);
        // _screenManager.TurnOn(ScreenType.Visual);
    }

    public void StartTask()
    {

        _screenManager.TurnOffAllScreens();
        currentState = CodeVanguardState.Task;
        if (_task != null)
        {
            EvaluationDataTracker.Instance.TrackTime(_task.TaskID);
        }

        RuntimeManager.Instance.Console.ClearConsole();
        if (_task.MiniGame != null)
        {
            _task.MiniGame.LoadScene();
            _screenManager.TurnOn(ScreenType.Visual);
        }
        _screenManager.Main.ShowCodeWindow();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.TurnOn(ScreenType.Task);
        _screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.TaskAction);

    }

    public void LoadTask(CSFileSet task)
    {
        if (task == null)
        {
            Debug.LogError("NULL TASK");
            return;
        }
        _runtimeCodeSystem.LoadTask(task);
        _screenManager.LoadTask(task);
        _uiObjective.LoadTask(task);
    }



    public void EndTask(GradingResult res)
    {
        _screenManager.TurnOffAllScreens();
        currentState = CodeVanguardState.TaskEnd;

        OnTaskEnd?.Invoke(_task.TaskID, res);

        if (_task != null)
        {
            EvaluationDataTracker.Instance.StopTrackTime(_task.TaskID);
        }


        _screenManager.LoadResult(res);
        _task.MiniGame.UnloadScene();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowPostTaskWindow();
        _screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Task);
        _uiObjective.Disable();

    }

    internal void Unplug()
    {
        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowLogInWindow();

        if (_task != null)
        {
            if (_task.MiniGame != null)
            {
                _task.MiniGame.UnloadScene();
            }
            EvaluationDataTracker.Instance.StopTrackTime(_task.TaskID);
        }

        _task = null;
        _uiObjective.Disable();

    }

    public void ResetToLogInScreen()
    {
        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowLogInWindow();
    }

    internal void PlugIn(CSFileSet task)
    {
        _task = task;

        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowLogInWindow();
    }


    public void LoadFeedback(List<string> l)
    {
        _feedbackBot.LoadFeedback(l);
    }

}
