using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene picker button's label (decision 11): the open group's name, found from the open scenes the way
    /// start-up finds it (the active scene first, then the others). With no group's main scene open, the active scene's
    /// name followed by "(no group)".
    /// </summary>
    public static class ScenePickerLabel
    {
        public const string NoGroupSuffix = " (no group)";
        public const string UntitledScene = "Untitled";

        /// <param name="_openScenesActiveFirst">The open scenes as (name, GUID) pairs, the active scene first.</param>
        public static string For(List<KeyValuePair<string, string>> _openScenesActiveFirst, List<ScenePickerGroupInfo> _groups)
        {
            if (_openScenesActiveFirst == null || _openScenesActiveFirst.Count == 0)
            {
                return UntitledScene + NoGroupSuffix;
            }
            foreach (KeyValuePair<string, string> scene in _openScenesActiveFirst)
            {
                ScenePickerGroupInfo group = GroupWithMainScene(scene.Value, _groups);
                if (group != null)
                {
                    return group.Name;
                }
            }
            string activeName = _openScenesActiveFirst[0].Key;
            if (string.IsNullOrEmpty(activeName))
            {
                activeName = UntitledScene;
            }
            return activeName + NoGroupSuffix;
        }

        private static ScenePickerGroupInfo GroupWithMainScene(string _sceneGuid, List<ScenePickerGroupInfo> _groups)
        {
            if (string.IsNullOrEmpty(_sceneGuid) || _groups == null)
            {
                return null;
            }
            foreach (ScenePickerGroupInfo group in _groups)
            {
                if (group != null && group.MainSceneGuid == _sceneGuid)
                {
                    return group;
                }
            }
            return null;
        }
    }
}
