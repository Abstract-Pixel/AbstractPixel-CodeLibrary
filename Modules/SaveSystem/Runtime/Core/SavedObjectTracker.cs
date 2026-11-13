using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The registered saved objects and how they meet their data: capture, restore, forget and gone markers.
    /// It applies the whichever-second rule: an object is restored as soon as both the object and its data exist.
    /// One <see cref="SaveSession"/> owns one.
    /// </summary>
    internal sealed class SavedObjectTracker
    {
        readonly SaveRegistry registry = new SaveRegistry();
        readonly LoadedSaveData loadedData;
        readonly SaveFileAddresses addresses;
        readonly SaveProfileManager profileManager;

        public SavedObjectTracker(LoadedSaveData loadedData, SaveFileAddresses addresses, SaveProfileManager profileManager)
        {
            this.loadedData = loadedData;
            this.addresses = addresses;
            this.profileManager = profileManager;
        }

        public IEnumerable<SavedObjectEntry> AllEntries => registry.AllEntries;

        /// <summary>A plain class's automatic save ID in its save file: its class name, then "Inventory 2" and so on.</summary>
        public AutomaticSaveIds AutomaticSaveIds { get; } = new AutomaticSaveIds();

        /// <summary>
        /// Starts tracking the entry and restores it right away if its data can be loaded now. Its new-game values are
        /// kept first, before anything is restored.
        /// </summary>
        public void Track(SavedObjectEntry entry)
        {
            // A refused duplicate stays out of the save system, so it can't overwrite the first object's data.
            if (!registry.Add(entry))
            {
                return;
            }
            loadedData.RememberNewGameValues(entry);
            TryRestore(entry);
        }

        /// <summary>Captures the object's current data, then stops tracking it. Its data stays for the next save.</summary>
        public void Untrack(object owner)
        {
            foreach (SavedObjectEntry entry in registry.GetEntriesOf(owner))
            {
                Capture(entry);
                StopTracking(entry);
            }
        }

        /// <summary>Stops tracking every entry this object owns without capturing it, for an object that turned itself off.</summary>
        public void DropWithoutCapturing(object owner)
        {
            foreach (SavedObjectEntry entry in registry.GetEntriesOf(owner))
            {
                StopTracking(entry);
            }
        }

        /// <summary>
        /// Makes a saved object's data go away on purpose and stops tracking it so it can't write itself back.
        /// An object placed in a scene gets a gone marker; a plain class or spawned copy loses its data.
        /// With no game profile open, an object whose data lives in one logs one error and nothing changes.
        /// </summary>
        public void Forget(object savedObject)
        {
            List<SavedObjectEntry> entries = registry.AllEntries.Where(entry => BelongsTo(entry, savedObject)).ToList();
            bool needsGameProfile = entries.Any(entry => entry.Location != SaveLocation.Global);
            bool isGameProfileOpen = profileManager.CurrentGameProfileId != null;
            if (needsGameProfile && !isGameProfileOpen)
            {
                Debug.LogError($"[SaveSystem] Forget({SavedObjectEntry.Describe(savedObject)}) did nothing: its data lives in a game profile, and no game profile is open, so there is nowhere to forget it from. Open one first, for example SaveActions.GameProfiles.Get(1).Open(), then call Forget.");
                return;
            }

            foreach (SavedObjectEntry entry in entries)
            {
                StopTracking(entry);
                if (!addresses.TryGetAddress(entry, out SaveFileAddress address))
                {
                    continue;
                }
                if (entry.IsSceneObject)
                {
                    loadedData.MarkGone(address, entry.SaveId, entry.EntryName);
                }
                else
                {
                    loadedData.Forget(address, entry.SaveId);
                }
            }
        }

        /// <summary>Captures every tracked object, or only those the filter accepts, into the loaded data.</summary>
        public void CaptureAll(Func<SavedObjectEntry, bool> filter = null)
        {
            foreach (SavedObjectEntry entry in registry.AllEntries.ToList())
            {
                if (filter == null || filter(entry))
                {
                    Capture(entry);
                }
            }
        }

        /// <summary>Restores every tracked object, or only those the filter accepts, from the loaded data.</summary>
        public void RestoreAll(Func<SavedObjectEntry, bool> filter = null)
        {
            foreach (SavedObjectEntry entry in registry.AllEntries.ToList())
            {
                bool isStillTracked = registry.Contains(entry);
                if (isStillTracked && (filter == null || filter(entry)))
                {
                    TryRestore(entry);
                }
            }
        }

        /// <summary>
        /// A game profile opened: every GameProfile and Scene object is restored from it and belongs to it from now on.
        /// An object that stayed alive from another game profile, and has no data in this one, gets its new-game values
        /// back, so a new game starts fresh and is saved into this game profile. Global objects never change here.
        /// </summary>
        public void RestoreForOpenedGameProfile(string gameProfileId)
        {
            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => entry.Location != SaveLocation.Global).ToList())
            {
                // An object that turned itself off because of a gone marker has already left the registry.
                if (!registry.Contains(entry))
                {
                    continue;
                }

                bool cameFromAnotherGameProfile = entry.GameProfileId != null && entry.GameProfileId != gameProfileId;
                bool restored = TryRestore(entry);
                bool needsNewGameValues = !restored && cameFromAnotherGameProfile && registry.Contains(entry);
                if (needsNewGameValues)
                {
                    loadedData.RestoreNewGameValues(entry);
                }
                entry.GameProfileId = gameProfileId;
            }
        }

        public void Clear()
        {
            registry.Clear();
            AutomaticSaveIds.Clear();
        }

        bool TryRestore(SavedObjectEntry entry)
        {
            if (!addresses.TryGetAddress(entry, out SaveFileAddress address))
            {
                return false;
            }

            // A forgotten scene object turns itself off instead of restoring. It is no longer tracked, so nothing
            // captures it again and the marker stays.
            bool isForgottenSceneObject = entry.IsSceneObject && loadedData.HasGoneMarker(address, entry.SaveId);
            if (isForgottenSceneObject)
            {
                StopTracking(entry);
                entry.TurnOffSceneObject();
                return false;
            }
            return loadedData.TryRestore(entry, address);
        }

        void Capture(SavedObjectEntry entry)
        {
            bool belongsToOpenGameProfile = entry.Location == SaveLocation.Global || entry.GameProfileId == profileManager.CurrentGameProfileId;
            if (belongsToOpenGameProfile && addresses.TryGetAddress(entry, out SaveFileAddress address))
            {
                loadedData.Capture(entry, address);
            }
        }

        void StopTracking(SavedObjectEntry entry)
        {
            if (registry.Contains(entry))
            {
                registry.RemoveEntry(entry.SaveFileName, entry.SaveId);
            }
            AutomaticSaveIds.Release(entry.SavedObject);
        }

        static bool BelongsTo(SavedObjectEntry entry, object savedObject)
        {
            bool isOwner = ReferenceEquals(entry.Owner, savedObject);
            bool isSavedObject = ReferenceEquals(entry.SavedObject, savedObject);
            if (isOwner || isSavedObject)
            {
                return true;
            }

            // Forget(gameObject) covers every saved script on it, through its Savable Bridge.
            GameObject gameObject = savedObject as GameObject;
            Component ownerComponent = entry.Owner as Component;
            bool isOnThatGameObject = gameObject != null && ownerComponent != null && ownerComponent.gameObject == gameObject;
            return isOnThatGameObject;
        }
    }
}
