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
        private Task runningEffect = Task.CompletedTask;

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

        /// <summary>Hide screen. An effect that is still running finishes first, so the screen is always covered at the end.</summary>
        internal async Task HideScreen()
        {
            if (!IsInitialized || transitionController == null)
            {
                return;
            }
            await WaitForRunningEffect();
            runningEffect = PlayStep(ScreenState.Hiding, ScreenState.Hidden, true);
            await runningEffect;
        }

        /// <summary>Show screen. An effect that is still running finishes first.</summary>
        internal async Task ShowScreen()
        {
            if (!IsInitialized || transitionController == null)
            {
                return;
            }
            await WaitForRunningEffect();
            runningEffect = PlayStep(ScreenState.Showing, ScreenState.Visible, false);
            await runningEffect;
        }

        // Checked again after every wait: another step may have started while this one waited.
        private async Task WaitForRunningEffect()
        {
            while (!runningEffect.IsCompleted)
            {
                await runningEffect;
            }
        }

        private async Task PlayStep(ScreenState _starting, ScreenState _finished, bool _hides)
        {
            TransitionActions.ChangeScreenState(_starting);
            try
            {
                if (_hides)
                {
                    await transitionController.PlayHideScreen();
                }
                else
                {
                    await transitionController.PlayShowScreen();
                }
            }
            finally
            {
                TransitionActions.ChangeScreenState(_finished);
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
    }
}
