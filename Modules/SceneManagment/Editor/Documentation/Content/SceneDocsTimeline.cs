using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using AbstractPixel.SceneTransitions;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Listens to the scene messages and screen states while Play runs and keeps them as rows for "What just happened"
    /// and for ticking steps. Subscribes when Play starts, unsubscribes when Play stops; subscribing twice is harmless,
    /// so a domain reload or a second window never adds duplicate rows.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneDocsTimeline
    {
        private static readonly List<SceneDocsTimelineRow> rows = new List<SceneDocsTimelineRow>();
        private static bool isSubscribed;

        /// <summary>Raised on the main thread for each new row.</summary>
        public static event Action<SceneDocsTimelineRow> RowAdded;

        /// <summary>Raised when Play starts and the rows are cleared.</summary>
        public static event Action Cleared;

        public static IReadOnlyList<SceneDocsTimelineRow> Rows
        {
            get { return rows; }
        }

        public static bool IsSubscribed
        {
            get { return isSubscribed; }
        }

        static SceneDocsTimeline()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (EditorApplication.isPlaying)
            {
                Subscribe();
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange _change)
        {
            if (_change == PlayModeStateChange.EnteredPlayMode)
            {
                rows.Clear();
                if (Cleared != null)
                {
                    Cleared();
                }
                Subscribe();
                return;
            }
            if (_change == PlayModeStateChange.ExitingPlayMode)
            {
                Unsubscribe();
            }
        }

        public static void Subscribe()
        {
            Unsubscribe();
            SceneEventBus.OnNewSceneGroupTransitionTo += OnNewSceneGroup;
            SceneEventBus.OnMainSceneLoaded += OnMainSceneLoaded;
            SceneEventBus.OnSceneGroupPreloaded += OnPreloaded;
            SceneEventBus.OnSceneGroupUnloaded += OnUnloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated += OnPreloadActivated;
            SceneEventBus.OnGameStarted += OnGameStarted;
            SceneEventBus.OnSceneChangeFailed += OnFailed;
            SceneEventBus.OnLaunchSceneReleased += OnLaunchReleased;
            TransitionEventBus.OnScreenStateChanged += OnScreenState;
            isSubscribed = true;
        }

        public static void Unsubscribe()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= OnNewSceneGroup;
            SceneEventBus.OnMainSceneLoaded -= OnMainSceneLoaded;
            SceneEventBus.OnSceneGroupPreloaded -= OnPreloaded;
            SceneEventBus.OnSceneGroupUnloaded -= OnUnloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated -= OnPreloadActivated;
            SceneEventBus.OnGameStarted -= OnGameStarted;
            SceneEventBus.OnSceneChangeFailed -= OnFailed;
            SceneEventBus.OnLaunchSceneReleased -= OnLaunchReleased;
            TransitionEventBus.OnScreenStateChanged -= OnScreenState;
            isSubscribed = false;
        }

        private static void OnNewSceneGroup(SceneGroup _group) { Add("OnNewSceneGroupTransitionTo", NameOf(_group)); }
        private static void OnMainSceneLoaded(SceneReference _scene) { Add("OnMainSceneLoaded", _scene != null ? _scene.SceneName : ""); }
        private static void OnPreloaded(SceneGroup _group) { Add("OnSceneGroupPreloaded", NameOf(_group)); }
        private static void OnUnloaded(SceneGroup _group) { Add("OnSceneGroupUnloaded", NameOf(_group)); }
        private static void OnPreloadActivated(SceneGroup _group) { Add("OnPreloadedSceneGroupActivated", NameOf(_group)); }
        private static void OnGameStarted(SceneGroup _group) { Add("OnGameStarted", NameOf(_group)); }
        private static void OnFailed(SceneGroup _group, SceneReference _scene) { Add("OnSceneChangeFailed", NameOf(_group)); }
        private static void OnLaunchReleased() { Add("OnLaunchSceneReleased", ""); }
        private static void OnScreenState(ScreenState _state) { Add("screen:" + _state, ""); }

        private static string NameOf(UnityEngine.Object _asset)
        {
            return _asset != null ? _asset.name : "";
        }

        private static void Add(string _message, string _detail)
        {
            SceneDocsTimelineRow row = new SceneDocsTimelineRow(Time.unscaledTime, _message, _detail);
            rows.Add(row);
            if (RowAdded != null)
            {
                RowAdded(row);
            }
        }

        /// <summary>One sentence, in glossary words, saying why a message fires.</summary>
        public static string WhyOf(string _message)
        {
            switch (_message)
            {
                case "OnNewSceneGroupTransitionTo": return "A scene change finished: every scene of the new scene group is open and it is now the active scene group.";
                case "OnMainSceneLoaded": return "The main scene loaded and became Unity's active scene.";
                case "OnSceneGroupPreloaded": return "A preload finished: the scene group's scenes are open in the background, its main scene waiting.";
                case "OnSceneGroupUnloaded": return "The old scene group's scenes closed during a scene change.";
                case "OnPreloadedSceneGroupActivated": return "The scene change switched to the preloaded scene group.";
                case "OnGameStarted": return "Start-up finished: the first scene group is fully open. It fires once per session.";
                case "OnSceneChangeFailed": return "A scene of the group did not load; the scene system goes to the fallback scene group.";
                case "OnLaunchSceneReleased": return "The launch scene handed over, so the start scene group can show itself.";
                case "screen:Hiding": return "Hide screen started: the screen effect covers the screen before scenes close.";
                case "screen:Hidden": return "The screen is covered, so scenes can close and open out of sight.";
                case "screen:Showing": return "Show screen started: the new scenes are ready and the screen uncovers.";
                case "screen:Visible": return "The screen is fully visible again.";
                default: return "";
            }
        }

        /// <summary>The guide section that explains a message.</summary>
        public static string TopicOf(string _message)
        {
            if (_message.StartsWith("screen:"))
            {
                return "guide/screen-and-transitions#screen-states";
            }
            return "guide/scene-messages#" + _message.ToLowerInvariant();
        }
    }
}
