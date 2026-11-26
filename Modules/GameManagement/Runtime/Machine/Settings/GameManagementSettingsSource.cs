using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
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
