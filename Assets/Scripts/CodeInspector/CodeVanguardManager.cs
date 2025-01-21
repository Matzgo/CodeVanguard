using CodeInspector;
using System.Collections;
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
    [SerializeField] ScreenManager _screenManager;
    [SerializeField] RuntimeCodeSystem _runtimeCodeSystem;

    [SerializeField] CSFileSet _task;
    private void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

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

        RuntimeManager.Instance.Console.ClearConsole();
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

    }

    public void EndTask(GradingResult res)
    {
        _screenManager.TurnOffAllScreens();




        _screenManager.LoadResult(res);

        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowPostTaskWindow();
        _screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Task);
    }

    internal void Unplug()
    {
        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowLogInWindow();
        _task = null;
    }

    internal void PlugIn(CSFileSet task)
    {
        _task = task;

        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowLogInWindow();
    }
}
