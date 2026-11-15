using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Fails the build when a scene has a saved script with no Savable Bridge, instead of shipping a scene whose save ID would change on the next build.</summary>
    public sealed class SavableBridgeSceneBuildGuard : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Unity also calls this for every scene loaded in play mode in the Editor, with no report. Throwing
            // there would break the scene load; SavableBridgePlayModeCheck already logs that scene.
            bool isABuild = report != null;
            if (isABuild)
            {
                CheckScene(scene);
            }
        }

        /// <summary>Thrown as a test fixture, instead of needing a real build to exercise this.</summary>
        internal static void CheckScene(Scene scene)
        {
            List<GameObject> missing = SavableBridgeSceneCheck.FindMissingBridges(scene);
            if (missing.Count > 0)
            {
                throw new BuildFailedException(SavableBridgeSceneCheck.Describe(scene, missing, "build again"));
            }
        }
    }
}
