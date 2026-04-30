using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    public static class SceneEventBus
    {
        public static event Action<SceneGroup> OnNewSceneGroupLoaded = delegate { };
        public static event Action<SceneGroup> OnSceneGroupPreloaded= delegate { };
        public static event Action<SceneGroup> OnSceneGroupUnloaded = delegate { };
        public static event Action<SceneGroup> OnPreloadedSceneGroupActivated= delegate { };


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            OnNewSceneGroupLoaded = delegate { };
            OnSceneGroupPreloaded = delegate { };
            OnSceneGroupUnloaded = delegate { };
            OnPreloadedSceneGroupActivated = delegate { };
        }


        public static void RaiseOnNewSceneGroupLoaded(SceneGroup newSceneGroup)
        {
            OnNewSceneGroupLoaded.Invoke(newSceneGroup);
        }

    }
}
