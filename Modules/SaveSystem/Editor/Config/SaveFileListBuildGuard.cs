using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Fails the build while the save file list has changes that aren't applied yet.</summary>
    public sealed class SaveFileListBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config != null)
            {
                CheckConfig(config);
            }
        }

        /// <summary>Thrown as a test fixture, instead of relying on the config a real project happens to have.</summary>
        internal static void CheckConfig(SaveSystemConfigSO config)
        {
            if (SaveFileListStatus.IsOutOfDate(config))
            {
                throw new BuildFailedException(SaveFileListStatus.OutOfDateMessage);
            }
        }
    }
}
