using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The loading scene of a scene change (ADR 0003, amended in 4C). It decides from the group's "Show the loading
    /// scene" whether this change shows one, opens it under the cover before the old scenes close, and at the end waits
    /// for the minimum time and the loading progress view's "full" animation, hides the screen and closes it. With no
    /// screen effect registered the same steps make an instant cut, and no frame is left without a camera.
    /// </summary>
    internal sealed class LoadingSceneRoute
    {
        private static ILoadingProgressView view;

        /// <summary>Reads the scene system settings; tests replace it.</summary>
        internal Func<SceneSystemSettings> FindSettings = SceneSystemSettingsSource.Find;

        private SceneReference openLoadingScene;
        private float shownAt;

        /// <summary>The loading scene this change shows, or null when it shows none.</summary>
        public SceneReference ChooseFor(SceneGroup _group, bool _isRetry, bool _usesPreload)
        {
            if (_usesPreload || _group == null)
            {
                return null;
            }
            ShowLoadingScene choice = _group.ShowLoadingScene;
            bool showsIt = choice == ShowLoadingScene.EveryTimeRetryIncluded
                           || (choice == ShowLoadingScene.WhenArrivingFromAnotherSceneGroup && !_isRetry);
            if (!showsIt)
            {
                return null;
            }

            SceneReference scene = _group.LoadingScene;
            if (IsEmpty(scene))
            {
                SceneSystemSettings settings = FindSettings();
                scene = settings == null ? null : settings.DefaultLoadingScene;
            }
            if (IsEmpty(scene))
            {
                Debug.LogWarning($"[Scene system] '{_group.name}' shows a loading scene, but it picked none and there is no default loading scene. The scene change goes on without one.");
                return null;
            }
            return scene;
        }

        /// <summary>Opens the loading scene under the cover, then Show screen uncovers it.</summary>
        public async Task Show(SceneReference _scene, ISceneLoader _loader)
        {
            try
            {
                await _loader.LoadScene(_scene, true, true, false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Scene system] The loading scene '{_scene.SceneName}' failed to load: {ex.Message} The scene change goes on without it.");
                return;
            }

            openLoadingScene = _scene;
            shownAt = Now;
            // The view starts its appear animation only as Show screen starts (or at the cut).
            if (view != null)
            {
                view.OnShowStarted();
            }
            // The Show screen that uncovers a loading scene never releases relocatables.
            await SceneTransitionExecutorSlot.ShowScreen(null);
        }

        /// <summary>
        /// Waits for the minimum time and the view's "full" animation, then Hide screen covers the loading scene and it
        /// closes. Does nothing when no loading scene is open. The coordinator's Show screen uncovers the new level.
        /// </summary>
        public async Task Leave(ISceneLoader _loader)
        {
            if (openLoadingScene == null)
            {
                return;
            }

            await WaitForMinimumTime();
            await PlayFullAnimation();
            await SceneTransitionExecutorSlot.HideScreen();
            SceneReference scene = openLoadingScene;
            openLoadingScene = null;
            try
            {
                await _loader.UnloadScene(scene);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        internal static void Register(ILoadingProgressView _view)
        {
            view = _view;
        }

        internal static void Unregister(ILoadingProgressView _view)
        {
            if (ReferenceEquals(view, _view))
            {
                view = null;
            }
        }

        private async Task WaitForMinimumTime()
        {
            SceneSystemSettings settings = FindSettings();
            float minimumSeconds = settings == null ? 0f : settings.LoadingSceneMinimumSeconds;
            float endTime = shownAt + minimumSeconds;
            while (Now < endTime)
            {
                await Task.Yield();
            }
        }

        private static async Task PlayFullAnimation()
        {
            if (view == null)
            {
                return;
            }
            try
            {
                await view.PlayFullAnimation();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static float Now
        {
            get { return (float)Time.realtimeSinceStartupAsDouble; }
        }

        private static bool IsEmpty(SceneReference _scene)
        {
            return _scene == null || string.IsNullOrEmpty(_scene.SceneName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            view = null;
        }
    }
}
