using AbstractPixel.Core;
using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    public static class SceneEventBus
    {
        public static event Action<SceneGroup> OnNewSceneGroupTransitionTo = delegate { };
        public static event Action<SceneReference> OnMainSceneLoaded = delegate { };
        public static event Action<SceneGroup> OnSceneGroupPreloaded= delegate { };
        public static event Action<SceneGroup> OnSceneGroupUnloaded = delegate { };
        public static event Action<SceneGroup> OnPreloadedSceneGroupActivated= delegate { };

        /// <summary>Raised once per session, right after start-up's <see cref="OnNewSceneGroupTransitionTo"/>, when every scene of the first scene group has loaded.</summary>
        public static event Action<SceneGroup> OnGameStarted = delegate { };

        /// <summary>Raised once, when the launch scene hands over to the start scene group (SceneActions.ReleaseLaunchScene).</summary>
        public static event Action OnLaunchSceneReleased = delegate { };


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            OnNewSceneGroupTransitionTo = delegate { };
            OnSceneGroupPreloaded = delegate { };
            OnSceneGroupUnloaded = delegate { };
            OnPreloadedSceneGroupActivated = delegate { };
            OnMainSceneLoaded = delegate { };
            OnGameStarted = delegate { };
            OnLaunchSceneReleased = delegate { };
        }

        public static void RaiseOnSceneGroupUnloaded(SceneGroup newSceneGroup)
        {
            OnSceneGroupUnloaded.Invoke(newSceneGroup);
        }

        public static void RaiseOnPreloadedSceneGroupActivated(SceneGroup newSceneGroup)
        {
            OnPreloadedSceneGroupActivated.Invoke(newSceneGroup);
        }

        public static void RaiseOnSceneGroupPreloaded(SceneGroup newSceneGroup)
        {
            OnSceneGroupPreloaded.Invoke(newSceneGroup);
        }

        public static void RaiseOnNewSceneTransitionedTo(SceneGroup newSceneGroup)
        {
            OnNewSceneGroupTransitionTo.Invoke(newSceneGroup);
        }

        public static void RaiseOnGameStarted(SceneGroup _startedGroup)
        {
            OnGameStarted.Invoke(_startedGroup);
        }

        public static void RaiseOnLaunchSceneReleased()
        {
            OnLaunchSceneReleased.Invoke();
        }

        public static void RaiseOnMainSceneLoaded(SceneReference mainScene)
        {
            OnMainSceneLoaded.Invoke(mainScene);
        }

    }
}
