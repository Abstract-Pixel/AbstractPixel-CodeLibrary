using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class InitializeTransitions : MonoBehaviour
    {
        [SerializeField] private TransitionProfile transitionInProfile;
        [SerializeField] private TransitionProfile transitionOutProfile;
        [SerializeField] private bool autoPlayTransitionIn = true;
        private void Awake()
        {
            TransitionActions.Initialize(transitionInProfile, transitionOutProfile);
            if (autoPlayTransitionIn)
            {
                TransitionActions.PlayTransitionIn().ForgetTask();
            }
        }

    }
}