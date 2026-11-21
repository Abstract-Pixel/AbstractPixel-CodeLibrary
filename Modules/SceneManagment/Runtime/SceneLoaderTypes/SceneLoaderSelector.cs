using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The loader the coordinator uses. Each scene picks its own loader from the asset's state: an Addressable scene
    /// opens through the <see cref="AddressablesSceneLoader"/>, every other scene through Build Settings. A scene is
    /// closed by the loader that opened it, so a held scene and its handle never change hands.
    /// </summary>
    public class SceneLoaderSelector : ISceneLoader
    {
        private readonly ISceneLoader buildSettingsLoader;
        private readonly ISceneLoader addressablesLoader;
        private readonly IAddressableSceneCheck addressableCheck;

        // Open scenes by GUID and by name, so a close that only knows the name (the active scene) finds its loader.
        private readonly Dictionary<string, ISceneLoader> loaderOfOpenScene = new Dictionary<string, ISceneLoader>();

        public SceneLoaderSelector() : this(new DefaultSceneLoader(), new AddressablesSceneLoader(), new AddressableSceneCheck())
        {
        }

        internal SceneLoaderSelector(ISceneLoader _buildSettingsLoader, ISceneLoader _addressablesLoader, IAddressableSceneCheck _addressableCheck)
        {
            buildSettingsLoader = _buildSettingsLoader;
            addressablesLoader = _addressablesLoader;
            addressableCheck = _addressableCheck;
        }

        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            ISceneLoader loader = await ChooseLoader(sceneReference);
            await loader.LoadScene(sceneReference, isAdditive, sceneActivatedByDefault, isMainScene);
            RememberLoader(sceneReference, loader);
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
            ISceneLoader loader = FindOpenSceneLoader(sceneReference);
            if (loader == null)
            {
                loader = buildSettingsLoader;
            }

            await loader.UnloadScene(sceneReference);
            ForgetLoader(sceneReference);
        }

        /// <summary>The progress from whichever loader knows the scene: an In-build scene is 0 to the Addressables loader and the other way round.</summary>
        public float GetLoadProgress(SceneReference sceneReference)
        {
            float fromBuildSettings = buildSettingsLoader.GetLoadProgress(sceneReference);
            float fromAddressables = addressablesLoader.GetLoadProgress(sceneReference);
            return Mathf.Max(fromBuildSettings, fromAddressables);
        }

        public Task ReleaseUnusedMemory()
        {
            return buildSettingsLoader.ReleaseUnusedMemory();
        }

        private async Task<ISceneLoader> ChooseLoader(SceneReference _scene)
        {
            ISceneLoader openedBy = FindOpenSceneLoader(_scene);
            if (openedBy != null)
            {
                return openedBy;
            }

            // A scene is In build or Addressable, never both. Answering an In-build scene without the Addressables
            // check (which waits for Addressables to start) lets start-up ask for its scenes before any Start runs.
            if (IsInBuildSettings(_scene))
            {
                return buildSettingsLoader;
            }

            bool isAddressable = await addressableCheck.IsAddressable(_scene);
            if (isAddressable)
            {
                Debug.Log("[SceneLoader] '" + _scene.SceneName + "' is Addressable, opening it through Addressables.");
                return addressablesLoader;
            }

            return buildSettingsLoader;
        }

        private static bool IsInBuildSettings(SceneReference _scene)
        {
            if (_scene == null || string.IsNullOrEmpty(_scene.ScenePath))
            {
                return false;
            }
            int buildIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(_scene.ScenePath);
            return buildIndex >= 0;
        }

        private ISceneLoader FindOpenSceneLoader(SceneReference _scene)
        {
            if (_scene == null)
            {
                return null;
            }

            if (_scene.HasGuid && loaderOfOpenScene.TryGetValue(_scene.SceneGuid, out ISceneLoader byGuid))
            {
                return byGuid;
            }

            if (!string.IsNullOrEmpty(_scene.SceneName) && loaderOfOpenScene.TryGetValue(_scene.SceneName, out ISceneLoader byName))
            {
                return byName;
            }

            return null;
        }

        private void RememberLoader(SceneReference _scene, ISceneLoader _loader)
        {
            if (_scene == null)
            {
                return;
            }

            if (_scene.HasGuid)
            {
                loaderOfOpenScene[_scene.SceneGuid] = _loader;
            }

            if (!string.IsNullOrEmpty(_scene.SceneName))
            {
                loaderOfOpenScene[_scene.SceneName] = _loader;
            }
        }

        private void ForgetLoader(SceneReference _scene)
        {
            if (_scene == null)
            {
                return;
            }

            if (_scene.HasGuid)
            {
                loaderOfOpenScene.Remove(_scene.SceneGuid);
            }

            if (!string.IsNullOrEmpty(_scene.SceneName))
            {
                loaderOfOpenScene.Remove(_scene.SceneName);
            }
        }
    }
}
