using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class InitializeTransitions : MonoBehaviour
    {
        [SerializeField] private TransitionProfile transitionProfile;
        [SerializeField] private bool autoPlayTransitionOut = true;
        private void Awake()
        {
            TransitionActions.Initialize(transitionProfile);
            if (autoPlayTransitionOut)
            {
                TransitionActions.PlayTransitionOut().ForgetTask();
            }
        }

    }
}