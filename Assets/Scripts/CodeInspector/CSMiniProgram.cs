using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "MiniProgram", menuName = "Game/CSMiniProgram")]
    public class CSMiniProgram : ScriptableObject
    {
        [SerializeField]
        string _programName;
        public string ProgramName => _programName;

        [SerializeField]
        List<CSFile> _csFiles;

    }
}

