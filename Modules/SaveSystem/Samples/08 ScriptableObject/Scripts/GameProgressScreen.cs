using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.ScriptableObjectSample
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
            SampleGui.BeginPanel(20, 20, 440, "Game Progress", "A ScriptableObject asset, saved in the game profile.");
            SampleGui.Value("Levels completed: ", progress.LevelsCompleted.ToString());
            SampleGui.Value("Stars: ", progress.Stars.ToString());
            SampleGui.BeginRow();
            if (SampleGui.Button("Complete a level"))
            {
                progress.LevelsCompleted++;
            }
            if (SampleGui.Button("Earn a star"))
            {
                progress.Stars++;
            }
            SampleGui.EndRow();
            SampleGui.Footer();
            SampleGui.EndPanel();
        }
    }
}
