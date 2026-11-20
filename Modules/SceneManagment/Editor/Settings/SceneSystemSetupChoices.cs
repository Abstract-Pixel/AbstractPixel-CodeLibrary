using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the designer chose in the setup: where the settings asset goes, where new scenes go, whether to migrate, and an optional launch scene.</summary>
    public class SceneSystemSetupChoices
    {
        public string Folder = SceneSystemSetup.DefaultFolder;
        public bool NewScenesAreAddressable = false;

        /// <summary>When the project has scene groups: migrate them to scene sets instead of making a starter scene set.</summary>
        public bool MigrateGroups = true;

        /// <summary>Optional: a launch scene (such as a splash) the game opens first, put at build index 0.</summary>
        public LaunchSceneChoice LaunchScene = LaunchSceneChoice.None;
        public SceneAsset ExistingLaunchScene = null;
        public string NewLaunchScenePath = "Assets/Scenes/" + LaunchSceneCreation.DefaultFileName + ".unity";
    }
}
