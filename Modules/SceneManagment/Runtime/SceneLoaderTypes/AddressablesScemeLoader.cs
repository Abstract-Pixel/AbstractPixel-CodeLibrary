using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    public class AddressablesSceneLoader : ISceneLoader
    {
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            throw new NotImplementedException();
        }

        public Task UnloadScene(SceneReference sceneReference)
        {
            throw new NotImplementedException();
        }

        public async Task ReleaseUnusedMemory()
        {
            await Resources.UnloadUnusedAssets().AsTask();
        }
    }
}