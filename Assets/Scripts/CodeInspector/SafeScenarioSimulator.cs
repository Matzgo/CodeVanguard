using System.Collections.Generic;

public class SafeScenarioSimulator
{
    private bool _isOpen = false;

    public SafeScenarioSimulator()
    {
        Initialize();
    }

    public void Initialize()
    {
        _isOpen = false;
    }

    public void ResetSafe()
    {
        _isOpen = false;
    }

    public void OpenSafe()
    {
        _isOpen = true;
    }

    public bool IsSafeOpen()
    {
        return _isOpen;
    }



    public (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness()
    {
        var feedback = new List<string>();
        var feedbackKey = new List<string>();

        if (_isOpen)
        {
            feedbackKey.Add("SAFE_Unlocked");
            return (true, feedback, feedbackKey);
        }
        else
        {
            feedbackKey.Add("SAFE_Locked");
            feedback.Add("The safe is still closed. Try to modify the code in such a way that the safe opens.");
            return (false, feedback, feedbackKey);
        }
    }
}
