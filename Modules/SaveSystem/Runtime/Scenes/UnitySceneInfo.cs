using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Reads the active scene from Unity's SceneManager.</summary>
    public sealed class UnitySceneInfo : ISceneInfo
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;
    }
}
