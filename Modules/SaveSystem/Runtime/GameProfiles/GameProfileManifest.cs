using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>The summary of one game profile that a save slot screen shows.</summary>
    [Serializable]
    public sealed class GameProfileManifest : IVersionedSaveData
    {
        public int FormatVersion = SaveFileStore.CurrentFormatVersion;
        public string GameProfileId;

        /// <summary>The number the save slot screen shows: 1, 2, 3. It never changes, even when other game profiles are deleted.</summary>
        public int SlotNumber;

        /// <summary>The name players see, for example "Game Profile 1".</summary>
        public string DisplayName;

        /// <summary>When the game profile was created, as UTC in ISO 8601 ("o") format.</summary>
        public string CreatedUtc;

        /// <summary>When the game profile was last saved, as UTC in ISO 8601 ("o") format. Empty until the first save.</summary>
        public string LastSavedUtc;

        public double PlaytimeSeconds;
        public string LastScene;

        /// <summary>Unused until a later step adds save slot screenshots.</summary>
        public string ScreenshotFile;

        /// <summary>The app version that last wrote this manifest.</summary>
        public string AppVersion;

        /// <summary>Game-specific fields for the save slot screen, such as difficulty or chapter.</summary>
        public Dictionary<string, string> Custom = new Dictionary<string, string>();

        int IVersionedSaveData.FormatVersion => FormatVersion;

        // Used by the JSON serializer when it reads a file.
        public GameProfileManifest() { }

        public GameProfileManifest(string gameProfileId, string displayName)
        {
            GameProfileId = gameProfileId;
            DisplayName = displayName;
            CreatedUtc = DateTime.UtcNow.ToString("o");
            AppVersion = Application.version;
        }
    }
}
