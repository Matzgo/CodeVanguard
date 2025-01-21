using System.Collections.Generic;
using UnityEngine;

public class CraneScenario : Scenario
{
    CraneScenarioSimulator _sim;

    CraneMiniGameParameters _parameters;

    int[] _cols;
    int _currentCraneCol = 0;

    [SerializeField]
    GameObject _crane;
    [SerializeField]
    GameObject _craneCarryItem;
    bool _craneCarryingItem;
    [SerializeField]
    CraneItem _craneItemPrefab;

    List<CraneItem> _craneItems;

    [SerializeField]
    CraneMiniGameParameters _awakeParams;



    [SerializeField]
    List<GameObject> _colPositions;

    [SerializeField]
    float _itemHeight;
    [SerializeField]
    float _craneHeight;


    protected override void Start()
    {
        base.Start();
        Initialize(_awakeParams);
    }

    protected override void ResetSimulator()
    {
        _sim.Reset(_parameters);
    }

    public void ResetScenario()
    {
        for (int i = _craneItems.Count - 1; i >= 0; i--)
        {
            Destroy(_craneItems[i].gameObject);
        }
        _craneCarryingItem = false;
        Initialize(_awakeParams);
    }

    public void Initialize(CraneMiniGameParameters parameters)
    {
        _parameters = parameters;
        _sim = new CraneScenarioSimulator();
        _sim.Initialize(_parameters);

        _cols = (int[])parameters.cols.Clone();
        Debug.Log("RESETTING");
        _currentCraneCol = parameters.craneStartPos;
        _craneItems = new List<CraneItem>();
        _craneCarryItem.SetActive(false);
        for (int i = 0; i < _cols.Length; i++)
        {
            _craneItems.Add(Instantiate(_craneItemPrefab, transform));
            Vector3 pos = new Vector3(_colPositions[i].transform.position.x, _itemHeight, _colPositions[i].transform.position.z);
            _craneItems[i].transform.position = pos;
            _craneItems[i].SetCount(_cols[i]);
        }

        Vector3 position = new Vector3(_colPositions[_currentCraneCol].transform.position.x, _craneHeight, _colPositions[_currentCraneCol].transform.position.z);
        _crane.transform.position = position;

        UpdateCraneItems();
    }

    public override (bool b, List<string> feedback) CheckCorrectness()
    {
        return _sim.CheckCorrectness(_parameters);

        //var feedback = new List<string>();
        //if (_parameters.targetCols == null)
        //{
        //    const string Message = "Parameters or targetCols are not set.";
        //    Debug.LogError(Message);
        //    feedback.Add(Message);
        //    return (false, feedback);
        //}

        //if (_cols.Length != _parameters.targetCols.Length)
        //{
        //    const string Message = "Incorrect: Columns do not match the target length.";
        //    Debug.LogError(Message);
        //    feedback.Add(Message);
        //    return (false, feedback);

        //}

        //for (int i = 0; i < _cols.Length; i++)
        //{
        //    if (_cols[i] != _parameters.targetCols[i])
        //    {
        //        const string Message = "Incorrect: Columns do not match the target configuration.";
        //        Debug.Log(Message);
        //        feedback.Add(Message);
        //        return (false, feedback);
        //    }
        //}

        //return (true, feedback);
    }




    public void MoveRight()
    {
        if (_simulate)
        {
            _sim.MoveRight();
            return;
        }

        _currentCraneCol = Mathf.Clamp(_currentCraneCol + 1, 0, _cols.Length - 1);
        Vector3 position = new Vector3(_colPositions[_currentCraneCol].transform.position.x, _craneHeight, _colPositions[_currentCraneCol].transform.position.z);
        _crane.transform.position = position;
    }

    public void MoveLeft()
    {
        if (_simulate)
        {
            _sim.MoveLeft();
            return;
        }

        _currentCraneCol = Mathf.Clamp(_currentCraneCol - 1, 0, _cols.Length - 1);
        Vector3 position = new Vector3(_colPositions[_currentCraneCol].transform.position.x, _craneHeight, _colPositions[_currentCraneCol].transform.position.z);
        _crane.transform.position = position;
    }

    public void UpdateCraneItems()
    {
        for (int i = 0; i < _craneItems.Count; i++)
        {
            _craneItems[i].SetCount(_cols[i]);
        }
    }

    public void PickUp()
    {
        if (_simulate)
        {
            _sim.PickUp();
            return;
        }

        if (!_craneCarryingItem)
        {
            _craneCarryingItem = true;
            _cols[_currentCraneCol] = _cols[_currentCraneCol] - 1;
            _craneCarryItem.SetActive(true);

        }
        UpdateCraneItems();

    }

    public void Drop()
    {
        if (_simulate)
        {
            _sim.Drop();
            return;
        }

        if (_craneCarryingItem)
        {
            _craneCarryingItem = false;
            _cols[_currentCraneCol] = _cols[_currentCraneCol] + 1;
            _craneCarryItem.SetActive(false);

        }
        UpdateCraneItems();

    }

}

[System.Serializable]
public struct CraneMiniGameParameters
{
    public int[] cols;
    public int craneStartPos;

    public int[] targetCols;

}
