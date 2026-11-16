using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples
{
    /// <summary>
    /// Keeps the samples out of player builds. Their scripts compile only in the Editor (the UNITY_EDITOR define
    /// constraint on AbstractPixel.SaveSystem.Samples.asmdef), so a sample scene in a build would have missing scripts.
    /// This stops the build and names the scene to take out of the build's scene list.
    /// </summary>
    sealed class SamplesBuildGuard : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            bool isPlayerBuild = report != null;
            if (!isPlayerBuild)
            {
                return;
            }
            if (IsInSamplesFolder(scene.path) || UsesASampleScript(scene))
            {
                throw new BuildFailedException($"[SaveSystem] '{scene.path}' uses the Save System Samples, which are for learning in the Editor only and never go into a build. Take it out of the build's scene list (File > Build Profiles), or remove the sample scripts from it.");
            }
        }

        static bool IsInSamplesFolder(string scenePath)
        {
            string asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(typeof(SamplesBuildGuard).Assembly.GetName().Name);
            if (string.IsNullOrEmpty(asmdefPath) || string.IsNullOrEmpty(scenePath))
            {
                return false;
            }
            string samplesFolder = Path.GetDirectoryName(asmdefPath).Replace(Path.DirectorySeparatorChar, '/') + "/";
            return scenePath.Replace(Path.DirectorySeparatorChar, '/').StartsWith(samplesFolder);
        }

        static bool UsesASampleScript(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Any(script => script != null && script.GetType().Assembly == typeof(SamplesBuildGuard).Assembly);
        }
    }
}
