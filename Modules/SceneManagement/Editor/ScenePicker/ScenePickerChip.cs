namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The colour of a row's status button, from <see cref="ScenePickerStatusRules"/>.</summary>
    public enum ScenePickerChip
    {
        /// <summary>Green: In build, and on the Scene Groups tab no group problem.</summary>
        InBuild,

        /// <summary>Blue: Addressable, and on the Scene Groups tab no group problem.</summary>
        Addressable,

        /// <summary>Red: Not in build, Missing or No scene; on the Scene Groups tab, any group problem.</summary>
        Problem,

        /// <summary>Grey: a sample scene or sample scene group, which never goes into a build.</summary>
        Sample,

        /// <summary>Yellow: a boot group from before scene sets, no place and no problem ("Boot group, unused" in the Debugger).</summary>
        Unused
    }
}
