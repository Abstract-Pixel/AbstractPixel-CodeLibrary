using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>The save system's own record in Global, so the game can reopen the most recent game profile at startup.</summary>
    [Serializable]
    public sealed class SaveSystemMetaData : IVersionedSaveData
    {
        public int FormatVersion = SaveFileStore.CurrentFormatVersion;
        public string LastOpenedGameProfileId;

        int IVersionedSaveData.FormatVersion => FormatVersion;
    }
}
