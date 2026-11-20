using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What a migration does: one scene set per distinct (manager scenes, shared scenes, Reset shared scenes)
    /// combination among scene groups with a main scene, with the groups that get each set. Index i of each list belongs
    /// to the same set.
    /// </summary>
    public class SceneGroupMigrationPlan
    {
        public readonly List<string> SetNames = new List<string>();
        public readonly List<List<SceneGroup>> GroupsBySet = new List<List<SceneGroup>>();
        public readonly List<SceneGroup> GroupsWithoutMainScene = new List<SceneGroup>();
        public readonly List<SceneGroup> GroupsAlreadyInASet = new List<SceneGroup>();

        /// <summary>The set used by the most groups, which becomes the default scene set. -1 when there is none.</summary>
        public int DefaultSetIndex = -1;

        public bool HasWork
        {
            get { return GroupsBySet.Count > 0; }
        }

        public int GroupsToMigrateCount
        {
            get
            {
                int count = 0;
                foreach (List<SceneGroup> groups in GroupsBySet)
                {
                    count += groups.Count;
                }
                return count;
            }
        }
    }
}
