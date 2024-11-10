using UnityEngine;

namespace CodeInspector
{
    public abstract class CSFile : ScriptableObject
    {
        [SerializeField]
        protected string _fileName;
        public string FileName => _fileName;

        public abstract string Text { get; }
    }


    [System.Serializable]
    public class CSFileCodeSegment
    {
        [SerializeField]
        bool _isEditable;
        [SerializeField]
        [TextArea(5, 10)]
        string _text;
        public string Text => _text;
    }
}

