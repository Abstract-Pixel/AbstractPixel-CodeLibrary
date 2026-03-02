using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

# if UNITY_EDITOR
using UnityEditor;
# endif

namespace AbstractPixel.Core
{
    public class TimerBootstrapper
    {
        static PlayerLoopSystem timerSystem;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogError("Failed to insert TimerManager into Player Loop.");
            }
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);

# if UNITY_EDITOR

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveTimerManager<Update>(ref loop);
                    PlayerLoop.SetPlayerLoop(loop);
                    TimerManager.ClearTimers();
                }
            }
#endif

        }

        static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, timerSystem);
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null,
            };

            return PlayerLoopUtils.TryInsertSystem<T>(ref loop, timerSystem, index);
        }
    }
}
