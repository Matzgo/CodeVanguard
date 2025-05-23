using System.Collections.Generic;
using UnityEngine;

public class CraneScenarioSimulator
{
    private int[] _cols;
    private int _currentCraneCol = 0;
    private bool _craneCarryingItem = false;
    private int _carriedItemCount = 0;

    public void Initialize(CraneMiniGameParameters parameters)
    {
        _cols = (int[])parameters.cols.Clone();
        _currentCraneCol = parameters.craneStartPos;
        _craneCarryingItem = false;
        _carriedItemCount = 0;
    }

    public void Reset(CraneMiniGameParameters parameters)
    {
        Initialize(parameters);
    }

    public void MoveRight()
    {
        _currentCraneCol = Mathf.Clamp(_currentCraneCol + 1, 0, _cols.Length - 1);
    }

    public void MoveLeft()
    {
        _currentCraneCol = Mathf.Clamp(_currentCraneCol - 1, 0, _cols.Length - 1);
    }

    public bool PickUp()
    {
        if (!_craneCarryingItem && _cols[_currentCraneCol] > 0)
        {
            _craneCarryingItem = true;
            _carriedItemCount = 1; // Assuming the crane can carry one item at a time.
            _cols[_currentCraneCol]--;

            return true;
        }
        return false;
    }

    public bool Drop()
    {
        if (_craneCarryingItem)
        {
            _craneCarryingItem = false;
            _carriedItemCount = 0;
            _cols[_currentCraneCol]++;

            return true;
        }
        return false;
    }

    public (bool isCorrect, List<string> feedback, List<string> feedbackKey) CheckCorrectness(CraneMiniGameParameters parameters)
    {
        var feedback = new List<string>();
        var feedbackKey = new List<string>();
        if (parameters.targetCols == null)
        {
            feedback.Add("Target columns are not set.");
            return (false, feedback, feedbackKey);
        }

        if (_cols.Length != parameters.targetCols.Length)
        {
            feedback.Add("The number of columns does not match the target.");
            return (false, feedback, feedbackKey);
        }

        bool noMatch = false;
        for (int i = 0; i < _cols.Length; i++)
        {
            if (_cols[i] != parameters.targetCols[i])
            {
                feedback.Add($"Column {i + 1} does not match the target value. Expected: {parameters.targetCols[i]} Current: {_cols[i]}");

                noMatch = true;
            }
        }
        if (noMatch)
        {
            feedbackKey.Add("CRANE_Invalid");
            return (false, feedback, feedbackKey);
        }

        feedback.Add("All columns match the target configuration.");
        feedbackKey.Add("CRANE_Valid");

        return (true, feedback, feedbackKey);
    }

    public int[] GetCurrentState()
    {
        return (int[])_cols.Clone();
    }

    public int GetCurrentCranePosition()
    {
        return _currentCraneCol;
    }

    public bool IsCarryingItem()
    {
        return _craneCarryingItem;
    }

    public int GetCarriedItemCount()
    {
        return _carriedItemCount;
    }
}
