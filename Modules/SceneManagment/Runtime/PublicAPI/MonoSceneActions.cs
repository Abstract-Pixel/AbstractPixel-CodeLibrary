using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [DisallowMultipleComponent]
    public class MonoSceneActions : MonoBehaviour
    {
        public void TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            SceneActions.TransitionToSceneGroup(_sceneGroup).ForgetTask();
        }

        public void PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            SceneActions.PreloadSceneGroup(_sceneGroup).ForgetTask();
        }

        public void TransitionToPreloadedSceneGroup()
        {
            SceneActions.TransitionToPreloadedSceneGroup().ForgetTask();
        }

        public void ReloadActiveSceneGroup()
        {
            SceneActions.ReloadActiveSceneGroup().ForgetTask();
        }
    }
}
