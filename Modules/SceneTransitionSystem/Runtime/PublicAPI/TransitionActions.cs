using AbstractPixel.SceneManagement;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    public static class TransitionActions
    {
        public static bool IsTransitioning;
        public static bool IsInitialized => TransitionManager.Instance.IsInitialized;

        public static void Initialize(TransitionProfile _transitionProfile)
        {
            TransitionManager.Instance?.Initialize(_transitionProfile);
        }

        public static async Task PlayTransitionIn()
        {
            await TransitionManager.Instance?.PlayTransitionIn();
        }

        public static Task PlayTransitionOut()
        {
            return TransitionManager.Instance?.PlayTransitionOut() ?? Task.CompletedTask;
        }

        public static void SetTransitionProfile(TransitionProfile _transitionProfile)
        {
            TransitionManager.Instance?.SetTransitionProfile(_transitionProfile);
        }

        #region Actual Scene Transition Sequences

        public static async Task ReloadActiveScenesWithEffects()
        {
            IsTransitioning = true;
            await PlayTransitionIn();
            try
            {
                await SceneActions.ReloadActiveSceneGroup();
            }
            finally
            {
                await PlayTransitionOut();
                IsTransitioning = false;
            }
        }
        public static async Task TransitionToSceneWithEffects(SceneGroup group)
        {
            IsTransitioning = true;
            await PlayTransitionIn();
            try
            {
                await SceneActions.TransitionToSceneGroup(group);
            }
            finally
            {
                await PlayTransitionOut();
                IsTransitioning = false;
            }
        }
        public static async Task TransitionToPreloadedSceneWithEffects()
        {
            IsTransitioning = true;
            await PlayTransitionIn();
            try
            {
                await SceneActions.TransitionToPreloadedSceneGroup();
            }
            finally
            {
                await PlayTransitionOut();
                IsTransitioning = false;
            }
        }
        #endregion

    }
}