namespace AbstractPixel.SaveSystem
{
    /// <summary>Where one save file lives: its storage key, plus the facts written into the file.</summary>
    internal readonly struct SaveFileAddress
    {
        public string Key { get; }
        public string FileName { get; }

        /// <summary>The game profile the file belongs to, or null for Global save files.</summary>
        public string GameProfileId { get; }

        public SaveFileAddress(string key, string fileName, string gameProfileId)
        {
            Key = key;
            FileName = fileName;
            GameProfileId = gameProfileId;
        }
    }
}
