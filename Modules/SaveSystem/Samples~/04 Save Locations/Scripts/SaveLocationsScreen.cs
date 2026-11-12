using System.Collections.Generic;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.SaveLocations
{
    // Shows one value from each save location, and buttons to change scenes and game profiles.
    public class SaveLocationsScreen : MonoBehaviour
    {
        [SerializeField] VolumeSetting volume;
        [SerializeField] PlayerCoins coins;
        [SerializeField] Chest[] chests;
        [SerializeField] string otherSceneName;

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
                SceneManager.LoadScene(otherSceneName);
            }
            if (GUILayout.Button("Save"))
            {
                SaveSystem.SaveAll();
            }
            if (GUILayout.Button("New game profile"))
            {
                OpenAndReload(SaveSystem.GameProfiles.CreateNew($"Game Profile {SaveSystem.GameProfiles.GetAll().Count + 1}"));
            }
            if (GUILayout.Button("Next game profile"))
            {
                OpenAndReload(NextGameProfileId());
            }
            GUILayout.EndArea();
        }

        // Reloading the scene resets every object to its new-game values, then restores the opened game profile's data.
        static void OpenAndReload(string gameProfileId)
        {
            SaveSystem.GameProfiles.Open(gameProfileId);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        static string NextGameProfileId()
        {
            IReadOnlyList<GameProfileManifest> all = SaveSystem.GameProfiles.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].GameProfileId == SaveSystem.GameProfiles.CurrentGameProfileId)
                {
                    return all[(i + 1) % all.Count].GameProfileId;
                }
            }
            return all.Count > 0 ? all[0].GameProfileId : null;
        }

        static string OpenGameProfileName()
        {
            string openId = SaveSystem.GameProfiles?.CurrentGameProfileId;
            if (openId != null && SaveSystem.GameProfiles.TryGetManifest(openId, out GameProfileManifest manifest))
            {
                return manifest.DisplayName;
            }
            return "none";
        }
    }
}
