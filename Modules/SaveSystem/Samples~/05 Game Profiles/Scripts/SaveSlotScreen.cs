using System;
using System.Globalization;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.GameProfiles
{
    // A simple save slot screen: create, open, list and delete game profiles.
    public class SaveSlotScreen : MonoBehaviour
    {
        [SerializeField] Score score;

        void OnGUI()
        {
            SaveProfileManager gameProfiles = SaveActions.GameProfiles;
            if (gameProfiles == null)
            {
                GUILayout.Label("The save system isn't running. Check that a SaveSystemConfig exists in a Resources folder.");
                return;
            }

            GUILayout.BeginArea(new Rect(20, 20, 560, 700));
            GUILayout.Label($"Points in the open game profile: {score.Points}");
            if (GUILayout.Button("Add 100 points"))
            {
                score.Add(100);
            }
            if (GUILayout.Button("Save"))
            {
                SaveActions.SaveAll();
            }

            GUILayout.Space(10);
            GUILayout.Label("Game profiles");
            foreach (GameProfileManifest manifest in gameProfiles.GetAll())
            {
                DrawSlot(gameProfiles, manifest);
            }
            if (GUILayout.Button("Create game profile"))
            {
                gameProfiles.CreateNew($"Game Profile {gameProfiles.GetAll().Count + 1}");
            }
            GUILayout.EndArea();
        }

        void DrawSlot(SaveProfileManager gameProfiles, GameProfileManifest manifest)
        {
            bool isOpen = manifest.GameProfileId == gameProfiles.CurrentGameProfileId;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{(isOpen ? "> " : "")}{manifest.DisplayName}   last saved: {FormatTime(manifest.LastSavedUtc)}   playtime: {FormatPlaytime(manifest.PlaytimeSeconds)}");
            if (!isOpen && GUILayout.Button("Open", GUILayout.Width(60)))
            {
                gameProfiles.Open(manifest.GameProfileId);
                // Reloading resets the scene to new-game values, then restores the opened game profile's data.
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                gameProfiles.Delete(manifest.GameProfileId);
                if (isOpen)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            GUILayout.EndHorizontal();
        }

        static string FormatTime(string utcTime)
        {
            if (string.IsNullOrEmpty(utcTime))
            {
                return "never";
            }
            // The manifest stores UTC; show it in the player's own time.
            return DateTime.Parse(utcTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime().ToString("g");
        }

        static string FormatPlaytime(double seconds)
        {
            TimeSpan playtime = TimeSpan.FromSeconds(seconds);
            return $"{(int)playtime.TotalMinutes}:{playtime.Seconds:00}";
        }
    }
}
