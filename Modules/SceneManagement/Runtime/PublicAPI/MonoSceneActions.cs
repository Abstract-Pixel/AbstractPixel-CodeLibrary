using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The scene calls for UnityEvents, such as a button's On Click.</summary>
    /// <remarks>
    /// - UnityEvents list only void methods, so the results are not returned.
    /// - Each call starts the matching SceneActions call and does not wait for it.
    /// </remarks>
    [DisallowMultipleComponent]
    public class MonoSceneActions : MonoBehaviour
    {
        /// <summary>Goes to the scene group given in the event's argument field (SceneActions.ChangeTo).</summary>
        public void ChangeTo(SceneGroup _sceneGroup)
        {
            SceneActions.ChangeTo(_sceneGroup).ForgetTask();
        }

        /// <summary>Starts the open scene group again (SceneActions.Retry).</summary>
        public void Retry()
        {
            SceneActions.Retry().ForgetTask();
        }

        /// <summary>Opens the scene group in the background, so the next change to it is fast (SceneActions.Preload).</summary>
        public void Preload(SceneGroup _sceneGroup)
        {
            SceneActions.Preload(_sceneGroup).ForgetTask();
        }
    }
}
