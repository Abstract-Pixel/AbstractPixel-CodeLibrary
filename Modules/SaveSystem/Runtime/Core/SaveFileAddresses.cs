namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Works out where each save file lives right now: its storage key, which depends on the open game profile and
    /// the scene, and whether its save file row keeps a backup.
    /// </summary>
    internal sealed class SaveFileAddresses
    {
        readonly SaveFileLookup saveFiles;
        readonly SaveProfileManager profileManager;

        public SaveFileAddresses(SaveFileLookup saveFiles, SaveProfileManager profileManager)
        {
            this.saveFiles = saveFiles;
            this.profileManager = profileManager;
        }

        public bool TryGetAddress(SavedObjectEntry entry, out SaveFileAddress address)
        {
            address = default;
            return saveFiles.TryGetDefinitionByFileName(entry.SaveFileName, out SaveFileDefinition definition)
                && TryGetAddress(definition, entry.SceneName, out address);
        }

        // GameProfile and Scene save files have no address until a game profile is open.
        public bool TryGetAddress(SaveFileDefinition definition, string sceneName, out SaveFileAddress address)
        {
            address = default;
            bool needsGameProfile = definition.Location != SaveLocation.Global;
            if (needsGameProfile && profileManager.CurrentGameProfileId == null)
            {
                return false;
            }
            address = AddressOf(definition, definition.Location == SaveLocation.Scene ? sceneName : null);
            return true;
        }

        public SaveFileAddress AddressOf(SaveFileDefinition definition, string sceneName)
        {
            string gameProfileId = definition.Location == SaveLocation.Global ? null : profileManager.CurrentGameProfileId;
            string key = SavePathGenerator.GetSaveFileKey(definition.FileName, definition.Location, gameProfileId, sceneName);
            return new SaveFileAddress(key, definition.FileName, gameProfileId, definition.Backup);
        }
    }
}
