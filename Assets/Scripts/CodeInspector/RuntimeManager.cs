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

        public Action<string> MiniGameEvent;
        public Action MiniGameReset;

        public Action<string> WorldGameEvent;
        public Action WorldGameReset;


        //All UserCode Coroutines run on the CoroutineRunner
        CoroutineRunner _coroutineRunner;
        public CoroutineRunner CoroutineRunner;

        private bool _activeMiniGame;
        private bool _activeWorld = true;

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

        public void ResetWorld()
        {
            WorldGameReset?.Invoke();
        }

        public void Trigger(string key)
        {
            if (_activeMiniGame)
                MiniGameEvent?.Invoke(key);

            if (_activeWorld)
                WorldGameEvent?.Invoke(key);
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


        public void DisableWorldGame()
        {
            _activeWorld = false;
        }

        public void EnableWorldGame()
        {
            _activeWorld = true;

        }

        public void DisableMiniGame()
        {
            _activeMiniGame = false;
        }

        public void EnableMiniGame()
        {
            _activeMiniGame = true;

        }
    }
}
