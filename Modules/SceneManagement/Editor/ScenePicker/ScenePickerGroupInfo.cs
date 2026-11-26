using System.Collections.Generic;
using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>One scene group asset as the scene picker reads it from the project. Plain data, so rows can be tested.</summary>
    public class ScenePickerGroupInfo
    {
        public string Name;
        public string Path;
        public string Guid;
        public bool IsSample;

        /// <summary>Empty when the group has no main scene or its scene asset is gone.</summary>
        public string MainSceneName = string.Empty;
        public string MainScenePath = string.Empty;
        public string MainSceneGuid = string.Empty;
        public SceneStatus MainSceneStatus = SceneStatus.NoScene;

        /// <summary>Empty when the group follows no scene set.</summary>
        public string SceneSetName = string.Empty;
        public string SceneSetGuid = string.Empty;

        /// <summary>The group's number in the Scene System page's list (the Scene Groups Debugger); 0 for a sample group.</summary>
        public int Number;

        /// <summary>The group check's result (game groups only): how many problems, the first one's message, and
        /// whether one is fixed by Make Addressable.</summary>
        public int ProblemCount;
        public string FirstProblem = string.Empty;
        public bool HasMakeAddressableProblem;

        /// <summary>A boot group from before scene sets (SceneGroupCheck.IsBootGroup): no place, no problem.</summary>
        public bool IsBootGroup;

        /// <summary>The GUIDs of the group's manager and shared scenes.</summary>
        public List<string> OtherSceneGuids = new List<string>();

        public ScenePickerGroupInfo(string _name, string _path, string _guid)
        {
            Name = _name;
            Path = _path;
            Guid = _guid;
        }

        public bool HasMainScene
        {
            get { return !string.IsNullOrEmpty(MainSceneGuid) && MainSceneStatus != SceneStatus.Missing && MainSceneStatus != SceneStatus.NoScene; }
        }
    }
}
