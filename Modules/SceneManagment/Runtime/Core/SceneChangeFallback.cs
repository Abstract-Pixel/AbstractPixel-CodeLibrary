using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Where a failed scene change goes (ADR 0011): the fallback scene group from the scene system settings, once. If
    /// there is none, if the failed change was already going there, or if it fails too, the screen is shown on whatever
    /// is open and the error is logged. It never loops and never tries a failed load again.
    /// </summary>
    internal static class SceneChangeFallback
    {
        /// <summary>The settings' fallback scene group (the start scene group when none is set), or null.</summary>
        public static SceneGroup FromSettings()
        {
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            if (settings == null)
            {
                return null;
            }
            return settings.FallbackSceneGroup;
        }

        /// <param name="_changeScenes">Runs the scene work of a change and returns the scene that failed, or null.</param>
        public static async Task GoToFallback(SceneGroup _failedGroup, SceneGroup _fallback, Func<SceneGroup, Task<SceneReference>> _changeScenes)
        {
            if (_fallback == null)
            {
                Debug.LogWarning("[Scene system] No fallback scene group is set. The screen is shown on whatever is open.");
                return;
            }
            if (_fallback == _failedGroup)
            {
                Debug.LogError($"[Scene system] The fallback scene group '{_fallback.name}' itself failed. The screen is shown on whatever is open.");
                return;
            }

            Debug.LogWarning($"[Scene system] Going to the fallback scene group '{_fallback.name}'.");
            SceneReference fallbackFailure = await _changeScenes(_fallback);
            if (fallbackFailure != null)
            {
                Debug.LogError($"[Scene system] The fallback scene group '{_fallback.name}' failed too. The screen is shown on whatever is open.");
            }
        }
    }
}
