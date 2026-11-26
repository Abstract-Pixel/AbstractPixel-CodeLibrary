using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>What happens to time while no state is active (between scenes).</summary>
    public enum NoStateTimeScale
    {
        /// <summary>Keep the last time scale. Matches Rally today.</summary>
        KeepLast,
        /// <summary>Set time to 1.</summary>
        SetToOne
    }

    /// <summary>The one game management settings asset per project.</summary>
    /// <remarks>
    /// - It can live in any folder; the Editor finds it by type and keeps it in Player Settings' Preloaded Assets,
    ///   the same method as the scene system.
    /// - Edited in Project Settings &gt; Abstract Pixel &gt; Game Management (Session 6).
    /// </remarks>
    public class GameManagementSettings : ScriptableObject
    {
        public const string DefaultFolder = "Assets/GameManagement";

        [Tooltip("The ordered list of every state.")]
        public GameStateCatalog Catalog;

        [Tooltip("Time scale while no state is active. Keep last is the default: a scene change then cannot let loaded shared scenes run early.")]
        public NoStateTimeScale NoStateTimeScale = NoStateTimeScale.KeepLast;

        [Tooltip("How many transitions the journal keeps for the debugger.")]
        [Min(8)] public int JournalSize = 128;
    }

    /// <summary>Finds the game management settings at runtime.</summary>
    /// <remarks>
    /// - A build loads them with Preloaded Assets before the first scene.
    /// - The Editor does not load Preloaded Assets for Play, so editor code sets a lookup that finds the asset by type.
    /// </remarks>
    public static class GameManagementSettingsSource
    {
        /// <summary>Set by the module's editor code. Null in a build.</summary>
        public static Func<GameManagementSettings> EditorLookup;

        /// <summary>The project's settings, or null when there are none.</summary>
        public static GameManagementSettings Find()
        {
            if (EditorLookup != null)
            {
                GameManagementSettings fromEditor = EditorLookup();
                if (fromEditor != null)
                {
                    return fromEditor;
                }
            }

            GameManagementSettings[] loaded = Resources.FindObjectsOfTypeAll<GameManagementSettings>();
            if (loaded.Length == 0)
            {
                return null;
            }
            return loaded[0];
        }
    }
}
