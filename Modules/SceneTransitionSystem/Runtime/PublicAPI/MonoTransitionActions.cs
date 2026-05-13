using AbstractPixel.Core;
using UnityEngine;
using AbstractPixel.SceneManagement;

namespace AbstractPixel.SceneTransitions
{
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
      
        public void PlayTransitionIn()
        {
            TransitionActions.PlayTransitionIn().ForgetTask();
        }

        public void PlayTransitionOut()
        {
            TransitionActions.PlayTransitionOut().ForgetTask();
        }

        public void ReloadActiveScenesWithEffects()
        {
            TransitionActions.ReloadActiveScenesWithEffects().ForgetTask();
        }

        public void TransitionToSceneWithEffects(SceneGroup group)
        {
            TransitionActions.TransitionToSceneWithEffects(group).ForgetTask();
        }

        public void TransitionToPreloadedSceneWithEffects()
        {
            TransitionActions.TransitionToPreloadedSceneWithEffects().ForgetTask();
        }
    }
}