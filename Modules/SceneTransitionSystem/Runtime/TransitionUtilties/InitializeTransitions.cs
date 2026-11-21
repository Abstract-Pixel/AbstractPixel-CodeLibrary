using UnityEngine;
using UnityEngine.Serialization;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>Sets the transition profile when the scene wakes, and optionally plays Show screen once.</summary>
    [DisallowMultipleComponent]
    public class InitializeTransitions : MonoBehaviour
    {
        [SerializeField] private TransitionProfile transitionProfile;
        [Tooltip("Plays Show screen when this scene wakes, so the game opens with the effect uncovering it.")]
        [FormerlySerializedAs("autoPlayTransitionOut")]
        [SerializeField] private bool showScreenOnAwake = true;

        private void Awake()
        {
            TransitionActions.Initialize(transitionProfile);
            if (showScreenOnAwake)
            {
                TransitionActions.ShowScreen().ForgetTask();
            }
        }
    }
}
