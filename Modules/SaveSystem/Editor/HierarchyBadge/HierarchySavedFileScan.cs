using System.Collections.Generic;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Whether a saved script's data is already in its save file on disk, read directly instead of asking a running
    /// save system. Edit mode has no save system instance to ask: <see cref="SaveActivity"/> only knows about a
    /// play session, so the Hierarchy badge reads the files the same way the game would, across every game profile
    /// that exists, since none of them is "open" outside play mode.
    /// </summary>
    internal static class HierarchySavedFileScan
    {
        /// <summary>True if any of the bridge's saved scripts has a live entry (not a gone marker) in its save file.</summary>
        public static bool AnyTargetSavedOnDisk(SavableBridge bridge, IEnumerable<SavableTarget> targets, SaveFileLookup saveFiles, SaveFileStore store, IDataStorageService storage)
        {
            foreach (SavableTarget target in targets)
            {
                if (target == null || target.Script == null)
                {
                    continue;
                }
                if (IsTargetSavedOnDisk(bridge, target, saveFiles, store, storage))
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTargetSavedOnDisk(SavableBridge bridge, SavableTarget target, SaveFileLookup saveFiles, SaveFileStore store, IDataStorageService storage)
        {
            if (!saveFiles.TryGetDefinitionForSavedObject(target.Script.GetType(), out SaveFileDefinition definition))
            {
                return false;
            }
            string saveId = bridge.GetSaveId(target);

            if (definition.Location == SaveLocation.Global)
            {
                string key = SavePathGenerator.GetSaveFileKey(definition.FileName, SaveLocation.Global, null, null);
                return FileHasLiveEntry(store, key, saveId);
            }

            string sceneName = definition.Location == SaveLocation.Scene ? target.Script.gameObject.scene.name : null;
            bool sceneFileNeedsAScene = definition.Location == SaveLocation.Scene && string.IsNullOrEmpty(sceneName);
            if (sceneFileNeedsAScene)
            {
                return false;
            }

            foreach (string gameProfileId in ExistingGameProfileIds(storage))
            {
                string key = SavePathGenerator.GetSaveFileKey(definition.FileName, definition.Location, gameProfileId, sceneName);
                if (FileHasLiveEntry(store, key, saveId))
                {
                    return true;
                }
            }
            return false;
        }

        static bool FileHasLiveEntry(SaveFileStore store, string key, string saveId)
        {
            return store.TryRead(key, out SaveFileData data) && data.Entries.TryGetValue(saveId, out SaveFileEntry entry) && !entry.Gone;
        }

        // Every game profile id that has a manifest on disk, found by its manifest key, the same way
        // SaveProfileManager.ReadAllManifests does, without needing one open.
        static IEnumerable<string> ExistingGameProfileIds(IDataStorageService storage)
        {
            foreach (string key in storage.ListKeys(SavePathGenerator.GetAllGameProfilesKeyPrefix()))
            {
                string[] parts = key.Split('/');
                bool isManifestKey = parts.Length == 3 && key == SavePathGenerator.GetGameProfileManifestKey(parts[1]);
                if (isManifestKey)
                {
                    yield return parts[1];
                }
            }
        }
    }
}
