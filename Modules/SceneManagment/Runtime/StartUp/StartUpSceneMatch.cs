using System;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Says whether an open scene is a scene named in the settings: by GUID when both sides know one, otherwise by
    /// path when both know one, otherwise by name. A player build knows no GUID for an open scene, only its path.
    /// </summary>
    public static class StartUpSceneMatch
    {
        public static bool IsSameScene(SceneReference _first, SceneReference _second)
        {
            if (_first == null || _second == null)
            {
                return false;
            }

            bool bothHaveGuid = _first.HasGuid && _second.HasGuid;
            if (bothHaveGuid)
            {
                return string.Equals(_first.SceneGuid, _second.SceneGuid, StringComparison.Ordinal);
            }

            bool bothHavePath = !string.IsNullOrEmpty(_first.ScenePath) && !string.IsNullOrEmpty(_second.ScenePath);
            if (bothHavePath)
            {
                return string.Equals(_first.ScenePath, _second.ScenePath, StringComparison.OrdinalIgnoreCase);
            }

            bool bothHaveName = !string.IsNullOrEmpty(_first.SceneName) && !string.IsNullOrEmpty(_second.SceneName);
            if (bothHaveName)
            {
                return string.Equals(_first.SceneName, _second.SceneName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>True when the list holds the scene.</summary>
        public static bool ListHolds(System.Collections.Generic.IEnumerable<SceneReference> _scenes, SceneReference _scene)
        {
            if (_scenes == null)
            {
                return false;
            }

            foreach (SceneReference candidate in _scenes)
            {
                if (IsSameScene(candidate, _scene))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
