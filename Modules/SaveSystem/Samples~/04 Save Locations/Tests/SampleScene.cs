using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.SaveLocations.Tests
{
    // Loads a sample scene by name, wherever the sample was imported, without needing it in the build's scene list.
    internal static class SampleScene
    {
        public static IEnumerator Load(string sceneName)
        {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.FindAssets("t:Scene " + sceneName)
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(candidate => Path.GetFileNameWithoutExtension(candidate) == sceneName);
            Assert.IsNotNull(path, $"Can't find the sample scene '{sceneName}'. Import the sample first.");

            AsyncOperation loading = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            while (!loading.isDone)
            {
                yield return null;
            }
            // One more frame, so every Start has run.
            yield return null;
#else
            Assert.Ignore("Sample tests run in the Unity Editor.");
            yield break;
#endif
        }
    }
}
