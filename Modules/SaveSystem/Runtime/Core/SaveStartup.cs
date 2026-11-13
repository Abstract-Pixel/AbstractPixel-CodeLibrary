using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Starts the save system before the first scene loads, so nothing needs to be placed in a scene.
    /// It also resets everything when play mode starts and stops, which keeps play mode working
    /// with domain reload turned off (and with Unity 7's code reload).
    /// </summary>
    internal static class SaveStartup
    {
        const string ConfigResourceName = SaveSystemConfigName.ResourceName;

        static bool saveOnQuit;
        static bool saveOnFocusLoss;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetWhenPlayModeStarts()
        {
            Stop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void StartBeforeTheFirstScene()
        {
            StartFromResources();
        }

        /// <summary>Starts the save system with the config in Resources and real save files on disk.</summary>
        internal static void StartFromResources()
        {
            SaveSystemConfigSO config = Resources.Load<SaveSystemConfigSO>(ConfigResourceName);
            IDataStorageService storage = config == null ? null : new DiskStorageService(config);
            Start(config, storage, new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        /// <summary>Builds the save system's parts and starts it. With no config it logs one error and stays inactive.</summary>
        internal static bool Start(SaveSystemConfigSO config, IDataStorageService storage, ISceneInfo sceneInfo, Func<double> unscaledTime)
        {
            Stop();
            if (config == null)
            {
                Debug.LogError($"[SaveSystem] No save system config named '{ConfigResourceName}' was found in a Resources folder, so saving is off and every save system call does nothing. Create one with {SaveSystemConfigName.CreateConfigMenuPath}.");
                return false;
            }

            SaveSession session = new SaveSession(config, storage, new JsonSerializer(), sceneInfo, unscaledTime);
            SaveActions.Start(session);
            session.ReadGlobalSaveFiles();
            ApplyStartupPolicy(config.StartupPolicy, session.ProfileManager);

            saveOnQuit = config.SaveOnQuit;
            saveOnFocusLoss = config.SaveOnFocusLoss;
            // Static Unity events, so no MonoBehaviour is needed. Stop removes them again.
            Application.quitting += HandleQuitting;
            Application.focusChanged += HandleFocusChanged;
            return true;
        }

        /// <summary>Stops the save system and lets go of every object and handler, so nothing outlives play mode.</summary>
        internal static void Stop()
        {
            Application.quitting -= HandleQuitting;
            Application.focusChanged -= HandleFocusChanged;
            SaveActions.Stop();
        }

        internal static void HandleFocusChanged(bool hasFocus)
        {
            if (!hasFocus && saveOnFocusLoss)
            {
                SaveActions.SaveAll();
            }
        }

        // In the Editor this also runs when play mode stops.
        static void HandleQuitting()
        {
            if (saveOnQuit)
            {
                SaveActions.SaveAll();
            }
            Stop();
        }

        static void ApplyStartupPolicy(SaveStartupPolicy policy, SaveProfileManager gameProfiles)
        {
            // Manual waits for the game to open a game profile, for example from a save slot screen.
            if (policy != SaveStartupPolicy.AutoSetUp)
            {
                return;
            }

            if (gameProfiles.TryGetLastOpened(out string lastOpenedId))
            {
                gameProfiles.Open(lastOpenedId);
                return;
            }
            gameProfiles.Open(gameProfiles.CreateNew());
        }
    }
}
