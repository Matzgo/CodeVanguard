using TMPro;
using UnityEngine;


public class ReadOnlyCodeSegment : CodeSegment
{
    public override bool IsEditable => true;

    public override string Text
    {
        get
        {
            if (_tmpText == null)
                return null;
            else return _tmpText.text;
        }
    }

    [SerializeField]
    TextMeshProUGUI _tmpText;
    public TextMeshProUGUI TmpText => _tmpText;
}
