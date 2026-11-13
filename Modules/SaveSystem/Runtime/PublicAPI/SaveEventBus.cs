using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Load and save events. Load events and OnGameProfileOpened are sticky: a script that subscribes after the event
    /// already happened is called right away, so it never matters whether it subscribed early or late.
    /// <code>
    /// SaveEventBus.OnLoadCompleted += file => { if (file == typeof(SaveFiles.Ghosts)) ShowGhosts(); };
    /// </code>
    /// </summary>
    public static class SaveEventBus
    {
        static Action<Type> loadCompleted;
        static Action loadAllCompleted;
        static Action<GameProfile> gameProfileOpened;

        /// <summary>
        /// One save file was read and every registered object in it restored. Gives the save file class.
        /// A Scene save file raises it once per scene. Sticky.
        /// </summary>
        public static event Action<Type> OnLoadCompleted
        {
            add
            {
                loadCompleted += value;
                SaveActions.Session?.LoadEvents.Replay(value);
            }
            remove => loadCompleted -= value;
        }

        /// <summary>Global data and the open game profile are restored. Raised each time a game profile opens. Sticky.</summary>
        public static event Action OnLoadAllCompleted
        {
            add
            {
                loadAllCompleted += value;
                bool hasLoadedAll = SaveActions.Session != null && SaveActions.Session.LoadEvents.HasLoadedAll;
                if (hasLoadedAll)
                {
                    InvokeSafely(value, nameof(OnLoadAllCompleted));
                }
            }
            remove => loadAllCompleted -= value;
        }

        /// <summary>A game profile opened. Sticky: subscribing while one is open calls the handler right away.</summary>
        public static event Action<GameProfile> OnGameProfileOpened
        {
            add
            {
                gameProfileOpened += value;
                GameProfile openGameProfile = SaveActions.GameProfiles.Current;
                if (openGameProfile != null)
                {
                    InvokeSafely(value, openGameProfile, nameof(OnGameProfileOpened));
                }
            }
            remove => gameProfileOpened -= value;
        }

        /// <summary>One save file was written. Save&lt;TFile&gt;() raises it for that file; SaveAll raises it for each file it wrote.</summary>
        public static event Action<Type> OnSaveCompleted;

        /// <summary>SaveAll finished writing every changed save file.</summary>
        public static event Action OnSaveAllCompleted;

        internal static void RaiseLoadCompleted(Type saveFileClass)
        {
            InvokeEach(loadCompleted, saveFileClass, nameof(OnLoadCompleted));
        }

        internal static void RaiseLoadAllCompleted()
        {
            InvokeEach(loadAllCompleted, nameof(OnLoadAllCompleted));
        }

        internal static void RaiseGameProfileOpened(GameProfile profile)
        {
            InvokeEach(gameProfileOpened, profile, nameof(OnGameProfileOpened));
        }

        internal static void RaiseSaveCompleted(Type saveFileClass)
        {
            InvokeEach(OnSaveCompleted, saveFileClass, nameof(OnSaveCompleted));
        }

        internal static void RaiseSaveAllCompleted()
        {
            InvokeEach(OnSaveAllCompleted, nameof(OnSaveAllCompleted));
        }

        // Called when play mode starts and stops, so no subscriber from an old session lingers.
        internal static void Clear()
        {
            loadCompleted = null;
            loadAllCompleted = null;
            gameProfileOpened = null;
            OnSaveCompleted = null;
            OnSaveAllCompleted = null;
        }

        // Each handler runs on its own, so one that throws can't stop the others or the save system.
        static void InvokeEach<T>(Action<T> handlers, T argument, string eventName)
        {
            if (handlers == null)
            {
                return;
            }
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                InvokeSafely((Action<T>)handler, argument, eventName);
            }
        }

        static void InvokeEach(Action handlers, string eventName)
        {
            if (handlers == null)
            {
                return;
            }
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                InvokeSafely((Action)handler, eventName);
            }
        }

        internal static void InvokeSafely<T>(Action<T> handler, T argument, string eventName)
        {
            try
            {
                handler(argument);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] A handler of SaveEventBus.{eventName} in {DescribeHandler(handler)} threw: {exception.Message} The other handlers still ran. Fix the handler; the save system itself is fine.");
            }
        }

        static void InvokeSafely(Action handler, string eventName)
        {
            try
            {
                handler();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] A handler of SaveEventBus.{eventName} in {DescribeHandler(handler)} threw: {exception.Message} The other handlers still ran. Fix the handler; the save system itself is fine.");
            }
        }

        static string DescribeHandler(Delegate handler)
        {
            if (handler.Method.DeclaringType == null)
            {
                return handler.Method.Name;
            }
            return $"{handler.Method.DeclaringType.Name}.{handler.Method.Name}";
        }
    }
}
