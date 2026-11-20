using System;
using System.Collections.Generic;
using System.Globalization;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// One game profile, such as "Game Profile 1": one playthrough of the game. Hold it, show it on a save slot
    /// screen, and call Open() or Delete() on it. You never type its ID.
    /// </summary>
    public sealed class GameProfile
    {
        readonly GameProfileList list;

        internal GameProfileManifest Manifest { get; set; }

        /// <summary>The game profile's folder name on disk. Only the save system uses it.</summary>
        internal string Id => Manifest.GameProfileId;

        internal GameProfile(GameProfileList list, GameProfileManifest manifest)
        {
            this.list = list;
            Manifest = manifest;
        }

        /// <summary>The number the save slot screen shows. It never changes.</summary>
        public int SlotNumber => Manifest.SlotNumber;

        /// <summary>The name players see, such as "Game Profile 1" or a custom name.</summary>
        public string DisplayName => Manifest.DisplayName;

        /// <summary>When this game profile was last saved, in local time, or null if it was never saved.</summary>
        public DateTime? LastSaved
        {
            get
            {
                bool wasSaved = DateTime.TryParse(Manifest.LastSavedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastSavedUtc);
                if (!wasSaved)
                {
                    return null;
                }
                return lastSavedUtc.ToLocalTime();
            }
        }

        /// <summary>How long this game profile has been played, counted up to its last save.</summary>
        public TimeSpan Playtime => TimeSpan.FromSeconds(Manifest.PlaytimeSeconds);

        /// <summary>The scene that was active at the last save.</summary>
        public string LastScene => Manifest.LastScene;

        /// <summary>Your own fields for the save slot screen, such as difficulty or chapter. They are written on the next save.</summary>
        public Dictionary<string, string> Custom => Manifest.Custom;

        /// <summary>True while this is the open game profile.</summary>
        public bool IsOpen => list.IsOpen(this);

        /// <summary>Opens this game profile: its saved objects are restored and its data is saved from now on.</summary>
        public void Open()
        {
            list.Open(this);
        }

        /// <summary>Deletes this game profile and all of its save files. If it was open, no game profile is open afterwards.</summary>
        public void Delete()
        {
            list.Delete(this);
        }

        /// <summary>Changes the name players see. The slot number stays the same.</summary>
        public void Rename(string newDisplayName)
        {
            list.Rename(this, newDisplayName);
        }

        public override string ToString()
        {
            return $"{SlotNumber}: {DisplayName}";
        }
    }
}
