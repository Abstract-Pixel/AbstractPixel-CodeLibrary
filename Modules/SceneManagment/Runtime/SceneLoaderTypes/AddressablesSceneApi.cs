using System;
using System.Threading.Tasks;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The real Addressables calls behind <see cref="IAddressablesSceneApi"/>.</summary>
    internal sealed class AddressablesSceneApi : IAddressablesSceneApi
    {
        // Loads under way by GUID, for the loading progress.
        private readonly System.Collections.Generic.Dictionary<string, AsyncOperationHandle<SceneInstance>> loadsUnderWay = new System.Collections.Generic.Dictionary<string, AsyncOperationHandle<SceneInstance>>();

        public async Task<ISceneHandle> LoadScene(SceneReference _scene, LoadSceneMode _mode, bool _activateOnLoad)
        {
            // The handle is only released by hand: Addressables' own release on scene unload matches the scene it reported,
            // which is wrong for a held scene (see AddressablesSceneHandle).
            AsyncOperationHandle<SceneInstance> operation = Addressables.LoadSceneAsync(_scene.SceneGuid, _mode, SceneReleaseMode.OnlyReleaseSceneOnHandleRelease, _activateOnLoad);
            loadsUnderWay[_scene.SceneGuid] = operation;
            try
            {
                await WaitFor(operation);
            }
            finally
            {
                loadsUnderWay.Remove(_scene.SceneGuid);
            }
            return new AddressablesSceneHandle(_scene, operation);
        }

        public float LoadProgress(SceneReference _scene)
        {
            if (loadsUnderWay.TryGetValue(_scene.SceneGuid, out AsyncOperationHandle<SceneInstance> operation) && operation.IsValid())
            {
                return operation.PercentComplete;
            }
            return 0f;
        }

        public async Task Activate(ISceneHandle _handle)
        {
            AddressablesSceneHandle handle = (AddressablesSceneHandle)_handle;
            AsyncOperation activation = handle.Operation.Result.ActivateAsync();
            await activation.AsTask();
        }

        public async Task Unload(ISceneHandle _handle)
        {
            AddressablesSceneHandle handle = (AddressablesSceneHandle)_handle;
            Scene openScene = handle.FindOpenScene();
            if (openScene.IsValid() && openScene.isLoaded)
            {
                AsyncOperation close = SceneManager.UnloadSceneAsync(openScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                if (close != null)
                {
                    await close.AsTask();
                }
            }

            // Releasing the handle lets the scene's assets leave memory.
            if (handle.Operation.IsValid())
            {
                Addressables.Release(handle.Operation);
            }
        }

        public void SetActive(ISceneHandle _handle)
        {
            AddressablesSceneHandle handle = (AddressablesSceneHandle)_handle;
            SceneManager.SetActiveScene(handle.FindOpenScene());
        }

        // AsyncOperationHandle.Task is avoided: a completion callback also works in the browser.
        private static Task WaitFor(AsyncOperationHandle<SceneInstance> _operation)
        {
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            if (_operation.IsDone)
            {
                FinishWaiting(_operation, completion);
                return completion.Task;
            }

            _operation.Completed += finished => FinishWaiting(finished, completion);
            return completion.Task;
        }

        private static void FinishWaiting(AsyncOperationHandle<SceneInstance> _operation, TaskCompletionSource<bool> _completion)
        {
            if (_operation.Status == AsyncOperationStatus.Succeeded)
            {
                _completion.SetResult(true);
                return;
            }

            Exception failure = _operation.OperationException;
            if (failure == null)
            {
                failure = new InvalidOperationException("The Addressables scene operation failed.");
            }
            _completion.SetException(failure);
        }
    }
}
