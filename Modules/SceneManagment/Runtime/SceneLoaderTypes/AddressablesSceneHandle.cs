using AbstractPixel.Core;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The Addressables operation behind one open scene. Releasing it is what lets the scene's assets unload.
    /// Addressables reports a held scene's <see cref="SceneInstance.Scene"/> as whatever scene was last in the list when
    /// the load was asked for, because the held scene is not in the list yet. So the scene is looked up again by its
    /// path (or name) whenever the loader needs it.
    /// </summary>
    internal sealed class AddressablesSceneHandle : ISceneHandle
    {
        private readonly SceneReference reference;

        public string Key { get; }
        public AsyncOperationHandle<SceneInstance> Operation { get; }

        public AddressablesSceneHandle(SceneReference _reference, AsyncOperationHandle<SceneInstance> _operation)
        {
            reference = _reference;
            Key = _reference.SceneGuid;
            Operation = _operation;
        }

        /// <summary>The open scene, found by path and then by name. Invalid when it is not open.</summary>
        public Scene FindOpenScene()
        {
            if (!string.IsNullOrEmpty(reference.ScenePath))
            {
                Scene byPath = SceneManager.GetSceneByPath(reference.ScenePath);
                if (byPath.IsValid())
                {
                    return byPath;
                }
            }

            return SceneManager.GetSceneByName(reference.SceneName);
        }
    }
}
