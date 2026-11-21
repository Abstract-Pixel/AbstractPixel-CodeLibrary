using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The scene calls for UnityEvents, which list only void methods. The results are not returned.</summary>
    [DisallowMultipleComponent]
    public class MonoSceneActions : MonoBehaviour
    {
        public void ChangeTo(SceneGroup _sceneGroup)
        {
            SceneActions.ChangeTo(_sceneGroup).ForgetTask();
        }

        public void Retry()
        {
            SceneActions.Retry().ForgetTask();
        }

        public void Preload(SceneGroup _sceneGroup)
        {
            SceneActions.Preload(_sceneGroup).ForgetTask();
        }
    }
}
