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


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            OnNewSceneGroupTransitionTo = delegate { };
            OnSceneGroupPreloaded = delegate { };
            OnSceneGroupUnloaded = delegate { };
            OnPreloadedSceneGroupActivated = delegate { };
            OnMainSceneLoaded = delegate { };
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

        public static void RaiseOnMainSceneLoaded(SceneReference mainScene)
        {
            OnMainSceneLoaded.Invoke(mainScene);
        }

    }
}
