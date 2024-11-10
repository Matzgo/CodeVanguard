using UnityEngine;

namespace CodeInspector
{
    [CreateAssetMenu(fileName = "StaticCSFile", menuName = "Game/StaticCSFile")]
    public class StaticCSFile : CSFile
    {

        [SerializeField]
        [TextArea(5, 40)]
        string _text;

        public override string Text => _text;
    }

}

