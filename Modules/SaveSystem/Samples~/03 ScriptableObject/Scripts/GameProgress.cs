using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.ScriptableObjectSample
{
    // A saved ScriptableObject asset. "Achievements" lives in the game profile.
    [CreateAssetMenu(fileName = "Game Progress", menuName = "Save System Samples/Game Progress")]
    [Savable(SaveCategory.Achievements)]
    public class GameProgress : ScriptableObject, ISavable<GameProgressData>
    {
        public int LevelsCompleted;
        public int Stars;

        // The asset registers itself when Unity loads it. A later step of the save system makes this automatic.
        void OnEnable()
        {
            RegisterWithSaveSystem();
        }

        // Unregistering captures the latest values, so they're written on the next save.
        void OnDisable()
        {
            SaveSystem.Unregister(this);
        }

        // In the Editor, Unity can load the asset before play mode starts the save system, so OnEnable's call does
        // nothing then. Calling this again from a scene script covers that. Registering twice is harmless.
        public void RegisterWithSaveSystem()
        {
            SaveSystem.Register(this, "game-progress");
        }

        public void ResetToNewGame()
        {
            LevelsCompleted = 0;
            Stars = 0;
        }

        public GameProgressData CaptureData()
        {
            return new GameProgressData { LevelsCompleted = LevelsCompleted, Stars = Stars };
        }

        public void RestoreData(GameProgressData _loadedData)
        {
            LevelsCompleted = _loadedData.LevelsCompleted;
            Stars = _loadedData.Stars;
        }
    }
}
