using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor script to ensure that the 'Main' scene opens automatically
/// when the project is opened for the first time or the editor starts.
/// </summary>
[InitializeOnLoad]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        // We only want to open the scene automatically if the editor has just loaded
        // and there is no open scene or the current scene is the default empty scene.
        EditorApplication.delayCall += OnEditorLoaded;
    }

    private static void OnEditorLoaded()
    {
        // If there is already an open scene with a path (it's not a new unsaved scene),
        // we respect the user's selection.
        if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
        {
            string scenePath = "Assets/_Complete-Game.unity";
            
            // Verify that the scene exists before attempting to open it
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            {
                Debug.Log("[DefaultSceneLoader] Opening default scene: " + scenePath);
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }
}
