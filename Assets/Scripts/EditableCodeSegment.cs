using UnityEngine;
using UnityEngine.UI;




public class EditableCodeSegment : CodeSegment
{
    public override bool IsEditable => true;

    public override string Text
    {
        get
        {
            if (_inputField == null)
                return null;
            else return _inputField.text;
        }
    }

    [SerializeField]
    InputField _inputField;
    public InputField InputField => _inputField;
}

