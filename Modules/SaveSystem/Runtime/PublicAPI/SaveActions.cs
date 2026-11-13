using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The one class game code calls to save and load. Register a saved object once, and it gets its data back
    /// as soon as both the object and its data exist, whichever comes second.
    /// Until the save system has started (it needs a save system config), every call is a safe no-op.
    /// </summary>
    public static class SaveActions
    {
        static SaveSession session;

        /// <summary>
        /// Every game profile, plus CreateNew, Current and Get(slot). Never null: without a save system config it is
        /// an empty list whose methods log one clear error.
        /// </summary>
        public static GameProfileList GameProfiles
        {
            get
            {
                if (session == null)
                {
                    return GameProfileList.Empty;
                }
                return session.GameProfiles;
            }
        }

        internal static SaveSession Session => session;

        /// <summary>
        /// Registers a plain class. Its save ID is its class name; later live instances of the same class get
        /// "Inventory 2", "Inventory 3" in the order they register. If its data is already loaded, it is restored
        /// inside this call, so a constructor should call Register last, after setting its new-game values.
        /// If the order instances register in can change between runs, numbered instances swap data: give them a custom save ID.
        /// Example: <c>SaveActions.Register(this);</c>
        /// </summary>
        public static void Register<T>(ISavable<T> savedObject) => session?.Register(savedObject, null);

        /// <summary>
        /// Registers a plain class under a custom save ID, which always wins over the automatic one.
        /// Example: <c>SaveActions.Register(this, "player-inventory");</c>
        /// </summary>
        public static void Register<T>(ISavable<T> savedObject, string saveId) => session?.Register(savedObject, saveId);

        /// <summary>Captures the object's current data, then stops tracking it. Its data is written on the next save.</summary>
        public static void Unregister(object savedObject) => session?.Unregister(savedObject);

        /// <summary>Captures every registered object, then writes every save file that changed.</summary>
        public static void SaveAll() => session?.SaveAll();

        /// <summary>Like SaveAll, for one save file only. Example: <c>SaveActions.Save&lt;GameSaveFiles.Ghosts&gt;();</c></summary>
        public static void Save<TFile>() where TFile : ISaveFile => session?.Save(typeof(TFile));

        /// <summary>Saves the save file picked in an inspector, through a <see cref="SaveFileReference"/>.</summary>
        public static void Save(SaveFileReference saveFile) => session?.Save(saveFile);

        /// <summary>
        /// Reads one save file from disk again and restores every saved object in it, throwing away changes that
        /// weren't saved. Use it after another tool wrote the file directly. Example: <c>SaveActions.Reload&lt;SaveFiles.Input&gt;();</c>
        /// </summary>
        public static void Reload<TFile>() where TFile : ISaveFile => session?.Reload(typeof(TFile));

        /// <summary>
        /// Removes one saved object's data on purpose, for example a coin that was picked up, so it doesn't come back.
        /// Pass the saved object itself: a plain class, a saved script or its GameObject. Example: <c>SaveActions.Forget(coin);</c>
        /// </summary>
        public static void Forget(object savedObject) => session?.Forget(savedObject);

        internal static void Start(SaveSession newSession)
        {
            Stop();
            session = newSession;
        }

        // Lets go of the session and every event handler, so nothing from this play session outlives it.
        internal static void Stop()
        {
            session?.Dispose();
            session = null;
            SaveEventBus.Clear();
        }
    }
}
