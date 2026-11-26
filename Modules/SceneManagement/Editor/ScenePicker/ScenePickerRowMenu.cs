namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Which right-click entries of a row apply (session 5A2, decision 9). Plain data, so it is tested without a menu.
    /// Open and Open additively need a scene to open and Edit mode; Select scene needs a scene (a group's main scene);
    /// Select scene group needs the row's group; Copy path needs an asset.
    /// </summary>
    public sealed class ScenePickerRowMenu
    {
        public const string OpenText = "Open";
        public const string OpenAdditivelyText = "Open additively";
        public const string SelectSceneText = "Select scene";
        public const string SelectGroupText = "Select scene group";
        public const string CopyPathText = "Copy path";

        public bool CanOpen;
        public bool CanOpenAdditively;
        public bool CanSelectScene;
        public bool CanSelectGroup;
        public bool CanCopyPath;

        /// <summary>The scene group "Select scene group" selects: the group row itself, or a scene's group.</summary>
        public string GroupGuid = string.Empty;

        public static ScenePickerRowMenu For(ScenePickerRow _row, bool _inPlayMode)
        {
            ScenePickerRowMenu menu = new ScenePickerRowMenu();
            if (_row == null || _row.IsHeading)
            {
                return menu;
            }
            bool hasScene = _row.CanOpen;
            menu.CanOpen = hasScene && !_inPlayMode;
            menu.CanOpenAdditively = hasScene && !_inPlayMode;
            menu.CanSelectScene = hasScene;
            menu.GroupGuid = _row.Kind == ScenePickerRowKind.SceneGroup ? _row.AssetGuid : _row.GroupGuid;
            menu.CanSelectGroup = !string.IsNullOrEmpty(menu.GroupGuid);
            menu.CanCopyPath = !string.IsNullOrEmpty(_row.AssetGuid);
            return menu;
        }
    }
}
