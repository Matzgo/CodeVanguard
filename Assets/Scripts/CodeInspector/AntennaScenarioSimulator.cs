using System.Collections.Generic;
using System.Linq;

public class AntennaScenarioSimulator
{
    private List<int> _solutionCols;

    public void Initialize(AntennaMiniGameParameters parameters)
    {
        _solutionCols = new List<int>(parameters.targetArray);
    }

    public (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness(List<int> cols)
    {
        bool isCorrect = _solutionCols.SequenceEqual(cols);

        List<string> feedback = new List<string>();
        List<string> feedbackKey = new List<string>();

        if (isCorrect)
        {
            //feedback.Add("Great job! The configuration is correct.");
            feedbackKey.Add("ANTENNA_Valid");
        }
        else
        {
            feedback.Add("The order is incorrect, try changing the order");
            feedbackKey.Add("ANTENNA_Invalid");

        }

        return (isCorrect, feedback, feedbackKey);
    }

    public void Reset(AntennaMiniGameParameters parameters)
    {
        Initialize(parameters);
    }
}
