using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Finds the project's save system config for editor tools, the same one the game loads at startup.</summary>
    public static class SaveSystemConfigFinder
    {
        /// <summary>The path Create Config uses, and the one the game loads from Resources.</summary>
        public const string ConfigAssetPath = "Assets/Resources/" + SaveSystemConfigName.ResourceName + ".asset";

        /// <summary>The config the game will load, or null when the project has none yet.</summary>
        public static SaveSystemConfigSO FindInProject()
        {
            SaveSystemConfigSO config = AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(ConfigAssetPath);
            if (config != null)
            {
                return config;
            }

            // A config somewhere else still counts for the editor tools; the game only finds one in Resources.
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(SaveSystemConfigSO));
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                bool isInsideAssets = path.StartsWith("Assets/");
                if (isInsideAssets)
                {
                    return AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(path);
                }
            }
            return null;
        }
    }
}
