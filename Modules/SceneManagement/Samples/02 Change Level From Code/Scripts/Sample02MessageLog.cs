using AbstractPixel.Core;
using AbstractPixel.SceneTransitions;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 02: listens to every scene message and the screen state, and shows the last 12 with their frame numbers.
    /// It lives in each main scene, so it hears nothing while no main scene is open (OnSceneGroupUnloaded comes then).
    /// </summary>
    public class Sample02MessageLog : MonoBehaviour
    {
        private const int LogLine = 2;
        private const int MaxEntries = 12;

        [SerializeField] private SampleLabel label;

        // Static, so the log carries on in the next main scene.
        private static readonly string[] entries = new string[MaxEntries];
        private static int entryCount;
        private static int logVersion;
        private int shownVersion = -1;

        // Domain reload may be off: start every Play with an empty log.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetLog()
        {
            entryCount = 0;
            logVersion = 0;
        }

        private void OnEnable()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo += OnNewSceneGroupTransitionTo;
            SceneEventBus.OnMainSceneLoaded += OnMainSceneLoaded;
            SceneEventBus.OnSceneGroupUnloaded += OnSceneGroupUnloaded;
            SceneEventBus.OnSceneGroupPreloaded += OnSceneGroupPreloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated += OnPreloadedSceneGroupActivated;
            SceneEventBus.OnGameStarted += OnGameStarted;
            SceneEventBus.OnSceneChangeFailed += OnSceneChangeFailed;
            SceneEventBus.OnLaunchSceneReleased += OnLaunchSceneReleased;
            TransitionEventBus.OnScreenStateChanged += OnScreenStateChanged;
        }

        private void OnDisable()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= OnNewSceneGroupTransitionTo;
            SceneEventBus.OnMainSceneLoaded -= OnMainSceneLoaded;
            SceneEventBus.OnSceneGroupUnloaded -= OnSceneGroupUnloaded;
            SceneEventBus.OnSceneGroupPreloaded -= OnSceneGroupPreloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated -= OnPreloadedSceneGroupActivated;
            SceneEventBus.OnGameStarted -= OnGameStarted;
            SceneEventBus.OnSceneChangeFailed -= OnSceneChangeFailed;
            SceneEventBus.OnLaunchSceneReleased -= OnLaunchSceneReleased;
            TransitionEventBus.OnScreenStateChanged -= OnScreenStateChanged;
        }

        private void Update()
        {
            if (shownVersion == logVersion || label == null)
            {
                return;
            }
            label.SetLine(LogLine, string.Join("\n", entries, 0, entryCount));
            shownVersion = logVersion;
        }

        private void OnNewSceneGroupTransitionTo(SceneGroup _group)
        {
            Add("OnNewSceneGroupTransitionTo (" + _group.name + ")");
        }

        private void OnMainSceneLoaded(SceneReference _scene)
        {
            Add("OnMainSceneLoaded (" + _scene.SceneName + ")");
        }

        private void OnSceneGroupUnloaded(SceneGroup _group)
        {
            Add("OnSceneGroupUnloaded (" + _group.name + ")");
        }

        private void OnSceneGroupPreloaded(SceneGroup _group)
        {
            Add("OnSceneGroupPreloaded (" + _group.name + ")");
        }

        private void OnPreloadedSceneGroupActivated(SceneGroup _group)
        {
            Add("OnPreloadedSceneGroupActivated (" + _group.name + ")");
        }

        private void OnGameStarted(SceneGroup _group)
        {
            Add("OnGameStarted (" + _group.name + ")");
        }

        private void OnSceneChangeFailed(SceneGroup _group, SceneReference _failedScene)
        {
            Add("OnSceneChangeFailed (" + _failedScene.SceneName + ")");
        }

        private void OnLaunchSceneReleased()
        {
            Add("OnLaunchSceneReleased");
        }

        private void OnScreenStateChanged(ScreenState _state)
        {
            Add("Screen " + _state);
        }

        // Keeps the newest 12, oldest first.
        private static void Add(string _message)
        {
            string entry = "frame " + Time.frameCount + ": " + _message;
            if (entryCount < MaxEntries)
            {
                entries[entryCount] = entry;
                entryCount++;
            }
            else
            {
                System.Array.Copy(entries, 1, entries, 0, MaxEntries - 1);
                entries[MaxEntries - 1] = entry;
            }
            logVersion++;
        }
    }
}
