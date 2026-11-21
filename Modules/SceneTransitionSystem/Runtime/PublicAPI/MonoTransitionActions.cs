using AbstractPixel.Core;
using UnityEngine;
using AbstractPixel.SceneManagement;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>The transition calls for UnityEvents, which list only void methods.</summary>
    [DisallowMultipleComponent]
    public class MonoTransitionActions : MonoBehaviour
    {
        public void Initialize(TransitionProfile _transitionProfile)
        {
            TransitionActions.Initialize(_transitionProfile);
        }

        public void SetTransitionProfile(TransitionProfile _transitionProfile)
        {
            TransitionActions.SetTransitionProfile(_transitionProfile);
        }

        public void HideScreen()
        {
            TransitionActions.HideScreen().ForgetTask();
        }

        public void ShowScreen()
        {
            TransitionActions.ShowScreen().ForgetTask();
        }

        /// <summary>Kept by name: pause, win and game over buttons call it through UnityEvents. Forwards to SceneActions.ChangeTo.</summary>
        public void TransitionToSceneWithEffects(SceneGroup group)
        {
            TransitionActions.TransitionToSceneWithEffects(group).ForgetTask();
        }

        /// <summary>Kept by name: pause, win and game over buttons call it through UnityEvents. Forwards to SceneActions.Retry.</summary>
        public void ReloadActiveScenesWithEffects()
        {
            TransitionActions.ReloadActiveScenesWithEffects().ForgetTask();
        }
    }
}
