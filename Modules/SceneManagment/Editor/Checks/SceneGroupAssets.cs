using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Finds every scene group asset in the project, sorted by path.</summary>
    public static class SceneGroupAssets
    {
        public static List<SceneGroup> FindAll()
        {
            List<SceneGroup> groups = new List<SceneGroup>();
            List<string> paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneGroup)))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(System.StringComparer.OrdinalIgnoreCase);

            foreach (string path in paths)
            {
                SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(path);
                if (group == null)
                {
                    continue;
                }
                groups.Add(group);
            }
            return groups;
        }

        /// <summary>Each group's problems keyed by the group's asset path; groups without problems are left out.</summary>
        public static Dictionary<string, List<SceneGroupProblem>> ProblemsByGroupPath()
        {
            Dictionary<string, List<SceneGroupProblem>> problemsByPath = new Dictionary<string, List<SceneGroupProblem>>();
            foreach (SceneGroup group in FindAll())
            {
                List<SceneGroupProblem> problems = SceneGroupCheck.Find(group);
                if (problems.Count == 0)
                {
                    continue;
                }
                problemsByPath[AssetDatabase.GetAssetPath(group)] = problems;
            }
            return problemsByPath;
        }
    }
}
