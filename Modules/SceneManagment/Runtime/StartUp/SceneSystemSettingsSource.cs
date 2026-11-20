using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Finds the scene system settings at runtime. A build loads them with Player Settings' Preloaded Assets before the
    /// first scene. The Editor does not load Preloaded Assets for Play, so the scene system's editor code registers a
    /// lookup that finds the asset by type.
    /// </summary>
    public static class SceneSystemSettingsSource
    {
        /// <summary>Set by the scene system's editor code. Null in a build.</summary>
        internal static Func<SceneSystemSettings> EditorLookup;

        /// <summary>The project's scene system settings, or null when there are none.</summary>
        public static SceneSystemSettings Find()
        {
            if (EditorLookup != null)
            {
                SceneSystemSettings fromEditor = EditorLookup();
                if (fromEditor != null)
                {
                    return fromEditor;
                }
            }

            SceneSystemSettings[] loaded = Resources.FindObjectsOfTypeAll<SceneSystemSettings>();
            if (loaded.Length == 0)
            {
                return null;
            }
            return loaded[0];
        }
    }
}
