using System.Linq;
using AbstractPixel.GameManagement.Debugging;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Leaves the debugger's code out of a release build without the opt-in, and preloads its UI assets only into
    /// builds that hold the code. Nothing else references either, so this is all that puts them in a build.</summary>
    internal class DebuggerBuildSteps : IFilterBuildAssemblies, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // After the settings are preloaded and the state check, so a stopped build leaves nothing extra behind.
        public int callbackOrder
        {
            get { return 2; }
        }

        public string[] OnFilterAssemblies(BuildOptions _options, string[] _assemblies)
        {
            return DebuggerBuildRule.Filter(_assemblies, Includes(_options));
        }

        public void OnPreprocessBuild(BuildReport _report)
        {
            Object[] preloaded = WithoutDebuggerAssets(PlayerSettings.GetPreloadedAssets());
            if (Includes(_report.summary.options))
            {
                DebuggerOverlayAssets assets = DebuggerEditorHookup.LoadAssets();
                preloaded = preloaded.Append(assets).ToArray();
            }
            PlayerSettings.SetPreloadedAssets(preloaded);
        }

        public void OnPostprocessBuild(BuildReport _report)
        {
            PlayerSettings.SetPreloadedAssets(WithoutDebuggerAssets(PlayerSettings.GetPreloadedAssets()));
        }

        private static Object[] WithoutDebuggerAssets(Object[] _preloaded)
        {
            return _preloaded.Where(_item => !(_item is DebuggerOverlayAssets)).ToArray();
        }

        private static bool Includes(BuildOptions _options)
        {
            GameManagementSettings settings = GameManagementSettingsAssets.Find();
            bool optedIn = settings != null && settings.IncludeDebuggerInReleaseBuilds;
            return DebuggerBuildRule.Includes((_options & BuildOptions.Development) != 0, optedIn);
        }
    }
}
