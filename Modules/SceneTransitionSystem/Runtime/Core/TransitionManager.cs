using UnityEngine;
using AbstractPixel.Core;
using System.Threading.Tasks;

namespace AbstractPixel.SceneTransitions
{
    [DisallowMultipleComponent]
    public class TransitionManager : PersistentSingleton<TransitionManager>
    {
        [SerializeField,ReadOnly] private TransitionProfile transitionInProfile;
        [SerializeField,ReadOnly] private TransitionProfile transitionOutProfile;

        private ITransitionController transitionInController;
        private ITransitionController transitionOutController;

        internal bool IsTransitioning { get; private set; } = false;
        internal bool IsInitialized { get; private set; } = false;

        internal void Initialize(TransitionProfile _transitionInProfile, TransitionProfile _transitionOutProfile)
        {
            transitionInProfile = _transitionInProfile;
            transitionOutProfile = _transitionOutProfile;

            SetTransitionInProfile(transitionInProfile);
            SetTransitionOutProfile(transitionOutProfile);

            if (transitionInController != null && transitionOutController != null)
            {
                IsInitialized = true;
            }
        }

        internal async Task PlayTransitionIn()
        {
            if (IsTransitioning || !IsInitialized) return;
            if (transitionInProfile == null || transitionInController == null)
            {
                Debug.LogError("Transition In Profile or Controller is not set. Cannot play transition in.");
                return;
            }

            IsTransitioning = true;
            Time.timeScale = 0;
            TransitionEventBus.RaiseTransitionInStarted();
            await transitionInController.PlayTransitionIn();
            Time.timeScale = 1;
            TransitionEventBus.RaiseTransitionInCompleted();
            IsTransitioning = false;
        }

        internal async Task PlayTransitionOut()
        {
            if (IsTransitioning || !IsInitialized) return;
            if (transitionOutProfile == null || transitionOutController == null)
            {
                Debug.LogError("Transition Out Profile or Controller is not set. Cannot play transition out.");
                return;
            }
            IsTransitioning = true;
            Time.timeScale = 0;
            TransitionEventBus.RaiseTransitionOutStarted();
            await transitionOutController.PlayTransitionOut();
            TransitionEventBus.RaiseTransitionOutCompleted();
            Time.timeScale = 1;
            IsTransitioning = false;
        }


        internal void SetTransitionInProfile(TransitionProfile _transitionInProfile)
        {
            if (transitionInController != null)
            {
                Destroy(transitionInController.gameObject);
            }
            if (_transitionInProfile == null)
            {
                IsInitialized = false;
                return;
            }
            transitionInProfile = _transitionInProfile;
            transitionInController = Instantiate(_transitionInProfile.TransitionControllerPrefab, transform)
                                                                     .GetComponentInChildren<ITransitionController>(true);
            transitionInController.Initialize(transitionInProfile);
        }

        internal void SetTransitionOutProfile(TransitionProfile _transitionOutProfile)
        {
            if (transitionOutController != null)
            {
                Destroy(transitionOutController.gameObject);
            }
            if (_transitionOutProfile == null)
            {
                IsInitialized = false;
                return;
            }
            transitionOutProfile = _transitionOutProfile;
            transitionOutController = Instantiate(_transitionOutProfile.TransitionControllerPrefab, transform)
                                                                     .GetComponentInChildren<ITransitionController>(true);
            transitionOutController.Initialize(transitionOutProfile);
        }
    }
}