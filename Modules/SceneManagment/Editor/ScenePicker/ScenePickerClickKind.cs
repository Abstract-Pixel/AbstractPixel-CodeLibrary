namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What a click on a row's status button does, from <see cref="ScenePickerStatusRules"/>.</summary>
    public enum ScenePickerClickKind
    {
        None,

        /// <summary>Build Profiles at the scene list in use: the active build profile's own list when it overrides the shared one.</summary>
        OpenBuildProfilesSceneList,

        /// <summary>The Addressables Groups window.</summary>
        OpenAddressablesGroups,

        /// <summary>Selects an asset (a scene group) and shows it in the Inspector.</summary>
        SelectAsset,

        /// <summary>The Scene System page at the Scene Groups Debugger, scrolled to the group's row.</summary>
        OpenDebuggerAtGroup
    }
}
