using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Every game profile, as <see cref="GameProfile"/> handles: <c>SaveActions.GameProfiles</c>.
    /// It is never null. Without a save system config it is an empty list whose methods log one clear error.
    /// </summary>
    public sealed class GameProfileList
    {
        /// <summary>The list used while the save system has no config.</summary>
        internal static readonly GameProfileList Empty = new GameProfileList(null);

        readonly SaveProfileManager manager;

        // One handle per game profile, so the same game profile is always the same object.
        readonly Dictionary<string, GameProfile> handlesById = new Dictionary<string, GameProfile>();
        readonly List<GameProfile> sortedHandles = new List<GameProfile>();

        // The manager's change count when sortedHandles was last built. A save slot screen reads All every frame,
        // so the files are only read again after something changed.
        int handlesBuiltAtChange = -1;

        internal GameProfileList(SaveProfileManager manager)
        {
            this.manager = manager;
        }

        /// <summary>Every game profile, sorted by slot number.</summary>
        public IReadOnlyList<GameProfile> All
        {
            get
            {
                RebuildHandlesIfSomethingChanged();
                return sortedHandles;
            }
        }

        /// <summary>The open game profile, or null when none is open.</summary>
        public GameProfile Current
        {
            get
            {
                if (manager == null || manager.CurrentGameProfileId == null)
                {
                    return null;
                }
                return GetById(manager.CurrentGameProfileId);
            }
        }

        /// <summary>Creates a game profile named "&lt;default game profile name&gt; &lt;slot number&gt;" in the lowest free slot. It doesn't open it.</summary>
        public GameProfile CreateNew()
        {
            return CreateNew(null);
        }

        /// <summary>Creates a game profile with a custom name in the lowest free slot. It doesn't open it.</summary>
        public GameProfile CreateNew(string displayName)
        {
            if (!HasSaveSystem(nameof(CreateNew)))
            {
                return null;
            }

            string gameProfileId = manager.CreateNew(displayName);
            if (gameProfileId == null)
            {
                return null;
            }
            return GetById(gameProfileId);
        }

        /// <summary>The game profile in this slot, or null if the slot is empty.</summary>
        public GameProfile Get(int slotNumber)
        {
            if (!HasSaveSystem(nameof(Get)))
            {
                return null;
            }

            foreach (GameProfile profile in All)
            {
                if (profile.SlotNumber == slotNumber)
                {
                    return profile;
                }
            }
            return null;
        }

        /// <summary>Closes the open game profile, for example when the player goes back to the main menu.</summary>
        public void CloseCurrent()
        {
            manager?.Close();
        }

        internal GameProfile GetById(string gameProfileId)
        {
            if (manager == null)
            {
                return null;
            }

            if (handlesById.TryGetValue(gameProfileId, out GameProfile handle))
            {
                return handle;
            }

            bool exists = manager.TryGetManifest(gameProfileId, out GameProfileManifest manifest);
            if (!exists)
            {
                return null;
            }
            handle = new GameProfile(this, manifest);
            handlesById.Add(gameProfileId, handle);
            return handle;
        }

        internal bool IsOpen(GameProfile profile)
        {
            return manager != null && manager.CurrentGameProfileId == profile.Id;
        }

        internal void Open(GameProfile profile)
        {
            if (HasSaveSystem(nameof(GameProfile.Open)))
            {
                manager.Open(profile.Id);
            }
        }

        internal void Delete(GameProfile profile)
        {
            if (!HasSaveSystem(nameof(GameProfile.Delete)))
            {
                return;
            }
            manager.Delete(profile.Id);
            handlesById.Remove(profile.Id);
        }

        internal void Rename(GameProfile profile, string newDisplayName)
        {
            if (!HasSaveSystem(nameof(GameProfile.Rename)))
            {
                return;
            }
            profile.Manifest.DisplayName = newDisplayName;
            manager.SaveManifest(profile.Manifest);
        }

        void RebuildHandlesIfSomethingChanged()
        {
            if (manager == null || handlesBuiltAtChange == manager.ChangeCount)
            {
                return;
            }

            sortedHandles.Clear();
            foreach (GameProfileManifest manifest in manager.GetAll())
            {
                GameProfile handle = GetById(manifest.GameProfileId);
                if (handle != null)
                {
                    // Each game profile has one manifest, the handle's, so custom fields the game set on it survive.
                    handle.Manifest.TakeSaveSystemFieldsFrom(manifest);
                    sortedHandles.Add(handle);
                }
            }
            // Reading can give older manifests a slot number, which counts as a change, so record the count afterwards.
            handlesBuiltAtChange = manager.ChangeCount;
        }

        bool HasSaveSystem(string methodName)
        {
            if (manager != null)
            {
                return true;
            }
            Debug.LogError($"[SaveSystem] SaveActions.GameProfiles.{methodName} did nothing: there is no save system config, so there are no game profiles. Use Tools > External Project Tools > Save System > Create Config.");
            return false;
        }
    }
}
