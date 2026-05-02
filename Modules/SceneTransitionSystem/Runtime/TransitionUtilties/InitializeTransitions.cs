using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class InitializeTransitions : MonoBehaviour
    {
        [SerializeField] private TransitionProfile transitionInProfile;
        [SerializeField] private TransitionProfile transitionOutProfile;
        [SerializeField] private bool autoPlayTransitionOut = true;
        private void Awake()
        {
            TransitionActions.Initialize(transitionInProfile, transitionOutProfile);
            if (autoPlayTransitionOut)
            {
                TransitionActions.PlayTransitionOut().ForgetTask();
            }
        }

    }
}