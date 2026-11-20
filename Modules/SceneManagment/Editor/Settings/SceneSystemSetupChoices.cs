namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the designer chose in the setup: where the settings asset goes, where new scenes go, whether to migrate.</summary>
    public class SceneSystemSetupChoices
    {
        public string Folder = SceneSystemSetup.DefaultFolder;
        public bool NewScenesAreAddressable = false;

        /// <summary>When the project has scene groups: migrate them to scene sets instead of making a starter scene set.</summary>
        public bool MigrateGroups = true;
    }
}
