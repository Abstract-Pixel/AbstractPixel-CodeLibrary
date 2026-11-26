using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The result of <see cref="StartUpRule.Choose"/>: what start-up does, the scene group it opens (none for a temporary group, which the caller builds), and at most one line of each kind for the console.</summary>
    public class StartUpChoice
    {
        public StartUpChoiceKind Kind { get; internal set; }

        /// <summary>The scene group to open. Null for <see cref="StartUpChoiceKind.TemporaryGroup"/> and DoNothing.</summary>
        public SceneGroup Group { get; internal set; }

        /// <summary>The open scene that picked the group, or the active scene that becomes a temporary group's main scene.</summary>
        public SceneReference MainScene { get; internal set; }

        /// <summary>The scene set a temporary group is built from (the default scene set, may be null).</summary>
        public SceneSet SceneSet { get; internal set; }

        public string ErrorMessage { get; internal set; } = string.Empty;
        public string WarningMessage { get; internal set; } = string.Empty;
        public string InfoMessage { get; internal set; } = string.Empty;
    }
}
