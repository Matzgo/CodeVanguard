using UnityEngine;

namespace CodeInspector
{
    public class RuntimeManager : MonoBehaviour
    {
        public static RuntimeManager Instance { get; private set; }

        [SerializeField]
        UIConsole _console;
        public UIConsole Console => _console;

        [SerializeField]
        CustomCodeEditor _customCodeEditor;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //SubscribeToConsoleEvents();  // Subscribe to the _console events
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void HighlightLine(int lineNr)
        {
            //Debug.Log("Highlight");
            _customCodeEditor.HightlightRunningLine(lineNr);
        }

        public void DisableHighlightLine()
        {
            _customCodeEditor.DisableRunningLine();
        }

    }
}
