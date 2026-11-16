using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Logs a loaded scene's saved scripts with no Savable Bridge when Play is pressed, instead of adding one there.</summary>
    [InitializeOnLoad]
    public static class SavableBridgePlayModeCheck
    {
        static SavableBridgePlayModeCheck()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                CheckScene(SceneManager.GetSceneAt(index));
            }
        }

        /// <summary>A test fixture calls this directly, instead of needing to enter play mode to exercise it.</summary>
        internal static void CheckScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return;
            }

            List<GameObject> missing = SavableBridgeSceneCheck.FindMissingBridges(scene);
            if (missing.Count > 0)
            {
                SaveLog.Error(SavableBridgeSceneCheck.Describe(scene, missing, "press Play again"));
            }
        }
    }
}
