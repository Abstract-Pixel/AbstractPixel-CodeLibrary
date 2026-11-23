using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>One row of the scene picker: what it shows, what search reads, and the asset a click opens or selects.</summary>
    public class ScenePickerRow
    {
        public ScenePickerRowKind Kind;

        /// <summary>The scene or scene group name, or the heading's text.</summary>
        public string Title = string.Empty;

        /// <summary>The label button's text: the scene's group, scene set, "Shared by X" or "No group"; the group's scene set.</summary>
        public string Detail = string.Empty;

        /// <summary>What the label button selects; None for "No group", which is plain text.</summary>
        public ScenePickerLabelKind LabelKind = ScenePickerLabelKind.None;

        /// <summary>The GUID of the scene group or scene set the label button selects.</summary>
        public string LabelGuid = string.Empty;

        /// <summary>The name of what the label button selects, for its tooltip ("Select level2Group").</summary>
        public string LabelTargetName = string.Empty;

        /// <summary>
        /// The number before the name: under Recent, 1 is the last used; on the Scene Groups tab, the group's number in
        /// the Scene System page's list (the Scene Groups Debugger). 0 shows no number.
        /// </summary>
        public int Number;

        /// <summary>A scene row's scene group: the group whose main scene it is, else the first that lists it.</summary>
        public string GroupGuid = string.Empty;
        public string GroupName = string.Empty;

        /// <summary>A group row's main scene name, shown in the row's tooltip.</summary>
        public string MainSceneName = string.Empty;

        /// <summary>A group row's group check: how many problems, the first one, and whether one is fixed by Make Addressable.</summary>
        public int ProblemCount;
        public string FirstProblem = string.Empty;
        public bool HasMakeAddressableProblem;

        /// <summary>A group row of a boot group from before scene sets (no main scene, no scene set, its own scenes).</summary>
        public bool IsBootGroup;

        /// <summary>The folder that holds the asset, such as "Assets/Levels/Level 3".</summary>
        public string Folder = string.Empty;

        /// <summary>The asset's GUID. A click finds the asset again by GUID, so a renamed or moved asset still opens.</summary>
        public string AssetGuid = string.Empty;
        public string AssetPath = string.Empty;

        public bool IsSample;

        /// <summary>The scene status of the scene, or of the group's main scene.</summary>
        public SceneStatus Status = SceneStatus.NoScene;

        /// <summary>False for a scene group with no main scene: it can be selected, never opened.</summary>
        public bool CanOpen = true;

        /// <summary>Lower-case text that search matches: name, folder and, for a scene, its group.</summary>
        public string SearchText = string.Empty;

        public static ScenePickerRow Heading(string _text)
        {
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = ScenePickerRowKind.Heading;
            row.Title = _text;
            row.CanOpen = false;
            return row;
        }

        public bool IsHeading
        {
            get { return Kind == ScenePickerRowKind.Heading; }
        }

        /// <summary>A copy with another number, for the Recent section, so the row in the full list keeps its own.</summary>
        public ScenePickerRow WithNumber(int _number)
        {
            ScenePickerRow copy = (ScenePickerRow)MemberwiseClone();
            copy.Number = _number;
            return copy;
        }
    }
}
