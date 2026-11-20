using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The optional pre-build check: when Check Scene Groups Before Build is on, a build stops if any scene group has a
    /// problem, with a message naming each group and scene.
    /// </summary>
    public class SceneGroupBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            if (!SceneGroupBuildCheckPreference.IsOn)
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
