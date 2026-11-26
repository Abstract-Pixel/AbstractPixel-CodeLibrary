using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Undo and Redo change scene groups, scene sets and the settings in memory only, while every edit before them was
    /// saved at once. This saves the scene system assets an Undo or Redo left dirty, so the disk matches what the Editor
    /// shows and a crash loses nothing (session 4B-2 persistence audit).
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSystemUndoSave
    {
        static SceneSystemUndoSave()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
            Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private static void HandleUndoRedo()
        {
            SaveDirtySceneSystemAssets();
        }

        /// <summary>Saves every loaded scene group, scene set and settings asset with unsaved changes. Returns how many.</summary>
        public static int SaveDirtySceneSystemAssets()
        {
            int saved = 0;
            saved += SaveDirty(Resources.FindObjectsOfTypeAll<SceneGroup>());
            saved += SaveDirty(Resources.FindObjectsOfTypeAll<SceneSet>());
            saved += SaveDirty(Resources.FindObjectsOfTypeAll<SceneSystemSettings>());
            return saved;
        }

        private static int SaveDirty(Object[] _assets)
        {
            int saved = 0;
            foreach (Object asset in _assets)
            {
                bool isUnsavedAsset = asset != null && EditorUtility.IsPersistent(asset) && EditorUtility.IsDirty(asset);
                if (!isUnsavedAsset)
                {
                    continue;
                }
                AssetDatabase.SaveAssetIfDirty(asset);
                saved++;
            }
            return saved;
        }
    }
}
