using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    public static class SaveSystemToolsMenu
    {
        [MenuItem("Tools/Save System/Open Release Folder")]
        public static void OpenReleaseFolderFromMenu()
        {
            if (TryInitializeGenerator())
            {
                OpenDirectory(SavePathGenerator.ShipRootPath);
            }
        }

        [MenuItem("Tools/Save System/Open Debug Folder")]
        public static void OpenDebugFolderFromMenu()
        {
            if (TryInitializeGenerator())
            {
                OpenDirectory(SavePathGenerator.DebugRootPath);
            }
        }
        [MenuItem("Tools/Save System/Delete All Save Files")]
        public static void DeleteAllSavesFromMenu()
        {

            bool confirm = EditorUtility.DisplayDialog(
                "Delete All Saves?",
                "Are you sure you want to delete ALL save files from the Tools menu?",
                "Yes, Delete",
                "Cancel"
            );

            if (confirm)
            {
                if (Directory.Exists(SavePathGenerator.ShipRootPath))
                    Directory.Delete(SavePathGenerator.ShipRootPath, true);

                if (Directory.Exists(SavePathGenerator.DebugRootPath))
                    Directory.Delete(SavePathGenerator.DebugRootPath, true);

                Debug.Log("[SaveSystem: FORCED] All files deleted via Tools Menu.");
            }
        }

        #region Helpers

        private static bool TryInitializeGenerator()
        {
            string[] guids = AssetDatabase.FindAssets("t:SaveSystemConfigSO");

            if (guids.Length == 0)
            {
                Debug.LogError("[SaveSystem] Could not find a SaveSystemConfigSO in the project! Menu item failed.");
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            SaveSystemConfigSO config = AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(path);

            SavePathGenerator.Initialize(config);
            return true;
        }

        private static void OpenDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        #endregion
    }
}