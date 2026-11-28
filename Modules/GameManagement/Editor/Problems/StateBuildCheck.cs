using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Stops a build when the state list has errors and the settings ask for the check.</summary>
    internal class StateBuildCheck : IPreprocessBuildWithReport
    {
        private const string Where = " Fix it in Project Settings > Abstract Pixel > Game Management.";

        public int callbackOrder
        {
            get { return 1; }
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            GameManagementSettings settings = GameManagementSettingsAssets.Find();
            if (settings == null || !settings.CheckStatesBeforeBuild)
            {
                return;
            }
            StringBuilder errors = new StringBuilder();
            foreach (StateProblem problem in StateListValidator.Find(settings.Catalog))
            {
                if (problem.IsError)
                {
                    Debug.LogError("[Game management] " + problem.Message + Where, problem.State);
                    errors.AppendLine(problem.Message);
                }
            }
            if (errors.Length > 0)
            {
                throw new BuildFailedException("[Game management] The state list has errors." + Where + "\n" + errors);
            }
        }
    }
}
