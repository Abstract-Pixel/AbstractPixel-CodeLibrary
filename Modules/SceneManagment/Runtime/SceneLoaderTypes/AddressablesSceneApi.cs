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
        public async Task<ISceneHandle> LoadScene(string _key, LoadSceneMode _mode, bool _activateOnLoad)
        {
            AsyncOperationHandle<SceneInstance> operation = Addressables.LoadSceneAsync(_key, _mode, _activateOnLoad);
            await WaitFor(operation);
            return new AddressablesSceneHandle(_key, operation);
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
            // The last argument releases the handle once the scene has closed.
            AsyncOperationHandle<SceneInstance> unload = Addressables.UnloadSceneAsync(handle.Operation, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects, true);
            await WaitFor(unload);
        }

        public void SetActive(ISceneHandle _handle)
        {
            AddressablesSceneHandle handle = (AddressablesSceneHandle)_handle;
            SceneManager.SetActiveScene(handle.Operation.Result.Scene);
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
