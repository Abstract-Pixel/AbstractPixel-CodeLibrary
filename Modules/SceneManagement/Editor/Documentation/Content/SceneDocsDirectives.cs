using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The directive names the scene documentation knows. Any other name is reported where it is written.</summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsDirectives
    {
        public static readonly HashSet<string> Known = new HashSet<string>
        {
            "level", "action", "done-when", "marks", "timeline", "your-project", "members", "code-map", "adr-list", "diagram", "scene-status-demo",
            "layout", "type-header", "decision", "locate"
        };

        /// <summary>The action kinds an action directive may name.</summary>
        public static readonly HashSet<string> ActionKinds = new HashSet<string>
        {
            "open-sample", "select", "ping", "select-in-scene", "open-settings", "open-preferences", "open-picker", "play", "stop", "highlight"
        };

        /// <summary>The messages a done-when may name: scene messages, or "screen:" and a screen state.</summary>
        public static readonly HashSet<string> Messages = new HashSet<string>
        {
            "OnNewSceneGroupTransitionTo", "OnMainSceneLoaded", "OnSceneGroupPreloaded", "OnSceneGroupUnloaded",
            "OnPreloadedSceneGroupActivated", "OnGameStarted", "OnSceneChangeFailed", "OnLaunchSceneReleased",
            "screen:Visible", "screen:Hiding", "screen:Hidden", "screen:Showing"
        };
    }
}
