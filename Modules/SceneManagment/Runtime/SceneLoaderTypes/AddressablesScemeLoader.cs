using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Opens and closes scenes that are Addressable. It follows the held scene contract on <see cref="ISceneLoader"/>,
    /// and keeps the Addressables handle of every open scene so a level's assets leave memory when it closes.
    /// A scene is known to Addressables by its asset GUID, so a reference without one is refused.
    /// </summary>
    public class AddressablesSceneLoader : ISceneLoader
    {
        private class OpenScene
        {
            public ISceneHandle Handle;
            public bool IsHeld;
        }

        private readonly IAddressablesSceneApi api;
        private readonly Dictionary<string, OpenScene> openScenes = new Dictionary<string, OpenScene>();

        public AddressablesSceneLoader() : this(new AddressablesSceneApi())
        {
        }

        internal AddressablesSceneLoader(IAddressablesSceneApi _api)
        {
            api = _api;
        }

        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            if (sceneReference == null || !sceneReference.HasGuid)
            {
                Debug.LogError("[AddressablesSceneLoader] A scene needs its asset GUID to load through Addressables.");
                return;
            }

            string key = sceneReference.SceneGuid;
            if (openScenes.TryGetValue(key, out OpenScene alreadyOpen))
            {
                // Rule 3: asking again for the held scene activates it.
                if (alreadyOpen.IsHeld && sceneActivatedByDefault)
                {
                    await ActivateHeldScene(alreadyOpen);
                    if (isMainScene)
                    {
                        MakeActiveMainScene(sceneReference, alreadyOpen);
                    }
                }
                return;
            }

            LoadSceneMode loadMode = LoadSceneMode.Single;
            if (isAdditive)
            {
                loadMode = LoadSceneMode.Additive;
            }

            // Rule 1: without activation the scene is held, and the handle comes back once it is ready to activate.
            ISceneHandle handle = await api.LoadScene(key, loadMode, sceneActivatedByDefault);
            OpenScene openScene = new OpenScene { Handle = handle, IsHeld = !sceneActivatedByDefault };
            openScenes[key] = openScene;

            if (sceneActivatedByDefault && isMainScene)
            {
                MakeActiveMainScene(sceneReference, openScene);
            }
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
            if (sceneReference == null || !sceneReference.HasGuid)
            {
                Debug.LogError("[AddressablesSceneLoader] A scene needs its asset GUID to unload through Addressables.");
                return;
            }

            string key = sceneReference.SceneGuid;
            if (!openScenes.TryGetValue(key, out OpenScene openScene))
            {
                return;
            }

            // Rule 4: a held scene cannot close until it has activated. It does not become the active scene.
            if (openScene.IsHeld)
            {
                await ActivateHeldScene(openScene);
            }

            // The API releases the handle here, which lets the scene's assets leave memory.
            await api.Unload(openScene.Handle);
            openScenes.Remove(key);
        }

        public async Task ReleaseUnusedMemory()
        {
            await Resources.UnloadUnusedAssets().AsTask();
        }

        private async Task ActivateHeldScene(OpenScene _openScene)
        {
            _openScene.IsHeld = false;
            await api.Activate(_openScene.Handle);
        }

        private void MakeActiveMainScene(SceneReference _sceneReference, OpenScene _openScene)
        {
            api.SetActive(_openScene.Handle);
            SceneEventBus.RaiseOnMainSceneLoaded(_sceneReference);
        }
    }
}
