namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What a click on a row's status button does, and on which asset. Plain data, so it is tested without a window.</summary>
    public sealed class ScenePickerClick
    {
        public readonly ScenePickerClickKind Kind;

        /// <summary>The GUID of the asset the click selects or scrolls to; empty when the click needs none.</summary>
        public readonly string TargetGuid;

        /// <summary>The asset's name, for the tooltip.</summary>
        public readonly string TargetName;

        public ScenePickerClick(ScenePickerClickKind _kind, string _targetGuid, string _targetName)
        {
            Kind = _kind;
            TargetGuid = _targetGuid ?? string.Empty;
            TargetName = _targetName ?? string.Empty;
        }

        public static ScenePickerClick Of(ScenePickerClickKind _kind)
        {
            return new ScenePickerClick(_kind, string.Empty, string.Empty);
        }
    }
}
