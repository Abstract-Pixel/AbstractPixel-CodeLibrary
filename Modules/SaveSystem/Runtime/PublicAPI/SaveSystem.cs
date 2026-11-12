using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The one class game code calls to save and load. Register a saved object once, and it gets its data back
    /// as soon as both the object and its data exist, whichever comes second.
    /// Until the save system has started (it needs a save system config), every call is a safe no-op.
    /// </summary>
    public static class SaveSystem
    {
        static SaveSession session;

        /// <summary>Creates, opens, lists and deletes game profiles. Null until the save system has started.</summary>
        public static SaveProfileManager GameProfiles => session?.GameProfiles;

        public static bool IsGameProfileOpen => session?.GameProfiles.CurrentGameProfileId != null;

        /// <summary>Raised when a game profile opens. Subscribing while one is open calls the handler right away.</summary>
        public static event Action<string> GameProfileOpened
        {
            add => SaveEvents.GameProfileOpened += value;
            remove => SaveEvents.GameProfileOpened -= value;
        }

        /// <summary>Raised after SaveAll or Save finishes writing.</summary>
        public static event Action Saved
        {
            add => SaveEvents.Saved += value;
            remove => SaveEvents.Saved -= value;
        }

        internal static SaveSession Session => session;

        /// <summary>
        /// Registers a saved object under a save ID. If its data is already loaded, it is restored inside this call,
        /// so a constructor should call Register last, after setting its new-game values.
        /// Example: <c>SaveSystem.Register(this, "player-inventory");</c>
        /// </summary>
        public static void Register<T>(ISavable<T> savedObject, string saveId) => session?.Register(savedObject, saveId);

        /// <summary>Captures the object's current data, then stops tracking it. Its data is written on the next save.</summary>
        public static void Unregister(object savedObject) => session?.Unregister(savedObject);

        /// <summary>Captures every registered object, then writes every save file that changed.</summary>
        public static void SaveAll() => session?.SaveAll();

        /// <summary>Like SaveAll, for one save file only, for example "Settings".</summary>
        public static void Save(string saveFileName) => session?.Save(saveFileName);

        /// <summary>Removes one saved object's data on purpose, for example a coin that was picked up.</summary>
        public static void Forget(string saveFileName, string saveId) => session?.Forget(saveFileName, saveId);

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
            SaveEvents.Clear();
        }
    }
}
