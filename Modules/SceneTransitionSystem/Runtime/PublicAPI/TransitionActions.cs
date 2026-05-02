using AbstractPixel.SceneManagement;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneTransitions
{
    public static class TransitionActions
    {
        public static bool IsTransitioning => TransitionManager.Instance.IsTransitioning;
        public static bool IsInitialized => TransitionManager.Instance.IsInitialized;

        public static void Initialize(TransitionProfile _transitionInProfile, TransitionProfile _transitionOutProfile)
        {
            TransitionManager.Instance?.Initialize(_transitionInProfile, _transitionOutProfile);
        }

        public static async Task PlayTransitionIn()
        {
            await TransitionManager.Instance?.PlayTransitionIn();
        }

        public static Task PlayTransitionOut()
        {
            return TransitionManager.Instance?.PlayTransitionOut() ?? Task.CompletedTask;
        }

        public static void SetTransitionInProfile(TransitionProfile _transitionInProfile)
        {
            TransitionManager.Instance?.SetTransitionInProfile(_transitionInProfile);
        }

        public static void SetTransitionOutProfile(TransitionProfile _transitionOutProfile)
        {
            TransitionManager.Instance?.SetTransitionOutProfile(_transitionOutProfile);
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