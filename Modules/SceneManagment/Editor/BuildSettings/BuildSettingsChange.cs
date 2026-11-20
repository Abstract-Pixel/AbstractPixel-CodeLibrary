using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What adding a scene group's scenes to Build Settings would do: the new scene list, and which scenes it added,
    /// switched on, or could not add because the scene file is missing.
    /// </summary>
    public class BuildSettingsChange
    {
        public EditorBuildSettingsScene[] Scenes { get; }
        public List<string> AddedPaths { get; }
        public List<string> EnabledPaths { get; }
        public List<string> MissingPaths { get; }

        public BuildSettingsChange(EditorBuildSettingsScene[] _scenes, List<string> _addedPaths, List<string> _enabledPaths, List<string> _missingPaths)
        {
            Scenes = _scenes;
            AddedPaths = _addedPaths;
            EnabledPaths = _enabledPaths;
            MissingPaths = _missingPaths;
        }

        /// <summary>True when Build Settings would change: a scene is added or switched on.</summary>
        public bool HasChanges
        {
            get
            {
                bool addsScenes = AddedPaths.Count > 0;
                bool enablesScenes = EnabledPaths.Count > 0;
                return addsScenes || enablesScenes;
            }
        }
    }
}
