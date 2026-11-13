using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Events the save system raises. <see cref="SaveActions"/> exposes the same events.</summary>
    public static class SaveEventBus
    {
        static Action<GameProfile> gameProfileOpened;
        static GameProfile openGameProfile;

        /// <summary>
        /// Raised with the game profile when it opens. Subscribing while a game profile is already open calls
        /// the handler right away, because the save system may open one before the first scene's Awake runs.
        /// </summary>
        public static event Action<GameProfile> GameProfileOpened
        {
            add
            {
                gameProfileOpened += value;
                if (openGameProfile != null)
                {
                    value(openGameProfile);
                }
            }
            remove => gameProfileOpened -= value;
        }

        /// <summary>Raised after SaveAll or Save finishes writing.</summary>
        public static event Action Saved;

        internal static void RaiseGameProfileOpened(GameProfile profile)
        {
            openGameProfile = profile;
            gameProfileOpened?.Invoke(profile);
        }

        internal static void RaiseGameProfileClosed()
        {
            openGameProfile = null;
        }

        internal static void RaiseSaved()
        {
            Saved?.Invoke();
        }

        // Called when play mode starts and stops, so no subscriber from an old session lingers.
        internal static void Clear()
        {
            gameProfileOpened = null;
            openGameProfile = null;
            Saved = null;
        }
    }
}
