using UnityEngine;
using AbstractPixel.Core;
using System.Threading.Tasks;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class TransitionManager : PersistentSingleton<TransitionManager>
    {
        [SerializeField,ReadOnly] private TransitionProfile transitionProfile;
        private ITransitionController transitionController;

        internal bool IsTransitioning { get; private set; } = false;
        internal bool IsInitialized { get; private set; } = false;

        internal void Initialize(TransitionProfile _transitionProfile)
        {
            transitionProfile = _transitionProfile;
            SetTransitionProfile(transitionProfile);
            if (transitionController != null)
            {
                IsInitialized = true;
            }
        }

        internal async Task PlayTransitionIn()
        {
            if (IsTransitioning || !IsInitialized) return;
            if (transitionProfile == null || transitionController == null)
            {
                Debug.LogError("Transition In Profile or Controller is not set. Cannot play transition in.");
                return;
            }

            IsTransitioning = true;
            //Time.timeScale = 0;
            TransitionEventBus.RaiseTransitionInStarted();
            await transitionController.PlayTransitionIn();
            Time.timeScale = 1;
            TransitionEventBus.RaiseTransitionInCompleted();
            IsTransitioning = false;
        }

        internal async Task PlayTransitionOut()
        {
            if (IsTransitioning || !IsInitialized) return;
            if (transitionProfile == null || transitionController == null)
            {
                Debug.LogError("Transition In Profile or Controller is not set. Cannot play transition in.");
                return;
            }

            IsTransitioning = true;
            TransitionEventBus.RaiseTransitionOutStarted();
            await transitionController.PlayTransitionOut();
            TransitionEventBus.RaiseTransitionOutCompleted();
            IsTransitioning = false;
        }


        internal void SetTransitionProfile(TransitionProfile _transitionInProfile)
        {
            if (transitionController != null)
            {
                Destroy(transitionController.gameObject);
            }
            if (_transitionInProfile == null)
            {
                IsInitialized = false;
                return;
            }
            transitionProfile = _transitionInProfile;
            transitionController = Instantiate(_transitionInProfile.TransitionControllerPrefab, transform)
                                                                     .GetComponentInChildren<ITransitionController>(true);
            transitionController.Initialize(transitionProfile);
        }
    }
}