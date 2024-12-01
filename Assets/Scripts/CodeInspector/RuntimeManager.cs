using System;
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

        public Action<string> GameEvent;
        public Action MiniGameReset;

        private bool _active;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ResetMiniGame()
        {
            MiniGameReset?.Invoke();
        }

        public void Trigger(string key)
        {
            if (_active)
                GameEvent?.Invoke(key);
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

        public void Disable()
        {
            _active = false;
        }

        public void Enable()
        {
            _active = true;

        }
    }
}
