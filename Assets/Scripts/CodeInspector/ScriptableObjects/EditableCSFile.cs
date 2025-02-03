using UnityEngine;

namespace CodeInspector
{
    [CreateAssetMenu(fileName = "EditableCSFile", menuName = "Game/EditableCSFile")]
    public class EditableCSFile : CSFile
    {
        [SerializeField]
        [TextArea(10, 50)]
        string _beforeText;
        [SerializeField]
        [TextArea(10, 50)]
        string _input;
        [TextArea(10, 50)]
        [SerializeField]
        string _afterText;

        public string BeforeText => _beforeText;
        public string Input => _input;
        public string AfterText => _afterText;

        //[SerializeField]
        //List<CSFileCodeSegment> _codeSegments;



        public override string Text
        {
            get
            {
                return _beforeText + '\n' + _input + '\n' + _afterText;


            }

        }
    }
}