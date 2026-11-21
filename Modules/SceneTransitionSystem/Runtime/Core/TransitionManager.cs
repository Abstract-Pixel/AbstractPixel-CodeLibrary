using UnityEngine;
using AbstractPixel.Core;
using AbstractPixel.SceneManagement;
using System.Threading.Tasks;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// Plays the screen effect of the current transition profile and reports the screen state. It is the scene
    /// transition executor: it registers itself with the scene system once a profile is set, so every scene change
    /// hides and shows the screen. It never reads or writes game time (ADR 0010).
    /// </summary>
    [DisallowMultipleComponent]
    public class TransitionManager : PersistentSingleton<TransitionManager>, ISceneTransitionExecutor
    {
        [SerializeField,ReadOnly] private TransitionProfile transitionProfile;
        private ITransitionController transitionController;

        internal bool IsInitialized { get; private set; } = false;

        internal void Initialize(TransitionProfile _transitionProfile)
        {
            SetTransitionProfile(_transitionProfile);
        }

        private void OnEnable()
        {
            if (IsInitialized)
            {
                SceneActions.RegisterTransitionExecutor(this);
            }
        }

        private void OnDisable()
        {
            SceneActions.UnregisterTransitionExecutor(this);
        }

        /// <summary>Hide screen. Does nothing while an effect is already running or no profile is set.</summary>
        internal async Task HideScreen()
        {
            if (!CanStartEffect())
            {
                return;
            }

            TransitionActions.ChangeScreenState(ScreenState.Hiding);
            try
            {
                await transitionController.PlayHideScreen();
            }
            finally
            {
                TransitionActions.ChangeScreenState(ScreenState.Hidden);
            }
        }

        /// <summary>Show screen. Does nothing while an effect is already running or no profile is set.</summary>
        internal async Task ShowScreen()
        {
            if (!CanStartEffect())
            {
                return;
            }

            TransitionActions.ChangeScreenState(ScreenState.Showing);
            try
            {
                await transitionController.PlayShowScreen();
            }
            finally
            {
                TransitionActions.ChangeScreenState(ScreenState.Visible);
            }
        }

        Task ISceneTransitionExecutor.TransitionToHideScreen()
        {
            return HideScreen();
        }

        Task ISceneTransitionExecutor.TransitionToShowScreen()
        {
            return ShowScreen();
        }

        internal void SetTransitionProfile(TransitionProfile _transitionProfile)
        {
            if (transitionController != null)
            {
                DestroyController(transitionController.gameObject);
                transitionController = null;
            }
            IsInitialized = false;
            if (_transitionProfile == null)
            {
                SceneActions.UnregisterTransitionExecutor(this);
                return;
            }

            transitionProfile = _transitionProfile;
            transitionController = Instantiate(_transitionProfile.TransitionControllerPrefab, transform)
                                                                     .GetComponentInChildren<ITransitionController>(true);
            if (transitionController == null)
            {
                Debug.LogError("[TransitionManager] The profile's controller prefab has no ITransitionController.");
                SceneActions.UnregisterTransitionExecutor(this);
                return;
            }

            transitionController.Initialize(transitionProfile);
            IsInitialized = true;
            SceneActions.RegisterTransitionExecutor(this);
        }

        private static void DestroyController(GameObject _controllerObject)
        {
            if (Application.isPlaying)
            {
                Destroy(_controllerObject);
                return;
            }
            DestroyImmediate(_controllerObject);
        }

        private bool CanStartEffect()
        {
            if (!IsInitialized || transitionController == null)
            {
                return false;
            }
            ScreenState state = TransitionActions.ScreenState;
            bool effectIsRunning = state == ScreenState.Hiding || state == ScreenState.Showing;
            return !effectIsRunning;
        }
    }
}
