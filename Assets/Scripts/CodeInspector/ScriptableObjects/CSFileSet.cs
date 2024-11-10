using System.Collections.Generic;
using UnityEngine;

namespace CodeInspector
{
    [CreateAssetMenu(fileName = "FileSet", menuName = "Game/CSFileSet")]
    public class CSFileSet : ScriptableObject
    {
        [SerializeField]
        string _fileSetName;
        public string FileSetName => _fileSetName;

        [SerializeField]
        CSFile _entryPointFile;
        public CSFile EntryPointFile => _entryPointFile;

        [SerializeField]
        [TextArea(1, 15)]

        string _entryPoint;
        [SerializeField]
        [TextArea(1, 15)]
        List<string> _testCaseCalls;


        [SerializeField]
        List<CSFileSetEntry> _csFiles;
        public List<CSFileSetEntry> CSFiles => _csFiles;



        private void OnValidate()
        {


            for (int i = 0; i < _csFiles.Count; i++)
            {
                //static files dont need solution
                if (_csFiles[i].File != null && _csFiles[i].File is StaticCSFile)
                    _csFiles[i].SetSolutionFile(null);
            }
        }
    }

    [System.Serializable]

    public class CSFileSetEntry
    {
        [SerializeField]
        CSFile _file;
        public CSFile File => _file;


        [SerializeField]
        StaticCSFile _solutionFile;
        public CSFile SolutionFile
        {
            get
            {
                if (_file is StaticCSFile)
                {
                    return _file;
                }
                else
                    return _solutionFile;
            }
        }

        internal void SetSolutionFile(StaticCSFile file)
        {
            _solutionFile = file;
        }
    }

}

