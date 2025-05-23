using CodeInspector;
using UnityEngine;

public enum ScreenType
{
    Main,
    Console,
    Task,
    TaskAction,
    Visual,
}

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    private MainScreen _main;
    public MainScreen Main => _main;


    [SerializeField]
    GameObject _mainScreen;
    [SerializeField]
    GameObject _consoleScreen;

    [SerializeField]
    FeedbackBotBehaviour _feedbackBot;
    public FeedbackBotBehaviour FeedbackBot => _feedbackBot;

    [SerializeField]
    GameObject _taskScreen;
    [SerializeField]
    GameObject _taskActionScreen;
    [SerializeField]
    GameObject _taskActionScreen2;
    [SerializeField]
    GameObject _visualScreen;
    [SerializeField]
    GameObject _docsScreen;



    public void TurnOffAllScreens()
    {
        _mainScreen.SetActive(false);
        _consoleScreen.SetActive(false);
        //_taskScreen.SetActive(false);
        _visualScreen.SetActive(false);
        _taskActionScreen.SetActive(false);
        _taskActionScreen2.SetActive(false);
        _docsScreen.SetActive(false);
    }

    public void TurnOn(ScreenType screenType)
    {
        switch (screenType)
        {
            case ScreenType.Main:
                _mainScreen.SetActive(true);
                break;
            case ScreenType.Console:
                _consoleScreen.SetActive(true);
                break;
            //case ScreenType.Task:
            //    _taskScreen.SetActive(true);
            //break;
            case ScreenType.TaskAction:
                _taskActionScreen.SetActive(true);
                _taskActionScreen2.SetActive(true);
                break;
            case ScreenType.Visual:
                _visualScreen.SetActive(true);
                break;
        }
    }

    public void TurnOff(ScreenType screenType)
    {
        switch (screenType)
        {
            case ScreenType.Main:
                _mainScreen.SetActive(false);
                break;
            case ScreenType.Console:
                _consoleScreen.SetActive(false);
                break;
            //case ScreenType.Task:
            //    _taskScreen.SetActive(false);
            //    break;
            case ScreenType.TaskAction:
                _taskActionScreen.SetActive(false);
                _taskActionScreen2.SetActive(false);
                break;
            case ScreenType.Visual:
                _visualScreen.SetActive(false);
                break;

        }
    }

    internal void LoadTask(CSFileSet task)
    {
        _main.LoadTask(task);
        _feedbackBot.LoadTask(task);

    }

    internal void LoadResult(GradingResult res)
    {
        _main.LoadResult(res);
        _feedbackBot.QueueFeedback(res);
    }
}
