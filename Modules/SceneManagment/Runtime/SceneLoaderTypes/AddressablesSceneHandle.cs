using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The Addressables operation behind one open scene. Releasing it is what lets the scene's assets unload.</summary>
    internal sealed class AddressablesSceneHandle : ISceneHandle
    {
        public string Key { get; }
        public AsyncOperationHandle<SceneInstance> Operation { get; }

        public AddressablesSceneHandle(string _key, AsyncOperationHandle<SceneInstance> _operation)
        {
            Key = _key;
            Operation = _operation;
        }
    }
}
