using System.Collections.Generic;
using UnityEngine;



public class CraneMiniGame : MonoBehaviour
{

    CraneMiniGameParameters _parameters;

    int[] _cols;
    int _craneCol = 0;

    [SerializeField]
    GameObject _crane;
    bool _craneCarryingItem;
    [SerializeField]
    CraneItem _craneItemPrefab;

    List<CraneItem> _craneItems;

    [SerializeField]
    CraneMiniGameParameters _awakeParams;

    private void Awake()
    {
        Initialize(_awakeParams);
    }

    public void Reset()
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
        _cols = (int[])parameters.cols.Clone();
        _craneCol = parameters.craneStartPos;
        _craneItems = new List<CraneItem>();
        for (int i = 0; i < _cols.Length; i++)
        {
            _craneItems.Add(Instantiate(_craneItemPrefab, transform));
            var v = new Vector3((i - _cols.Length / 2f) * 4f, 0, 0);
            _craneItems[i].transform.position = v;
            _craneItems[i].SetCount(_cols[i]);
        }

        _crane.transform.position = new Vector3((_craneCol - _cols.Length / 2f) * 4f, 9, 0);

        UpdateCraneItems();
    }

    public (bool b, List<string> feedback) CheckCorrectness()
    {
        var feedback = new List<string>();
        if (_parameters.targetCols == null)
        {
            const string Message = "Parameters or targetCols are not set.";
            Debug.LogError(Message);
            feedback.Add(Message);
            return (false, feedback);
        }

        if (_cols.Length != _parameters.targetCols.Length)
        {
            const string Message = "Incorrect: Columns do not match the target length.";
            Debug.LogError(Message);
            feedback.Add(Message);
            return (false, feedback);

        }

        for (int i = 0; i < _cols.Length; i++)
        {
            if (_cols[i] != _parameters.targetCols[i])
            {
                const string Message = "Incorrect: Columns do not match the target configuration.";
                Debug.LogError(Message);
                feedback.Add(Message);
                return (false, feedback);
            }
        }

        return (true, feedback);
    }


    public void MoveRight()
    {
        _craneCol = Mathf.Clamp(_craneCol + 1, 0, _cols.Length - 1);
        _crane.transform.position = new Vector3((_craneCol - _cols.Length / 2f) * 4f, 9, 0);
    }

    public void MoveLeft()
    {
        _craneCol = Mathf.Clamp(_craneCol - 1, 0, _cols.Length - 1);
        _crane.transform.position = new Vector3((_craneCol - _cols.Length / 2f) * 4f, 9, 0);
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
        if (!_craneCarryingItem)
        {
            _craneCarryingItem = true;
            _cols[_craneCol] = _cols[_craneCol] - 1;
        }
        UpdateCraneItems();

    }

    public void Drop()
    {
        if (_craneCarryingItem)
        {
            _craneCarryingItem = false;
            _cols[_craneCol] = _cols[_craneCol] + 1;
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
