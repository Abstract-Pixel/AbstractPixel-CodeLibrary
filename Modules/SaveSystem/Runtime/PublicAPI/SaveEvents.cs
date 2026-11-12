using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Events the save system raises. <see cref="SaveSystem"/> exposes the same events.</summary>
    public static class SaveEvents
    {
        static Action<string> gameProfileOpened;
        static string openGameProfileId;

        /// <summary>
        /// Raised with the game profile's ID when it opens. Subscribing while a game profile is already open calls
        /// the handler right away, because the save system may open one before the first scene's Awake runs.
        /// </summary>
        public static event Action<string> GameProfileOpened
        {
            add
            {
                gameProfileOpened += value;
                if (openGameProfileId != null)
                {
                    value(openGameProfileId);
                }
            }
            remove => gameProfileOpened -= value;
        }

        /// <summary>Raised after SaveAll or Save finishes writing.</summary>
        public static event Action Saved;

        internal static void RaiseGameProfileOpened(string gameProfileId)
        {
            openGameProfileId = gameProfileId;
            gameProfileOpened?.Invoke(gameProfileId);
        }

        internal static void RaiseGameProfileClosed()
        {
            openGameProfileId = null;
        }

        internal static void RaiseSaved()
        {
            Saved?.Invoke();
        }

        // Called when play mode starts and stops, so no subscriber from an old session lingers.
        internal static void Clear()
        {
            gameProfileOpened = null;
            openGameProfileId = null;
            Saved = null;
        }
    }
}
