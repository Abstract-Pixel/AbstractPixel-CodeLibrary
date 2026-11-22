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
            // Left: one value per save file, each with the one button that changes it. Right: what to do with them.
            SampleGui.BeginPanel(20, 20, 440, SceneManager.GetActiveScene().name, $"Game profile: {OpenGameProfileName()} · Scene file loaded {SceneFileLoadCount}×");

            SampleGui.Section("Settings file (Global)");
            SampleGui.BeginRow();
            SampleGui.Value("Volume: ", volume.Volume.ToString());
            if (SampleGui.SmallButton("Volume up", 160))
            {
                volume.VolumeUp();
            }
            SampleGui.EndRow();

            SampleGui.Section("GameProfile file");
            SampleGui.BeginRow();
            SampleGui.Value("Coins: ", coins.Coins.ToString());
            if (SampleGui.SmallButton("Add 5 coins", 160))
            {
                coins.Add(5);
            }
            SampleGui.EndRow();

            SampleGui.Section("Scene file");
            foreach (Chest chest in chests)
            {
                SampleGui.BeginRow();
                SampleGui.Value($"{chest.name}: ", chest.IsOpen ? "open" : "closed");
                if (!chest.IsOpen && SampleGui.SmallButton("Open", 160))
                {
                    chest.Open();
                }
                SampleGui.EndRow();
            }
            SampleGui.EndPanel();

            SampleGui.BeginPanel(480, 20, 360, "Save and switch");
            if (SampleGui.Button("Save everything"))
            {
                SaveEverything();
            }
            if (SampleGui.Button("Save only the settings"))
            {
                SaveOnlyTheSettings();
            }
            if (SampleGui.Button("Undo unsaved coins"))
            {
                UndoUnsavedCoins();
            }
            SampleGui.Section("Switch");
            if (SampleGui.Button($"Go to {otherSceneName}"))
            {
                GoToOtherScene();
            }
            if (SampleGui.Button("New game profile"))
            {
                NewGameProfile();
            }
            if (SampleGui.Button("Next game profile"))
            {
                NextGameProfile();
            }
            SampleGui.EndPanel();
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
