using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Editor menu items for finding and clearing save files.</summary>
    public static class SaveSystemToolsMenu
    {
        const string MenuRoot = "Tools/External Project Tools/Save System/";

        /// <summary>The release and debug save folders, the same ones the game writes to.</summary>
        public static IReadOnlyList<string> GetSaveFolders()
        {
            return new[] { DiskStorageService.GetRootFolder(false), DiskStorageService.GetRootFolder(true) };
        }

        [MenuItem(MenuRoot + "Save All")]
        public static void SaveAll()
        {
            SaveSystem.SaveAll();
            Debug.Log("[SaveSystem] Saved every registered object (Save All from the Tools menu).");
        }

        [MenuItem(MenuRoot + "Load All")]
        public static void LoadAll()
        {
            SaveSystem.Session?.ReloadAll();
            Debug.Log("[SaveSystem] Re-read every save file and restored every registered object (Load All from the Tools menu).");
        }

        // Save All and Load All act on the running game, so they're greyed out outside play mode.
        [MenuItem(MenuRoot + "Save All", true)]
        [MenuItem(MenuRoot + "Load All", true)]
        static bool IsPlaying() => EditorApplication.isPlaying;

        [MenuItem(MenuRoot + "Open Release Folder")]
        public static void OpenReleaseFolder()
        {
            OpenFolder(DiskStorageService.GetRootFolder(false));
        }

        [MenuItem(MenuRoot + "Open Debug Folder")]
        public static void OpenDebugFolder()
        {
            OpenFolder(DiskStorageService.GetRootFolder(true));
        }

        [MenuItem(MenuRoot + "Delete All Save Files")]
        public static void DeleteAllSaveFiles()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete all save files?",
                "This deletes every save file in the release and debug save folders, for every game profile. It can't be undone.",
                "Delete",
                "Cancel");
            if (!confirmed)
            {
                return;
            }

            foreach (string folder in GetSaveFolders())
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            Debug.Log($"[SaveSystem] Deleted all save files in: {string.Join(", ", GetSaveFolders())}");
        }

        static void OpenFolder(string folder)
        {
            // The folder appears on the first save, so create it to have something to show.
            Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }
    }
}
