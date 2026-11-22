using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>One row of the scene picker: what it shows, what search reads, and the asset a click opens or selects.</summary>
    public class ScenePickerRow
    {
        public ScenePickerRowKind Kind;

        /// <summary>The scene or scene group name, or the heading's text.</summary>
        public string Title = string.Empty;

        /// <summary>The grey text after the title: the scene's group, or the group's main scene and scene set.</summary>
        public string Detail = string.Empty;

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
    }
}
