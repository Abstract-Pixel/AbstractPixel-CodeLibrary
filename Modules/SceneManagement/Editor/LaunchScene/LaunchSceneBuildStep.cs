using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Before a build: the launch scene must be build index 0. A build started from the Build Profiles window gets it put
    /// back before Unity reads the scene list. A scripted build has already read the list when this step runs, so it is
    /// put back and the build stops with a message saying so; the next build is right. Does nothing without a launch scene.
    /// </summary>
    [InitializeOnLoad]
    public class LaunchSceneBuildStep : IPreprocessBuildWithReport
    {
        static LaunchSceneBuildStep()
        {
            BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler(PutLaunchSceneFirstThenGetOptions);
        }

        public int callbackOrder
        {
            get { return 1; }
        }

        private static BuildPlayerOptions PutLaunchSceneFirstThenGetOptions(BuildPlayerOptions _options)
        {
            LaunchSceneBuildOrder.EnsureFirst(SceneSystemSettingsAssets.Find());
            return BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(_options);
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (LaunchSceneBuildOrder.IsInPlace(settings))
            {
                return;
            }
            string firstBefore = LaunchSceneBuildOrder.FirstScenePath();
            LaunchSceneBuildOrder.EnsureFirst(settings);
            throw new BuildFailedException("[Scene system] The launch scene '" + settings.LaunchScene.SceneName + "' was not build index 0, so this build would have opened '" + firstBefore + "' first. It is back at index 0 now: build again.");
        }
    }
}
