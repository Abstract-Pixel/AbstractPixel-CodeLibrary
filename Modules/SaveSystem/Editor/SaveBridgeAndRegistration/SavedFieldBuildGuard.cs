using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Fails the build while SavedFieldScan has found a [Save] mistake, instead of shipping broken saves.</summary>
    public sealed class SavedFieldBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (SavedFieldScan.HasProblems)
            {
                throw new BuildFailedException("[SaveSystem] One or more [Save] fields are invalid. Fix the errors the console logged after the last compile, then build again.");
            }
        }
    }
}
