using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        public static void PreloadSceneGroup(SceneGroup _sceneGroup)
        {
           _=SceneCoordinator.Instance.PreloadSceneGroup(_sceneGroup);
        }
    }
}
