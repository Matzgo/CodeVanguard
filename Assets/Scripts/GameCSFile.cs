using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "GameCSFile", menuName = "Game/GameCSFile")]
    public class GameCSFile : ScriptableObject
    {
        [SerializeField]
        string _fileName;
        public string FileName => _fileName;

        [SerializeField]
        List<GameCSFileCodeSegment> _codeSegments;


        public string Text
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


    [System.Serializable]
    public class GameCSFileCodeSegment
    {
        [SerializeField]
        bool _isEditable;
        [SerializeField]
        [TextArea(5, 10)]
        string _text;
        public string Text => _text;
    }
}

