using System.Collections.Generic;
using UnityEngine;

namespace CodeInspector
{
    [CreateAssetMenu(fileName = "EditableCSFile", menuName = "Game/EditableCSFile")]
    public class EditableCSFile : CSFile
    {

        [SerializeField]
        List<CSFileCodeSegment> _codeSegments;



        public override string Text
        {
            get
            {
                if (_codeSegments == null || _codeSegments.Count == 0)
                    return null;
                else
                {
                    string txt = "";
                    for (int i = 0; i < _codeSegments.Count; i++)
                    {
                        txt += _codeSegments[i].Text;
                    }
                    return txt;
                }



            }

        }
    }
}