using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>One registered saved object: who owns it, where its data lives, and how to capture and restore it.</summary>
    internal sealed class SavedObjectEntry
    {
        /// <summary>The object that registered. Unregistering with this object removes the entry.</summary>
        public object Owner { get; }
        public string SaveId { get; }

        /// <summary>The save file's name, such as "Game". The config says which save location it lives in.</summary>
        public string SaveFileName { get; }
        public SaveLocation Location { get; }

        /// <summary>The scene the object registered in. Only Scene save files use it.</summary>
        public string SceneName { get; }

        /// <summary>A readable name written next to the data, such as "Forest/Chest (2)/ChestLid". Matching uses only the save ID.</summary>
        public string EntryName { get; }

        public Type DataType { get; }
        public Func<object> Capture { get; }
        public Action<object> Restore { get; }

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
