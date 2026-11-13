using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>One registered saved object: who owns it, where its data lives, and how to capture and restore it.</summary>
    internal sealed class SavedObjectEntry
    {
        /// <summary>The object that registered. Unregistering with this object removes the entry.</summary>
        public object Owner { get; }

        /// <summary>
        /// The object whose data this is. For a plain class it is the owner; for a scene object it is the saved script,
        /// while the owner is its Savable Bridge. Forget looks for either.
        /// </summary>
        public object SavedObject
        {
            get
            {
                if (savedObject != null)
                {
                    return savedObject;
                }
                return Owner;
            }
            set
            {
                savedObject = value;
            }
        }

        object savedObject;
        public string SaveId { get; }

        /// <summary>The save file's name, such as "Game". The config says which save location it lives in.</summary>
        public string SaveFileName { get; }
        public SaveLocation Location { get; }

        /// <summary>The scene the object registered in. Only Scene save files use it.</summary>
        public string SceneName { get; }

        /// <summary>A readable name written next to the data, such as "Forest/Chest (2)/ChestLid". Matching uses only the save ID.</summary>
        public string EntryName { get; }

        /// <summary>
        /// The game profile this entry's data belongs to, or null if none was open when it registered.
        /// An entry only ever writes into its own game profile, so one playthrough can't leak into another.
        /// </summary>
        public string GameProfileId { get; set; }

        public Type DataType { get; }
        public Func<object> Capture { get; }
        public Action<object> Restore { get; }

        /// <summary>
        /// Turns off an object placed in a scene when its save file holds a gone marker. Null for plain classes and
        /// spawned copies, whose data Forget removes instead.
        /// </summary>
        public Action TurnOffSceneObject { get; set; }

        /// <summary>An object placed in a scene: Forget writes a gone marker for it, so it stays gone after a reload.</summary>
        public bool IsSceneObject => TurnOffSceneObject != null;

        public SavedObjectEntry(object owner, string saveId, string saveFileName, SaveLocation location, string sceneName,
            string entryName, Type dataType, Func<object> capture, Action<object> restore)
        {
            Owner = owner;
            SaveId = saveId;
            SaveFileName = saveFileName;
            Location = location;
            SceneName = sceneName;
            EntryName = entryName;
            DataType = dataType;
            Capture = capture;
            Restore = restore;
        }

        /// <summary>Builds an entry for an <see cref="ISavable{T}"/> without reflection.</summary>
        public static SavedObjectEntry Create<T>(ISavable<T> savable, string saveId, string saveFileName, SaveLocation location,
            string sceneName, string entryName)
        {
            return new SavedObjectEntry(savable, saveId, saveFileName, location, sceneName, entryName, typeof(T),
                () => savable.CaptureData(),
                data => savable.RestoreData((T)data));
        }
    }
}
