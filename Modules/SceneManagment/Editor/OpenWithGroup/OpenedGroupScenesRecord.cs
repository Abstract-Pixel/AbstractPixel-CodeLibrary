using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scenes "Open the group with its main scene" opened, by path, kept for the Editor session under the tool's
    /// own SessionState key.
    /// </summary>
    public static class OpenedGroupScenesRecord
    {
        public const string SessionKey = "AbstractPixel.SceneSystem.OpenedWithGroup";
        private const char Separator = '\n';

        public static List<SceneReference> Read()
        {
            List<SceneReference> scenes = new List<SceneReference>();
            string stored = SessionState.GetString(SessionKey, string.Empty);
            if (string.IsNullOrEmpty(stored))
            {
                return scenes;
            }
            foreach (string path in stored.Split(Separator))
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                string guid = AssetDatabase.AssetPathToGUID(path);
                scenes.Add(new SceneReference(sceneName, guid, path));
            }
            return scenes;
        }

        public static void Write(List<SceneReference> _scenes)
        {
            List<string> paths = new List<string>();
            foreach (SceneReference scene in _scenes)
            {
                if (!string.IsNullOrEmpty(scene.ScenePath))
                {
                    paths.Add(scene.ScenePath);
                }
            }
            SessionState.SetString(SessionKey, string.Join(Separator.ToString(), paths));
        }
    }
}
