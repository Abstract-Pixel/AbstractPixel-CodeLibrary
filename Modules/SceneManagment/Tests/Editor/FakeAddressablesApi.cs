using System.Collections.Generic;
using AbstractPixel.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A stand-in for Addressables that follows Unity's held-scene rule: while a scene is held, any load or unload of a
    /// different scene never finishes, and a held scene cannot close until it has activated.
    /// </summary>
    internal sealed class FakeAddressablesApi : IAddressablesSceneApi
    {
        private sealed class FakeHandle : ISceneHandle
        {
            public string Key { get; }
            public bool IsReleased;

            public FakeHandle(string _key)
            {
                Key = _key;
            }
        }

        public readonly List<string> ActivatedKeys = new List<string>();
        public readonly List<string> ClosedKeys = new List<string>();

        /// <summary>Every key that was loaded, in order, so a test can see a scene opened twice.</summary>
        public readonly List<string> LoadedKeys = new List<string>();

        public string HeldKey { get; private set; }
        public string ActiveKey { get; private set; }

        /// <summary>The handles that were handed out and not yet released by an unload.</summary>
        public int HandlesInUse { get; private set; }

        public Task<ISceneHandle> LoadScene(SceneReference _scene, LoadSceneMode _mode, bool _activateOnLoad)
        {
            string key = _scene.SceneGuid;
            LoadedKeys.Add(key);
            if (IsStalledByHeldScene(key))
            {
                return NeverCompletes<ISceneHandle>();
            }

            if (!_activateOnLoad)
            {
                HeldKey = key;
            }

            HandlesInUse++;
            ISceneHandle handle = new FakeHandle(key);
            return Task.FromResult(handle);
        }

        public Task Activate(ISceneHandle _handle)
        {
            ActivatedKeys.Add(_handle.Key);
            if (HeldKey == _handle.Key)
            {
                HeldKey = null;
            }

            return Task.CompletedTask;
        }

        public Task Unload(ISceneHandle _handle)
        {
            bool isHeldAndNotActivated = HeldKey == _handle.Key;
            if (isHeldAndNotActivated || IsStalledByHeldScene(_handle.Key))
            {
                return NeverCompletes<bool>();
            }

            FakeHandle handle = (FakeHandle)_handle;
            if (!handle.IsReleased)
            {
                handle.IsReleased = true;
                HandlesInUse--;
            }

            ClosedKeys.Add(_handle.Key);
            if (ActiveKey == _handle.Key)
            {
                ActiveKey = null;
            }

            return Task.CompletedTask;
        }

        public void SetActive(ISceneHandle _handle)
        {
            ActiveKey = _handle.Key;
        }

        private bool IsStalledByHeldScene(string _key)
        {
            return HeldKey != null && HeldKey != _key;
        }

        private static Task<T> NeverCompletes<T>()
        {
            TaskCompletionSource<T> never = new TaskCompletionSource<T>();
            return never.Task;
        }
    }
}
