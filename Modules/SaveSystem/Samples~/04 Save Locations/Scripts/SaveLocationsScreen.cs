using System.Collections.Generic;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.SaveLocations
{
    // Shows one value from each save location, and buttons to change scenes and game profiles.
    // Each button calls a public method, so tests and tools can press it from code too.
    public class SaveLocationsScreen : MonoBehaviour
    {
        [SerializeField] VolumeSetting volume;
        [SerializeField] PlayerCoins coins;
        [SerializeField] Chest[] chests;
        [SerializeField] string otherSceneName;

        public void GoToOtherScene()
        {
            SceneManager.LoadScene(otherSceneName);
        }

        public void Save()
        {
            SaveActions.SaveAll();
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
            GUILayout.BeginArea(new Rect(20, 20, 420, 600));
            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}   Game profile: {OpenGameProfileName()}");

            GUILayout.Label($"Global - volume: {volume.Volume}");
            if (GUILayout.Button("Volume up"))
            {
                volume.VolumeUp();
            }

            GUILayout.Label($"Game profile - coins: {coins.Coins}");
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

            if (GUILayout.Button($"Go to {otherSceneName}"))
            {
                GoToOtherScene();
            }
            if (GUILayout.Button("Save"))
            {
                Save();
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
