using AbstractPixel.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    /// <summary>Opens and closes scenes that are in Build Settings, or known only by name, with Unity's SceneManager.</summary>
    public class DefaultSceneLoader : ISceneLoader
    {
        /// <summary>True for a scene of a sample scene group, which is never in Build Settings.</summary>
        /// <remarks>
        /// - Set by the scene system's editor code; null in a build.
        /// - Such a scene is opened in Play mode with EditorSceneManager.LoadSceneAsyncInPlayMode.
        /// </remarks>
        internal static System.Func<string, bool> IsEditorOnlySampleScene;

        /// <summary>True when the Editor opens this scene from its file because Build Settings does not list it.</summary>
        internal static bool OpensFromItsFile(SceneReference _scene)
        {
            return _scene != null && !string.IsNullOrEmpty(_scene.ScenePath) && scenesOpenedFromTheirFile.Contains(_scene.ScenePath);
        }

        /// <summary>Remembers the main scene of a temporary scene group, so a Retry can open it again in the Editor.</summary>
        internal static void OpenFromItsFile(SceneReference _scene)
        {
            if (_scene != null && !string.IsNullOrEmpty(_scene.ScenePath))
            {
                scenesOpenedFromTheirFile.Add(_scene.ScenePath);
            }
        }

        private static readonly HashSet<string> scenesOpenedFromTheirFile = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ForgetScenesOpenedFromTheirFile()
        {
            scenesOpenedFromTheirFile.Clear();
        }

        private Dictionary<string, AsyncOperation> sceneLoadHandles = new Dictionary<string, AsyncOperation>();

        // For the loading progress: loads under way, and scenes that have loaded and not closed yet.
        private readonly Dictionary<string, AsyncOperation> loadsUnderWay = new Dictionary<string, AsyncOperation>();
        private readonly HashSet<string> loadedScenes = new HashSet<string>();

        public float GetLoadProgress(SceneReference sceneReference)
        {
            if (sceneReference == null)
            {
                return 0f;
            }
            string sceneName = sceneReference.SceneName;
            if (loadedScenes.Contains(sceneName))
            {
                return 1f;
            }
            if (loadsUnderWay.TryGetValue(sceneName, out AsyncOperation operation))
            {
                // Unity stops at 0.9 until the scene activates.
                return Mathf.Clamp01(operation.progress / 0.9f);
            }
            return 0f;
        }
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            if (sceneReference == null || string.IsNullOrEmpty(sceneReference.SceneName))
            {
                throw new System.ArgumentException("[DefaultSceneLoader] No scene was given to load.");
            }

            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out AsyncOperation operationHandle))
            {
                // The scene is already loaded and held back by an earlier preload. Asking for it again activates it.
                if (sceneActivatedByDefault && !operationHandle.allowSceneActivation)
                {
                    await ActivateHeldScene(sceneReference, operationHandle);
                    if (isMainScene)
                    {
                        MakeActiveMainScene(sceneReference);
                    }
                }
                return;
            }

            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation sceneLoadHandle = StartLoad(sceneReference, loadMode);
            if (sceneLoadHandle == null)
            {
                throw new System.InvalidOperationException($"Scene '{sceneReference.SceneName}' could not be loaded.");
            }
            sceneLoadHandle.allowSceneActivation = sceneActivatedByDefault;
            loadsUnderWay[sceneReference.SceneName] = sceneLoadHandle;

            if (sceneLoadHandle.allowSceneActivation)
            {
                await sceneLoadHandle.AsTask();
                if (isMainScene)
                {
                    MakeActiveMainScene(sceneReference);
                }
            }
            else
            {
                sceneLoadHandles.Add(sceneReference.SceneName, sceneLoadHandle);
                while (sceneLoadHandle.progress < 0.9f)
                {
                    await Task.Yield();
                }
            }
            loadsUnderWay.Remove(sceneReference.SceneName);
            loadedScenes.Add(sceneReference.SceneName);
        }

        // A scene missing from the build fails the load, so the scene change fails instead of looking finished. Checked
        // first, so Unity does not log an error of its own for the same scene. A sample scene opens from its path, in
        // the Editor only.
        private static AsyncOperation StartLoad(SceneReference _sceneReference, LoadSceneMode _loadMode)
        {
            string sceneName = _sceneReference.SceneName;
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return SceneManager.LoadSceneAsync(sceneName, _loadMode);
            }
#if UNITY_EDITOR
            string scenePath = _sceneReference.ScenePath;
            bool isSampleScene = IsEditorOnlySampleScene != null && !string.IsNullOrEmpty(scenePath) && IsEditorOnlySampleScene(scenePath);
            bool isSceneInNoGroup = OpensFromItsFile(_sceneReference);
            if (isSceneInNoGroup)
            {
                Debug.LogWarning($"[Scene system] '{sceneName}' is not in Build Settings, so the Editor opens it from its file. A build needs the entry.");
            }
            if (isSampleScene || isSceneInNoGroup)
            {
                LoadSceneParameters parameters = new LoadSceneParameters(_loadMode);
                return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, parameters);
            }
#endif
            throw new System.InvalidOperationException($"Scene '{sceneName}' is not in the build (Build Settings or Addressables).");
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
            if (sceneReference == null || string.IsNullOrEmpty(sceneReference.SceneName))
            {
                Debug.LogWarning("[DefaultSceneLoader] No scene was given to close.");
                return;
            }
            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out AsyncOperation heldHandle))
            {
                // Unity cannot cancel a held load and the scene is not unloadable until it has activated. It opens
                // without becoming the active scene, and is closed again below.
                await ActivateHeldScene(sceneReference, heldHandle);
            }

            loadedScenes.Remove(sceneReference.SceneName);
            AsyncOperation sceneUnLoadHandle = SceneManager.UnloadSceneAsync(sceneReference.SceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            if (sceneUnLoadHandle == null)
            {
                return;
            }
            await sceneUnLoadHandle.AsTask();
        }

        private async Task ActivateHeldScene(SceneReference _sceneReference, AsyncOperation _heldHandle)
        {
            sceneLoadHandles.Remove(_sceneReference.SceneName);
            _heldHandle.allowSceneActivation = true;
            await _heldHandle.AsTask();
        }

        private static void MakeActiveMainScene(SceneReference _sceneReference)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneReference.SceneName));
            SceneEventBus.RaiseOnMainSceneLoaded(_sceneReference);
        }

        public async Task ReleaseUnusedMemory()
        {
            await Resources.UnloadUnusedAssets().AsTask();
        }
    }
}
