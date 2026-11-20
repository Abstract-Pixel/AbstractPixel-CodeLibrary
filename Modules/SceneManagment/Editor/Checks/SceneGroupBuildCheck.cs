using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Before a build: puts the scene system settings back into Preloaded Assets if they went missing, then, when
    /// Check scene groups before a build is on in the settings, stops the build if any scene group has a problem, with a
    /// message naming each group and scene.
    /// </summary>
    public class SceneGroupBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings == null)
            {
                return;
            }
            SceneSystemSettingsAssets.EnsurePreloaded(settings);
            if (!settings.CheckSceneGroupsBeforeBuild)
            {
                return;
            }
            Dictionary<string, List<SceneGroupProblem>> problemsByGroup = SceneGroupAssets.ProblemsByGroupPath();
            string message = SceneGroupCheck.BuildFailureMessage(problemsByGroup);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            throw new BuildFailedException(message);
        }
    }
}
