using System;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.SaveSlots
{
    // A simple save slot screen: create, open, list and delete game profiles by slot number.
    // Each button calls a public method, so tests and tools can press it from code too.
    public class SaveSlotScreen : MonoBehaviour
    {
        [SerializeField] Score score;

        public void AddPoints()
        {
            score.Add(100);
        }

        public void Save()
        {
            SaveActions.SaveAll();
        }

        // CreateNew() names the game profile from the config: "Game Profile 1", "Game Profile 2", and so on.
        public GameProfile CreateGameProfile()
        {
            return SaveActions.GameProfiles.CreateNew();
        }

        public void OpenSlot(int slotNumber)
        {
            GameProfile profile = SaveActions.GameProfiles.Get(slotNumber);
            if (profile == null)
            {
                return;
            }
            profile.Open();
            // Reloading resets the scene to new-game values, then restores the opened game profile's data.
            SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
        }

        // A main menu's Continue button: open the game profile the player had open last time.
        // LastOpened is null on a first launch, so the button is greyed out then (see OnGUI).
        public void Continue()
        {
            GameProfile lastOpened = SaveActions.GameProfiles.LastOpened;
            if (lastOpened == null)
            {
                return;
            }
            lastOpened.Open();
            SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
        }

        // Back to the main menu: nothing is open, so Continue has something to do.
        public void BackToMainMenu()
        {
            SaveActions.SaveAll();
            SaveActions.GameProfiles.CloseCurrent();
        }

        public void DeleteSlot(int slotNumber)
        {
            GameProfile profile = SaveActions.GameProfiles.Get(slotNumber);
            if (profile == null)
            {
                return;
            }
            bool wasOpen = profile.IsOpen;
            profile.Delete();
            if (wasOpen)
            {
                SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
            }
        }

        void OnGUI()
        {
            bool isAnyGameProfileOpen = SaveActions.GameProfiles.Current != null;
            if (isAnyGameProfileOpen)
            {
                SampleGui.BeginPanel(20, 20, 600, "Playing", "Each game profile keeps its own points.");
                SampleGui.Value("Points: ", score.Points.ToString());
                SampleGui.BeginRow();
                if (SampleGui.Button("Add 100 points"))
                {
                    AddPoints();
                }
                if (SampleGui.Button("Save"))
                {
                    Save();
                }
                SampleGui.EndRow();
                if (SampleGui.Button("Back to the main menu"))
                {
                    BackToMainMenu();
                }
            }
            else
            {
                DrawMainMenu();
            }

            SampleGui.Section($"Game profiles ({SaveActions.GameProfiles.Count})");
            foreach (GameProfile profile in SaveActions.GameProfiles.All)
            {
                DrawSlot(profile);
            }
            if (SampleGui.Button("Create game profile"))
            {
                CreateGameProfile();
            }
            SampleGui.EndPanel();
        }

        void DrawMainMenu()
        {
            SampleGui.BeginPanel(20, 20, 600, "Main menu", "No game profile is open. Continue opens the last one.");
            GameProfile lastOpened = SaveActions.GameProfiles.LastOpened;
            GUI.enabled = lastOpened != null;
            string continueLabel = lastOpened == null ? "Continue (no saves yet)" : $"Continue {lastOpened.DisplayName}";
            if (SampleGui.Button(continueLabel))
            {
                Continue();
            }
            GUI.enabled = true;
        }

        // Two lines per slot: its name, then when it was last saved and how long it was played.
        void DrawSlot(GameProfile profile)
        {
            string openMarker = profile.IsOpen ? "  (open)" : "";
            SampleGui.BeginRow();
            GUILayout.BeginVertical();
            SampleGui.Value($"Slot {profile.SlotNumber}: ", profile.DisplayName + openMarker);
            SampleGui.Hint($"Last saved {FormatLastSaved(profile.LastSaved)} · played {FormatPlaytime(profile.Playtime)}");
            GUILayout.EndVertical();
            if (!profile.IsOpen && SampleGui.SmallButton("Open", 100))
            {
                OpenSlot(profile.SlotNumber);
            }
            if (SampleGui.SmallButton("Delete", 100))
            {
                DeleteSlot(profile.SlotNumber);
            }
            SampleGui.EndRow();
        }

        static string FormatLastSaved(DateTime? lastSaved)
        {
            if (lastSaved == null)
            {
                return "never";
            }
            return lastSaved.Value.ToString("g");
        }

        static string FormatPlaytime(TimeSpan playtime)
        {
            return $"{(int)playtime.TotalMinutes}:{playtime.Seconds:00}";
        }
    }
}
