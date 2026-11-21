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

        /// <summary>
        /// Raised once when a scene change fails: a scene of the group did not load. Passes the group asked for and the
        /// scene that failed. No "new level" is raised for it; the scene system then goes to the fallback scene group.
        /// </summary>
        public static event Action<SceneGroup, SceneReference> OnSceneChangeFailed = delegate { };

        /// <summary>Raised once, when the launch scene hands over to the start scene group (SceneActions.ReleaseLaunchScene).</summary>
        public static event Action OnLaunchSceneReleased = delegate { };


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            OnNewSceneGroupTransitionTo = delegate { };
            OnSceneGroupPreloaded = delegate { };
            OnSceneGroupUnloaded = delegate { };
            OnPreloadedSceneGroupActivated = delegate { };
            OnMainSceneLoaded = delegate { };
            OnGameStarted = delegate { };
            OnLaunchSceneReleased = delegate { };
            OnSceneChangeFailed = delegate { };
        }

        internal static void RaiseOnSceneGroupUnloaded(SceneGroup newSceneGroup)
        {
            OnSceneGroupUnloaded.Invoke(newSceneGroup);
        }

        internal static void RaiseOnPreloadedSceneGroupActivated(SceneGroup newSceneGroup)
        {
            OnPreloadedSceneGroupActivated.Invoke(newSceneGroup);
        }

        internal static void RaiseOnSceneGroupPreloaded(SceneGroup newSceneGroup)
        {
            OnSceneGroupPreloaded.Invoke(newSceneGroup);
        }

        internal static void RaiseOnNewSceneTransitionedTo(SceneGroup newSceneGroup)
        {
            OnNewSceneGroupTransitionTo.Invoke(newSceneGroup);
        }

        internal static void RaiseOnGameStarted(SceneGroup _startedGroup)
        {
            OnGameStarted.Invoke(_startedGroup);
        }

        internal static void RaiseOnLaunchSceneReleased()
        {
            OnLaunchSceneReleased.Invoke();
        }

        internal static void RaiseOnSceneChangeFailed(SceneGroup _group, SceneReference _failedScene)
        {
            OnSceneChangeFailed.Invoke(_group, _failedScene);
        }

        internal static void RaiseOnMainSceneLoaded(SceneReference mainScene)
        {
            OnMainSceneLoaded.Invoke(mainScene);
        }

    }
}
