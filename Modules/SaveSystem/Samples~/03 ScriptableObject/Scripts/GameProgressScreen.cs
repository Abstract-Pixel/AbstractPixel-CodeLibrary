using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.ScriptableObjectSample
{
    // Shows the Game Progress asset's values and the sample's buttons.
    public class GameProgressScreen : MonoBehaviour
    {
        [SerializeField] GameProgress progress;

        public GameProgress Progress => progress;

        // See GameProgress.RegisterWithSaveSystem for why the scene registers the asset as well.
        void Awake()
        {
            progress.RegisterWithSaveSystem();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 360, 300));
            GUILayout.Label("Game Progress (a ScriptableObject asset, saved in the game profile)");
            GUILayout.Label($"Levels completed: {progress.LevelsCompleted}");
            GUILayout.Label($"Stars: {progress.Stars}");

            if (GUILayout.Button("Complete a level"))
            {
                progress.LevelsCompleted++;
            }
            if (GUILayout.Button("Earn a star"))
            {
                progress.Stars++;
            }
            if (GUILayout.Button("Save"))
            {
                SaveSystem.SaveAll();
            }
            if (GUILayout.Button("Reload scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            GUILayout.EndArea();
        }
    }
}
