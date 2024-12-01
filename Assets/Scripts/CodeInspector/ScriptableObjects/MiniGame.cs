using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "MiniGame", menuName = "Game/MiniGame")]

public abstract class MiniGame : ScriptableObject
{
    [SerializeField]
    private string _sceneName; // Scene name to reference for runtime loading

    public string SceneName => _sceneName; // Expose the scene name publicly


    [SerializeField]
    private List<string> _whitelistedDirectives;
    public List<string> WhitelistedDirectives => _whitelistedDirectives;

    // Load the scene additively
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(_sceneName))
        {
            if (!SceneManager.GetSceneByName(_sceneName).isLoaded)
            {
                SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogWarning($"Scene '{_sceneName}' is already loaded.");
            }
        }
        else
        {
            Debug.LogError("Scene name is not set!");
        }
    }

    // Unload the scene
    public void UnloadScene()
    {
        if (!string.IsNullOrEmpty(_sceneName))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(_sceneName);
            if (sceneToUnload.isLoaded)
            {
                SceneManager.UnloadSceneAsync(_sceneName);
            }
            else
            {
                Debug.LogWarning($"Scene '{_sceneName}' is not currently loaded.");
            }
        }
        else
        {
            Debug.LogError("Scene name is not set!");
        }
    }
}
