using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RenderTextureAutoUpdater
{
    private static float lastUpdateTime;

    static RenderTextureAutoUpdater()
    {
        // Subscribe to the update event
        EditorApplication.update += UpdateRenderTexture;
        lastUpdateTime = Time.realtimeSinceStartup; // Initialize timing
    }

    private static void UpdateRenderTexture()
    {
        // Skip updates during play mode
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            return;

        // Check if one second has passed
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastUpdateTime < 1f)
            return; // Wait until 1 second has elapsed

        lastUpdateTime = currentTime; // Reset the timer

        // Find all Render Textures in the project
        var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();

        foreach (var rt in renderTextures)
        {
            if (rt.IsCreated())
            {
                rt.Release(); // Release the existing Render Texture
                rt.Create();  // Re-create the Render Texture
            }
        }

        // Force repaint of the scene view
        SceneView.RepaintAll();
    }
}
