using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Closes and opens the scenes of a <see cref="SceneChangePlan"/> through the scene loader, which is created here
    /// and nowhere else. It never decides what to open: the plan does.
    /// </summary>
    internal sealed class SceneOpening
    {
        private ISceneLoader loader;

        /// <summary>The loader, created the first time it is needed, so a scene change in the first frame has one.</summary>
        public ISceneLoader Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = new SceneLoaderSelector();
                }
                return loader;
            }
            set { loader = value; }
        }

        /// <summary>
        /// Closes the old main scene and the shared scenes the plan closes, then frees memory once. Manager scenes are
        /// never closed; they close when the game quits.
        /// </summary>
        /// <remarks>Raises <see cref="SceneEventBus.OnSceneGroupUnloaded"/> with the group that closed.</remarks>
        public async Task CloseOldScenes(SceneChangePlan _plan)
        {
            try
            {
                SceneReference mainSceneToClose = _plan.MainSceneToClose;
                if (mainSceneToClose == null || string.IsNullOrEmpty(mainSceneToClose.SceneName))
                {
                    // No main scene on record: close the scene Unity says is active. Only the loader closes scenes.
                    mainSceneToClose = new SceneReference(SceneManager.GetActiveScene().name);
                }
                await Loader.UnloadScene(mainSceneToClose);

                foreach (SceneReference scene in _plan.ContextualToClose)
                {
                    await Loader.UnloadScene(scene);
                }

                await Loader.ReleaseUnusedMemory();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (_plan.GroupThatCloses != null)
            {
                SceneEventBus.RaiseOnSceneGroupUnloaded(_plan.GroupThatCloses);
            }
        }

        /// <summary>
        /// Opens the plan's scenes. A scene change opens manager and shared scenes one by one, then the main scene.
        /// Start-up asks for every scene at once with the main scene first: Unity opens additive scenes in the order they
        /// were asked for, so the main scene still becomes active before the others wake, and every scene is asked for
        /// before the first frame's Start.
        /// </summary>
        /// <param name="_activateMainScene">False holds the main scene back (preload). Other scenes always activate.</param>
        /// <returns>False when a scene failed to load. The failing scene is logged.</returns>
        public async Task<bool> OpenScenes(SceneChangePlan _plan, bool _activateMainScene, bool _isStartUp)
        {
            if (!_isStartUp)
            {
                bool sharedScenesLoaded = await OpenSharedScenes(_plan, false);
                if (!sharedScenesLoaded)
                {
                    return false;
                }
                return await TryLoadScene(_plan.MainSceneToOpen, _activateMainScene, true);
            }

            Task<bool> mainSceneLoad = Task.FromResult(true);
            if (_plan.MainSceneToOpen != null)
            {
                mainSceneLoad = TryLoadScene(_plan.MainSceneToOpen, true, true);
            }
            Task<bool> sharedScenesLoad = OpenSharedScenes(_plan, true);
            bool[] results = await Task.WhenAll(mainSceneLoad, sharedScenesLoad);
            return results[0] && results[1];
        }

        /// <summary>
        /// A switch to a preloaded group: the held main scene activates first, because Unity holds every later scene
        /// operation while it waits; then the old scenes close and the missing shared scenes open. Scenes the preload
        /// opened stay as they are.
        /// </summary>
        public async Task<bool> SwitchToPreloaded(SceneChangePlan _plan)
        {
            bool mainSceneActivated = await TryLoadScene(_plan.MainSceneToOpen, true, true);
            if (!mainSceneActivated)
            {
                return false;
            }
            await CloseOldScenes(_plan);
            return await OpenSharedScenes(_plan, false);
        }

        /// <summary>Opens the manager scenes, then the shared scenes: one by one, or all asked for at once.</summary>
        private async Task<bool> OpenSharedScenes(SceneChangePlan _plan, bool _askAllAtOnce)
        {
            List<Task<bool>> loads = new List<Task<bool>>();
            foreach (SceneReference scene in _plan.ManagerialToOpen.Concat(_plan.ContextualToOpen))
            {
                Task<bool> load = TryLoadScene(scene, true, false);
                if (_askAllAtOnce)
                {
                    loads.Add(load);
                    continue;
                }
                if (!await load)
                {
                    return false;
                }
            }

            bool[] results = await Task.WhenAll(loads);
            return Array.TrueForAll(results, loaded => loaded);
        }

        private async Task<bool> TryLoadScene(SceneReference _scene, bool _activate, bool _isMainScene)
        {
            try
            {
                await Loader.LoadScene(_scene, true, _activate, _isMainScene);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[SceneCoordinator] Failed to load scene '{_scene.SceneName}'.");
                return false;
            }
        }
    }
}
