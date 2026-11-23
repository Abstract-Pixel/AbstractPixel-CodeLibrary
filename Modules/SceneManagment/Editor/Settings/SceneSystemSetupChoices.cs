using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the designer chose in the setup: where the settings asset goes, where new scenes go, an optional launch and loading scene, and the scene sets.</summary>
    public class SceneSystemSetupChoices
    {
        public string Folder = SceneSystemSetup.DefaultFolder;
        public bool NewScenesAreAddressable = false;

        /// <summary>Optional: a launch scene (such as a splash) the game opens first, put at build index 0.</summary>
        public LaunchSceneChoice LaunchScene = LaunchSceneChoice.None;
        public SceneAsset ExistingLaunchScene = null;
        public string NewLaunchScenePath = "Assets/Scenes/" + LaunchSceneCreation.DefaultFileName + ".unity";

        /// <summary>Optional: the default loading scene. The ready-made one is copied by default, as before this step existed.</summary>
        public LoadingSceneChoice LoadingScene = LoadingSceneChoice.ReadyMade;
        public SceneAsset ExistingLoadingScene = null;

        /// <summary>
        /// "Set up your scene sets now?" Yes: setup makes <see cref="SceneSets"/>, the first being the default. Not now:
        /// setup makes the Default Scene Set with the starter Managers scene, as before session 5A2.
        /// </summary>
        public bool SetUpSceneSetsNow = false;
        public List<SceneSetDraft> SceneSets = new List<SceneSetDraft> { SceneSetDraft.Default() };
    }
}
