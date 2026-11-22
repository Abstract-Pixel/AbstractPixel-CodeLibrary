namespace AbstractPixel.SaveSystem
{
    /// <summary>Where one save file lives: its storage key, the facts written into the file, and whether it keeps a backup.</summary>
    internal readonly struct SaveFileAddress
    {
        public string Key { get; }
        public string FileName { get; }

        /// <summary>The game profile the file belongs to, or null for Global save files.</summary>
        public string GameProfileId { get; }

        /// <summary>The Backup box of the file's save file row.</summary>
        public bool KeepsBackup { get; }

        public SaveFileAddress(string key, string fileName, string gameProfileId, bool keepsBackup = true)
        {
            Key = key;
            FileName = fileName;
            GameProfileId = gameProfileId;
            KeepsBackup = keepsBackup;
        }
    }
}
