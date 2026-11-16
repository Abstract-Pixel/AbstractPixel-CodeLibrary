using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The few Addressables calls the <see cref="AddressablesSceneLoader"/> makes. A seam, so the loader's rules can be
    /// tested with a fake that follows Unity's held-scene rule, without opening a real scene.
    /// </summary>
    internal interface IAddressablesSceneApi
    {
        /// <summary>
        /// Opens the scene with this key. With <paramref name="_activateOnLoad"/> false the task finishes when the scene
        /// is ready to activate, and the scene waits (it is held) until <see cref="Activate"/>.
        /// </summary>
        Task<ISceneHandle> LoadScene(string _key, LoadSceneMode _mode, bool _activateOnLoad);

        /// <summary>Lets a held scene activate. Finishes when it is active.</summary>
        Task Activate(ISceneHandle _handle);

        /// <summary>Closes the scene and releases its handle, so the assets only it used can leave memory.</summary>
        Task Unload(ISceneHandle _handle);

        /// <summary>Makes the scene the active scene.</summary>
        void SetActive(ISceneHandle _handle);
    }
}
