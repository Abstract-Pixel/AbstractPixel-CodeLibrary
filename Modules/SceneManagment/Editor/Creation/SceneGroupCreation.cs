using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The rules behind "Create Scene Group": where the new group goes, and which group already uses a scene as its main scene.</summary>
    public static class SceneGroupCreation
    {
        /// <summary>The new group's path: next to the scene, named after it, "Level 7.unity" becomes "Level 7Group.asset".</summary>
        public static string GroupPathFor(string _scenePath)
        {
            string folder = Path.GetDirectoryName(_scenePath).Replace('\\', '/');
            string sceneName = Path.GetFileNameWithoutExtension(_scenePath);
            return folder + "/" + sceneName + "Group.asset";
        }

        /// <summary>The group whose main scene has this asset GUID, or null. A scene is the main scene of at most one scene group.</summary>
        public static SceneGroup FindGroupWithMainScene(IEnumerable<SceneGroup> _groups, string _sceneGuid)
        {
            if (_groups == null || string.IsNullOrEmpty(_sceneGuid))
            {
                return null;
            }
            foreach (SceneGroup group in _groups)
            {
                if (group == null)
                {
                    continue;
                }
                SceneReference mainScene = group.MainScene;
                if (mainScene == null)
                {
                    continue;
                }
                if (mainScene.SceneGuid == _sceneGuid)
                {
                    return group;
                }
            }
            return null;
        }
    }
}
