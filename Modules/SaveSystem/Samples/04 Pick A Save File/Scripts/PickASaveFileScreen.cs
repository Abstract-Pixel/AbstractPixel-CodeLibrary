using System.Collections.Generic;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.PickASaveFile
{
    // Shows one value from each save file, and buttons to save one file, change scenes and change game profiles.
    // Each button calls a public method, so tests and tools can press it from code too.
    public class PickASaveFileScreen : MonoBehaviour
    {
        [SerializeField] VolumeSetting volume;
        [SerializeField] PlayerCoins coins;
        [SerializeField] Chest[] chests;
        [SerializeField] string otherSceneName;

        /// <summary>How many times this scene's Scene save file loaded, as the typed load listener heard it.</summary>
        public int SceneFileLoadCount { get; private set; }

        // A typed load listener is sticky: this screen starts after the Scene file loaded, and still hears about it.
        void OnEnable()
        {
            SaveEventBus.AddLoadListener<SaveFiles.Scene>(CountSceneFileLoad);
        }

        void OnDisable()
        {
            SaveEventBus.RemoveLoadListener<SaveFiles.Scene>(CountSceneFileLoad);
        }

        void CountSceneFileLoad()
        {
            SceneFileLoadCount++;
        }

        public void GoToOtherScene()
        {
            SampleScenes.Load(otherSceneName);   // in your game: SceneManager.LoadScene(otherSceneName)
        }

        public void SaveEverything()
        {
            SaveActions.SaveAll();
        }

        // Writes only the settings file; unsaved coins and chests stay unsaved.
        public void SaveOnlyTheSettings()
        {
            SaveActions.Save<SaveFiles.Settings>();
        }

        // Throws away unsaved changes to the coins by reading the GameProfile file back from disk.
        public void UndoUnsavedCoins()
        {
            SaveActions.Reload<SaveFiles.GameProfile>();
        }

        public void NewGameProfile()
        {
            OpenAndReload(SaveActions.GameProfiles.CreateNew());
        }

        public void NextGameProfile()
        {
            OpenAndReload(FindNextGameProfile());
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 460, 640));
            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}   Game profile: {OpenGameProfileName()}");
            GUILayout.Label($"This scene's Scene file loaded {SceneFileLoadCount} time(s)");

            GUILayout.Label($"Settings (Global) - volume: {volume.Volume}");
            if (GUILayout.Button("Volume up"))
            {
                volume.VolumeUp();
            }

            GUILayout.Label($"GameProfile - coins: {coins.Coins}");
            if (GUILayout.Button("Add 5 coins"))
            {
                coins.Add(5);
            }

            foreach (Chest chest in chests)
            {
                GUILayout.Label($"Scene - {chest.name}: {(chest.IsOpen ? "open" : "closed")}");
                if (!chest.IsOpen && GUILayout.Button($"Open {chest.name}"))
                {
                    chest.Open();
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Save everything"))
            {
                SaveEverything();
            }
            if (GUILayout.Button("Save only the settings"))
            {
                SaveOnlyTheSettings();
            }
            if (GUILayout.Button("Undo unsaved coins"))
            {
                UndoUnsavedCoins();
            }
            if (GUILayout.Button($"Go to {otherSceneName}"))
            {
                GoToOtherScene();
            }
            if (GUILayout.Button("New game profile"))
            {
                NewGameProfile();
            }
            if (GUILayout.Button("Next game profile"))
            {
                NextGameProfile();
            }
            GUILayout.EndArea();
        }

        // Reloading the scene resets every object to its new-game values, then restores the opened game profile's data.
        static void OpenAndReload(GameProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            profile.Open();
            SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
        }

        // The game profile after the open one, by slot number, going back to the first after the last.
        static GameProfile FindNextGameProfile()
        {
            IReadOnlyList<GameProfile> all = SaveActions.GameProfiles.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].IsOpen)
                {
                    return all[(i + 1) % all.Count];
                }
            }
            if (all.Count > 0)
            {
                return all[0];
            }
            return null;
        }

        static string OpenGameProfileName()
        {
            GameProfile current = SaveActions.GameProfiles.Current;
            if (current == null)
            {
                return "none";
            }
            return current.DisplayName;
        }
    }
}
