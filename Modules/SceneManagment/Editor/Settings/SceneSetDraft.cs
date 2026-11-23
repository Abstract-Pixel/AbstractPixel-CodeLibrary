using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A scene set setup will make (session 5A2, Scene Sets step): its name, its manager and shared scenes picked from
    /// the project, and Reset shared scenes. The first draft is the default scene set; it lists the starter Managers
    /// scene (which setup makes, holding the copied fade) until the designer removes it.
    /// </summary>
    public class SceneSetDraft
    {
        public const string DefaultName = "Default Scene Set";

        public string Name = DefaultName;

        /// <summary>Setup makes the starter Managers scene (with the copied fade) and lists it first among the manager scenes.</summary>
        public bool IncludeStarterManagers;

        /// <summary>Picked scenes; an empty slot (null) is a row the designer added but has not filled yet, and is skipped.</summary>
        public List<SceneAsset> ManagerScenes = new List<SceneAsset>();
        public List<SceneAsset> SharedScenes = new List<SceneAsset>();
        public bool ResetSharedScenes;

        /// <summary>The first scene set: "Default Scene Set" with the starter Managers scene.</summary>
        public static SceneSetDraft Default()
        {
            SceneSetDraft draft = new SceneSetDraft();
            draft.IncludeStarterManagers = true;
            return draft;
        }

        /// <summary>A further scene set from "Add another scene set": a free name, no scenes.</summary>
        public static SceneSetDraft Another(IEnumerable<SceneSetDraft> _existing)
        {
            SceneSetDraft draft = new SceneSetDraft();
            List<string> taken = new List<string>();
            foreach (SceneSetDraft other in _existing)
            {
                taken.Add(other.Name);
            }
            draft.Name = SceneSetNames.FirstFree("Scene Set", taken);
            return draft;
        }
    }
}
