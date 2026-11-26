using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.GameManagement
{
    /// <summary>The one MonoBehaviour that runs the machine: Tick each frame, LeaveOnSceneChange on scene changes, and the time ease.</summary>
    /// <remarks>
    /// - Made on its own (hidden, kept across scenes) the first time <see cref="GameStates"/> builds its machine.
    /// - Nothing is made while no code uses <see cref="GameStates"/>, so a game on the old components is untouched.
    /// </remarks>
    [AddComponentMenu("")]
    public class GameStateDriver : MonoBehaviour
    {
        private static GameStateDriver instance;
        private GameStateMachine machine;
        private UnityEffectSink sink;

        /// <summary>Builds a machine with the Unity sink, gives it to the facade and starts driving it.</summary>
        public static GameStateMachine CreateMachine(GameManagementSettings _settings)
        {
            UnityEffectSink unitySink = new UnityEffectSink(new UnityTimeScaleTarget(), InputSourceFactory.FromSettings(_settings));
            GameStateMachine newMachine = new GameStateMachine(_settings, new RealtimeGameClock(), unitySink);
            if (Application.isPlaying)
            {
                Ensure().Drive(newMachine, unitySink);
            }
            return newMachine;
        }

        /// <summary>Uses a custom input source instead of the one in the settings. Null leaves input alone.</summary>
        public static void UseInputSource(IInputSource _source)
        {
            if (instance != null && instance.sink != null)
            {
                instance.sink.InputSource = _source;
            }
        }

        public void Drive(GameStateMachine _machine, UnityEffectSink _sink)
        {
            machine = _machine;
            sink = _sink;
        }

        private static GameStateDriver Ensure()
        {
            if (instance == null)
            {
                GameObject host = new GameObject("[Game State Driver]");
                host.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(host);
                instance = host.AddComponent<GameStateDriver>();
            }
            return instance;
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void Update()
        {
            Step(Time.unscaledDeltaTime);
        }

        /// <summary>One frame of driving: ticks the machine, then moves the time ease on. Does nothing before <see cref="Drive"/>.</summary>
        public void Step(float _unscaledSeconds)
        {
            if (machine == null)
            {
                return;
            }
            machine.Tick();
            sink.Step(_unscaledSeconds);
        }

        private void HandleActiveSceneChanged(Scene _previous, Scene _next)
        {
            machine?.LeaveOnSceneChange();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewPlaySession()
        {
            instance = null;
        }
    }
}
