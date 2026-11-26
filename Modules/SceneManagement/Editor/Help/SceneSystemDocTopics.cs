namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Topic ids that code opens with <see cref="SceneSystemDocs.Open"/>. An id is the page path under Pages/ without
    /// ".md", then "#" and the heading's slug. Ids never change once code uses them; a test checks each one resolves.
    /// </summary>
    public static class SceneSystemDocTopics
    {
        public const string Home = "home";

        // Project Settings > Abstract Pixel > Scene System, its eight sections, and Preferences > Abstract Pixel > Scenes.
        public const string SceneSystemPage = "guide/editor-tools#scene-groups-debugger";
        public const string SetupSection = "guide/get-started#run-the-setup";
        public const string StartUpSection = "guide/start-up#pick-the-start-scene-group";
        public const string SceneSetsSection = "guide/scene-groups-and-scene-sets#scene-sets";
        public const string LoadingScenesSection = "guide/loading-scene#show-a-loading-scene";
        public const string NewScenesSection = "guide/build-settings-and-addressables#where-new-scenes-go";
        public const string DebuggerSection = "guide/editor-tools#scene-groups-debugger";
        public const string SampleGroupsSection = "guide/editor-tools#sample-scene-groups";
        public const string RelocationSection = "guide/relocation#what-happens-in-a-scene-change";
        public const string ScenesPreferences = "guide/editor-tools#scene-opening-preferences";

        // Inspectors, windows and tools.
        public const string SceneGroupInspector = "guide/scene-groups-and-scene-sets#scene-groups";
        public const string SceneSetInspector = "guide/scene-groups-and-scene-sets#scene-sets";
        public const string RelocatableInspector = "guide/relocation#make-an-object-a-relocatable";
        public const string SpawnPointInspector = "guide/relocation#add-spawn-points";
        public const string RelocationLabelInspector = "guide/relocation#make-a-relocation-label";
        public const string ScenePicker = "guide/editor-tools#scene-picker";
        public const string SceneSetRemoval = "guide/scene-groups-and-scene-sets#scene-sets";
        public const string SetupWindow = "guide/get-started#run-the-setup";

        // Setup steps.
        public const string SetupWelcome = "guide/get-started#run-the-setup";
        public const string SetupFolder = "guide/get-started#run-the-setup";
        public const string SetupNewScenes = "guide/build-settings-and-addressables#where-new-scenes-go";
        public const string SetupLaunchScene = "guide/start-up#launch-scene";
        public const string SetupLoadingScene = "guide/loading-scene#show-a-loading-scene";
        public const string SetupSceneSets = "guide/scene-groups-and-scene-sets#scene-sets";
        public const string SetupOpeningScenes = "guide/editor-tools#scene-opening-preferences";
        public const string SetupReady = "guide/get-started#run-the-setup";

        // Group check problems: their Find a problem entries.
        public const string ProblemSceneGroup = "reference/find-a-problem#scene-group-problems";
        public const string ProblemSceneStatus = "reference/find-a-problem#scene-status";
        public const string ProblemRelocation = "reference/find-a-problem#relocation";

        // Components and assets with a help row.
        public const string MonoSceneActions = "types/monosceneactions";
        public const string LoadingProgressView = "types/loadingprogressview";
        public const string SimpleLoadingProgressBar = "types/simpleloadingprogressbar";
        public const string LaunchSceneReleaseReceiver = "types/launchscenereleasereceiver";
        public const string LaunchSceneTimelineClock = "types/launchscenetimelineclock";
        public const string LaunchSceneRenderSettingsGuard = "types/launchscenerendersettingsguard";
        public const string LaunchSceneStartUpLog = "types/launchscenestartuplog";
        public const string SceneSystemSettings = "types/scenesystemsettings";
        public const string RelocationLabel = "types/relocationlabel";
        public const string TransitionProfile = "types/transitionprofile";
        public const string ImageFadeProfile = "types/imagefadeprofile";

        /// <summary>The Find a problem entry for a group check problem.</summary>
        public static string ForProblem(SceneGroupProblemKind _kind)
        {
            switch (_kind)
            {
                case SceneGroupProblemKind.MissingScene: return ProblemSceneStatus;
                case SceneGroupProblemKind.NotInBuild: return ProblemSceneStatus;
                case SceneGroupProblemKind.RelocationSetup: return ProblemRelocation;
                default: return ProblemSceneGroup;
            }
        }
    }
}
