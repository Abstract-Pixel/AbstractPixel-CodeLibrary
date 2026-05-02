using System.Threading.Tasks;

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

        public static Task PlayTransitionIn()
        {
            return TransitionManager.Instance?.PlayTransitionIn();
        }

        public static Task PlayTransitionOut()
        {
            return TransitionManager.Instance?.PlayTransitionOut();
        }

        public static void SetTransitionInProfile(TransitionProfile _transitionInProfile)
        {
            TransitionManager.Instance?.SetTransitionInProfile(_transitionInProfile);
        }

        public static void SetTransitionOutProfile(TransitionProfile _transitionOutProfile)
        {
            TransitionManager.Instance?.SetTransitionOutProfile(_transitionOutProfile);
        }
    }
}