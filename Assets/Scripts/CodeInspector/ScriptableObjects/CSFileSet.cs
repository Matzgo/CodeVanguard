using System.Collections.Generic;
using UnityEngine;



namespace CodeInspector
{
    public enum TaskType
    {
        Corruption, //Have to write missing code
        Verification, //Have to verify if shown code does what the task describes
    }





    [CreateAssetMenu(fileName = "FileSet", menuName = "Game/CSFileSet")]
    public class CSFileSet : ScriptableObject
    {
        [SerializeField]
        string _taskId;
        public string TaskID => _taskId;

        [SerializeField]
        StructureFeedbackTypes _structureFeedbackTypes;
        public StructureFeedbackTypes StructureFeedbackTypes => _structureFeedbackTypes;


        [SerializeField]
        string _onPluggedInFeedbackKey;
        public string OnPluggedInFeedbackKey => _onPluggedInFeedbackKey;

        [SerializeField]
        private TaskType _taskType;
        public TaskType TaskType => _taskType;


        [SerializeField]
        ScenarioType _scenarioType;
        public ScenarioType ScenarioType => _scenarioType;

        [SerializeField]
        string _title;
        public string Title => _title;

        [SerializeField]
        [TextArea(1, 10)]
        private string _description;
        public string Description => _description;




        [SerializeField]
        string _fileSetName;
        public string FileSetName => _fileSetName;

        [SerializeField]
        CSFile _entryPointFile;
        public CSFile EntryPointFile => _entryPointFile;

        [SerializeField]
        [TextArea(1, 15)]

        string _entryPoint;
        public string EntryPoint => _entryPoint;


        [SerializeField]
        [TextArea(1, 15)]
        List<string> _testCaseCalls;


        [SerializeField]
        [TextArea(10, 20)]
        private string _testMethod;
        public string TestMethod => _testMethod;

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

        public string GetTaskTypeString()
        {
            switch (_taskType)
            {
                case TaskType.Corruption:
                    return "CORRUPTION";
                case TaskType.Verification:
                    return "VERIFICATION";

                default:
                    return "";
            };

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

