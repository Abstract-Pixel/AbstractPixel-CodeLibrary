using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Before a build: puts the scene system settings back into Preloaded Assets if they went missing, re-saves scene
    /// groups whose scene names are stale (a build reads the stored names), then, when
    /// Check scene groups before a build is on in the settings, stops the build if any scene group has a problem, with a
    /// message naming each group and scene, and opens the Scene System page at the Scene Groups Debugger (not in batch
    /// mode).
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
            SceneRenameTracking.RefreshStaleSceneGroups();
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
            if (!UnityEngine.Application.isBatchMode)
            {
                EditorApplication.delayCall += SceneSystemSettingsProvider.OpenAtDebugger;
            }
            throw new BuildFailedException(message);
        }
    }
}
