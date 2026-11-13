using System;
using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Remembers which save files have loaded in this session, so a script that subscribes to a load event late is
    /// told right away. It lives in the session, not in a static, so nothing outlives play mode.
    /// </summary>
    internal sealed class LoadEventLog
    {
        // One loaded save file. Scene save files load once per scene, so the scene name is part of it.
        readonly struct LoggedLoad
        {
            public readonly Type SaveFileClass;
            public readonly SaveLocation Location;
            public readonly string SceneName;

            public LoggedLoad(Type saveFileClass, SaveLocation location, string sceneName)
            {
                SaveFileClass = saveFileClass;
                Location = location;
                SceneName = sceneName;
            }
        }

        readonly List<LoggedLoad> loads = new List<LoggedLoad>();

        /// <summary>True while Global data and the open game profile are restored.</summary>
        public bool HasLoadedAll { get; private set; }

        /// <summary>Records one loaded save file and raises OnLoadCompleted for it.</summary>
        public void RecordLoad(Type saveFileClass, SaveLocation location, string sceneName)
        {
            if (saveFileClass == null)
            {
                return;
            }

            bool isAlreadyLogged = loads.Exists(logged => logged.SaveFileClass == saveFileClass && logged.SceneName == sceneName);
            if (!isAlreadyLogged)
            {
                loads.Add(new LoggedLoad(saveFileClass, location, sceneName));
            }
            SaveEventBus.RaiseLoadCompleted(saveFileClass);
        }

        /// <summary>Records that everything is restored and raises OnLoadAllCompleted.</summary>
        public void RecordLoadAll()
        {
            HasLoadedAll = true;
            SaveEventBus.RaiseLoadAllCompleted();
        }

        /// <summary>Calls the handler once for every save file loaded so far, in the order they loaded.</summary>
        public void Replay(Action<Type> handler)
        {
            foreach (LoggedLoad logged in loads.ToArray())
            {
                SaveEventBus.InvokeSafely(handler, nameof(SaveEventBus.OnLoadCompleted), () => handler(logged.SaveFileClass));
            }
        }

        /// <summary>The game profile closed or changed: only Global save files stay loaded.</summary>
        public void ForgetGameProfileLoads()
        {
            loads.RemoveAll(logged => logged.Location != SaveLocation.Global);
            HasLoadedAll = false;
        }

        public void ForgetSceneLoads(string sceneName)
        {
            loads.RemoveAll(logged => logged.SceneName == sceneName);
        }
    }
}
