using System;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.SaveSlots
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 640, 700));
            bool isAnyGameProfileOpen = SaveActions.GameProfiles.Current != null;
            if (isAnyGameProfileOpen)
            {
                GUILayout.Label($"Points in the open game profile: {score.Points}");
                if (GUILayout.Button("Add 100 points"))
                {
                    AddPoints();
                }
                if (GUILayout.Button("Save"))
                {
                    Save();
                }
                if (GUILayout.Button("Back to the main menu (save and close)"))
                {
                    BackToMainMenu();
                }
            }
            else
            {
                DrawMainMenu();
            }

            GUILayout.Space(10);
            GUILayout.Label($"Game profiles, by slot number ({SaveActions.GameProfiles.Count} in total)");
            foreach (GameProfile profile in SaveActions.GameProfiles.All)
            {
                DrawSlot(profile);
            }
            if (GUILayout.Button("Create game profile"))
            {
                CreateGameProfile();
            }
            GUILayout.EndArea();
        }

        void DrawMainMenu()
        {
            GUILayout.Label("Main menu: no game profile is open.");
            GameProfile lastOpened = SaveActions.GameProfiles.LastOpened;
            GUI.enabled = lastOpened != null;
            string continueLabel = lastOpened == null ? "Continue (no saves yet)" : $"Continue {lastOpened.DisplayName}";
            if (GUILayout.Button(continueLabel))
            {
                Continue();
            }
            GUI.enabled = true;
        }

        void DrawSlot(GameProfile profile)
        {
            string openMarker = profile.IsOpen ? "> " : "";
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{openMarker}Slot {profile.SlotNumber}: {profile.DisplayName}   last saved: {FormatLastSaved(profile.LastSaved)}   playtime: {FormatPlaytime(profile.Playtime)}");
            if (!profile.IsOpen && GUILayout.Button("Open", GUILayout.Width(60)))
            {
                OpenSlot(profile.SlotNumber);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                DeleteSlot(profile.SlotNumber);
            }
            GUILayout.EndHorizontal();
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
