using AbstractPixel.Core;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>Says whether a scene is Addressable, so the <see cref="SceneLoaderSelector"/> can pick its loader.</summary>
    internal interface IAddressableSceneCheck
    {
        Task<bool> IsAddressable(SceneReference _scene);
    }
}
