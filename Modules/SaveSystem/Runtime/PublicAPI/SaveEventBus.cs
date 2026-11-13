using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Load and save events. Load events and OnGameProfileOpened are sticky: a script that subscribes after the event
    /// already happened is called right away, so it never matters whether it subscribed early or late.
    /// <code>
    /// SaveEventBus.OnLoadCompleted += file => { if (file == typeof(GameSaveFiles.Ghosts)) ShowGhosts(); };
    /// </code>
    /// </summary>
    public static class SaveEventBus
    {
        static Action<Type> loadCompleted;
        static Action loadAllCompleted;
        static Action<GameProfile> gameProfileOpened;

        // The Action<Type> each AddLoadListener<TFile> call actually subscribed with, so RemoveLoadListener<TFile>
        // can unsubscribe that exact one. Keyed by both the handler and TFile, so the same handler can listen to
        // more than one save file.
        static readonly Dictionary<(Delegate Handler, Type FileType), Action<Type>> typedLoadListeners = new Dictionary<(Delegate, Type), Action<Type>>();

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
                    InvokeEach(value, nameof(OnLoadAllCompleted));
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
                    InvokeEach(value, openGameProfile, nameof(OnGameProfileOpened));
                }
            }
            remove => gameProfileOpened -= value;
        }

        /// <summary>
        /// Calls handler every time this save file's load event fires: once per scene for a Scene file, otherwise
        /// once. Sticky: subscribing after it already loaded calls handler right away.
        /// Example: <c>SaveEventBus.AddLoadListener&lt;GameSaveFiles.Ghosts&gt;(ShowGhosts);</c>
        /// </summary>
        public static void AddLoadListener<TFile>(Action handler) where TFile : ISaveFile
        {
            (Delegate, Type) key = (handler, typeof(TFile));
            if (typedLoadListeners.ContainsKey(key))
            {
                return;
            }
            Action<Type> listener = loadedFile =>
            {
                if (loadedFile == typeof(TFile))
                {
                    handler();
                }
            };
            typedLoadListeners[key] = listener;
            OnLoadCompleted += listener;
        }

        /// <summary>Stops handler from hearing about this save file's load event.</summary>
        public static void RemoveLoadListener<TFile>(Action handler) where TFile : ISaveFile
        {
            (Delegate, Type) key = (handler, typeof(TFile));
            if (typedLoadListeners.TryGetValue(key, out Action<Type> listener))
            {
                OnLoadCompleted -= listener;
                typedLoadListeners.Remove(key);
            }
        }

        /// <summary>One save file was written. Save&lt;TFile&gt;() raises it for that file; SaveAll raises it for each file it wrote.</summary>
        public static event Action<Type> OnSaveCompleted;

        /// <summary>SaveAll finished writing every changed save file.</summary>
        public static event Action OnSaveAllCompleted;

        /// <summary>
        /// A save file could not be written. Gives the save file class and a reason to show the player, such as
        /// "Saved failed". The previous good save on disk is untouched; check the console for exactly what went wrong.
        /// </summary>
        public static event Action<Type, string> OnSaveFailed;

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

        internal static void RaiseSaveFailed(Type saveFileClass, string reason)
        {
            if (OnSaveFailed == null)
            {
                return;
            }
            foreach (Delegate handler in OnSaveFailed.GetInvocationList())
            {
                InvokeSafely(handler, nameof(OnSaveFailed), () => ((Action<Type, string>)handler)(saveFileClass, reason));
            }
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
            OnSaveFailed = null;
            typedLoadListeners.Clear();
        }

        // Each handler runs on its own, so one that throws can't stop the others or the save system.
        static void InvokeEach<T>(Action<T> handlers, T argument, string eventName)
        {
            InvokeEach(handlers, eventName, handler => ((Action<T>)handler)(argument));
        }

        static void InvokeEach(Action handlers, string eventName)
        {
            InvokeEach(handlers, eventName, handler => ((Action)handler)());
        }

        static void InvokeEach(Delegate handlers, string eventName, Action<Delegate> call)
        {
            if (handlers == null)
            {
                return;
            }
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                InvokeSafely(handler, eventName, () => call(handler));
            }
        }

        internal static void InvokeSafely(Delegate handler, string eventName, Action call)
        {
            try
            {
                call();
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
