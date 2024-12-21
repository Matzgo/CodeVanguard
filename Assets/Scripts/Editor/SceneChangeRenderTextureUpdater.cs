using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RenderTextureAutoUpdater
{
    private static float lastUpdateTime;
    const bool disabled = true;
    static RenderTextureAutoUpdater()
    {
        // Subscribe to the update event
        EditorApplication.update += UpdateRenderTexture;
        lastUpdateTime = Time.realtimeSinceStartup; // Initialize timing
    }

    private static void UpdateRenderTexture()
    {
        if (disabled)
            return;

        // Skip updates during play mode
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            return;

        // Check if one second has passed
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastUpdateTime < 15f)
            return; // Wait until 1 second has elapsed

        lastUpdateTime = currentTime; // Reset the timer

        // Find all Render Textures in the project
        var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
        if (renderTextures == null || renderTextures.Length == 0)
            return;

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
