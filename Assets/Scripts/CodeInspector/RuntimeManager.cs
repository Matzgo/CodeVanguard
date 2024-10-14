using UnityEngine;

namespace CodeInspector
{
    public class RuntimeManager : MonoBehaviour
    {
        public static RuntimeManager Instance { get; private set; }

        [SerializeField]
        UIConsole _console;
        public UIConsole Console => _console;

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

    }
}
