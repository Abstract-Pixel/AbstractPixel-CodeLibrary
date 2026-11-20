using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples
{
    /// <summary>
    /// Loads a sample scene by name in play mode, wherever the samples were imported, without adding it to the build's
    /// scene list. In your game, use <c>SceneManager.LoadScene(name)</c> instead: the samples are Editor-only.
    /// </summary>
    public static class SampleScenes
    {
        /// <summary>Loads the open scene again, like <c>SceneManager.LoadScene(SceneManager.GetActiveScene().name)</c>.</summary>
        public static void Reload()
        {
            Load(SceneManager.GetActiveScene().name);
        }

        /// <summary>Loads a sample scene, like <c>SceneManager.LoadScene(sceneName)</c>.</summary>
        public static void Load(string sceneName)
        {
            EditorSceneManager.LoadSceneInPlayMode(FindPath(sceneName), new LoadSceneParameters(LoadSceneMode.Single));
        }

        /// <summary>For tests: loads a sample scene and waits until every Start in it has run.</summary>
        public static IEnumerator LoadAndWait(string sceneName)
        {
            AsyncOperation loading = EditorSceneManager.LoadSceneAsyncInPlayMode(FindPath(sceneName), new LoadSceneParameters(LoadSceneMode.Single));
            while (!loading.isDone)
            {
                yield return null;
            }
            // One more frame, so every Start has run.
            yield return null;
        }

        static string FindPath(string sceneName)
        {
            string path = AssetDatabase.FindAssets("t:Scene " + sceneName)
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(candidate => Path.GetFileNameWithoutExtension(candidate) == sceneName);
            if (path == null)
            {
                throw new InvalidOperationException($"Can't find the sample scene '{sceneName}'. The samples ship in the Abstract Pixel Library package under Modules/SaveSystem/Samples.");
            }
            return path;
        }
    }
}
