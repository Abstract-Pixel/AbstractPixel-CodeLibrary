using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Gives every relocation label asset its ID (its asset GUID) when it is imported, so labels made before IDs
    /// existed, and copies made with Duplicate, match correctly in builds where Addressables carry their own copy.
    /// </summary>
    internal sealed class RelocationLabelIds : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] _imported, string[] _deleted, string[] _moved, string[] _movedFrom)
        {
            StampAll(_imported);
            StampAll(_moved);
        }

        private static void StampAll(string[] _paths)
        {
            foreach (string path in _paths)
            {
                if (!path.EndsWith(".asset"))
                {
                    continue;
                }
                RelocationLabel label = AssetDatabase.LoadAssetAtPath<RelocationLabel>(path);
                if (label != null)
                {
                    Stamp(label, path);
                }
            }
        }

        /// <summary>Sets the label's ID to its asset GUID and saves it when it differs.</summary>
        public static void Stamp(RelocationLabel _label, string _path)
        {
            string guid = AssetDatabase.AssetPathToGUID(_path);
            if (_label.Id == guid)
            {
                return;
            }
            _label.Id = guid;
            EditorUtility.SetDirty(_label);
            AssetDatabase.SaveAssetIfDirty(_label);
        }

        [MenuItem("Tools/Abstract Pixel/Scene System/Stamp relocation label IDs")]
        public static void StampEveryLabel()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(RelocationLabel)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RelocationLabel label = AssetDatabase.LoadAssetAtPath<RelocationLabel>(path);
                if (label != null)
                {
                    Stamp(label, path);
                }
            }
        }
    }
}
