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
        currentState = CodeVanguardState.TaskStart;
        LoadTask(_task);

        _screenManager.TurnOffAllScreens();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowPreTaskWindow();
        _screenManager.TurnOn(ScreenType.Docs);
        //_screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Task);
        // _screenManager.TurnOn(ScreenType.Visual);
    }

    public void StartTask()
    {
        RuntimeManager.Instance.Console.ClearConsole();
        _task.MiniGame.LoadScene();
        _screenManager.TurnOffAllScreens();
        _screenManager.Main.ShowCodeWindow();
        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.TurnOn(ScreenType.Task);
        _screenManager.TurnOn(ScreenType.Docs);
        _screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Visual);
        _screenManager.TurnOn(ScreenType.TaskAction);

    }

    public void LoadTask(CSFileSet task)
    {
        _runtimeCodeSystem.LoadTask(task);
        _screenManager.LoadTask(task);

    }

    public void EndTask(GradingResult res)
    {
        _screenManager.TurnOffAllScreens();
        _task.MiniGame.UnloadScene();

        _screenManager.LoadResult(res);

        _screenManager.TurnOn(ScreenType.Main);
        _screenManager.Main.ShowPostTaskWindow();
        _screenManager.TurnOn(ScreenType.Task);
        _screenManager.TurnOn(ScreenType.Console);
        _screenManager.TurnOn(ScreenType.Visual);
    }

}
