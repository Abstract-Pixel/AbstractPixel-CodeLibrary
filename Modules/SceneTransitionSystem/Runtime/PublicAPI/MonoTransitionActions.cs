using AbstractPixel.Core;
using UnityEngine;
using AbstractPixel.SceneManagement;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class MonoTransitionActions : MonoBehaviour
    {
        public void Initialize(TransitionProfile _transitionInProfile, TransitionProfile _transitionOutProfile)
        {
            TransitionActions.Initialize(_transitionInProfile, _transitionOutProfile);
        }
        public void SetTransitionInProfile(TransitionProfile _transitionInProfile)
        {
            TransitionActions.SetTransitionInProfile(_transitionInProfile);
        }
        public void SetTransitionOutProfile(TransitionProfile _transitionOutProfile)
        {
            TransitionActions.SetTransitionOutProfile(_transitionOutProfile);
        }

        public void PlayTransitionIn()
        {
            TransitionActions.PlayTransitionIn().ForgetTask();
        }

        public void PlayTransitionOut()
        {
            TransitionActions.PlayTransitionOut().ForgetTask();
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