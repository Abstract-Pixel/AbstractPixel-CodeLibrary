using AbstractPixel.SceneManagement;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    public static class TransitionActions
    {
        public static bool IsTransitioning => TransitionManager.Instance.IsTransitioning;
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
        public static async Task TransitionToSceneWithEffects(SceneGroup group)
        {
            await PlayTransitionIn();
            await SceneActions.TransitionToSceneGroup(group);
            await PlayTransitionOut();
        }
        public static async Task TransitionToPreloadedSceneWithEffects()
        {
            await PlayTransitionIn();
            await SceneActions.TransitionToPreloadedSceneGroup();
            await PlayTransitionOut();
        }
        #endregion

    }
}