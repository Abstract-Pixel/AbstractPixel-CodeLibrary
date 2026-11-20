using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.ScriptableObjectSample
{
    // A saved ScriptableObject asset: [Save] fields and one Register line, like a plain C# class.
    // With no ISaveIn, it lives in the GameProfile save file.
    [CreateAssetMenu(fileName = "Game Progress", menuName = "Save System Samples/Game Progress")]
    public class GameProgress : ScriptableObject
    {
        [Save] public int LevelsCompleted;
        [Save] public int Stars;

        // The asset registers itself when Unity loads it. A later step of the save system makes this automatic.
        void OnEnable()
        {
            RegisterWithSaveSystem();
        }

        // Unregistering captures the latest values, so the next save still writes them.
        void OnDisable()
        {
            SaveActions.Unregister(this);
        }

        // In the Editor, Unity can load the asset before play mode starts the save system, so OnEnable's call does
        // nothing then. Calling this again from a scene script covers that. Registering twice with the same save ID
        // is harmless. The save ID is fixed, so renaming the asset or the class never loses its data.
        public void RegisterWithSaveSystem()
        {
            SaveActions.Register(this, "game-progress");
        }

        public void ResetToNewGame()
        {
            LevelsCompleted = 0;
            Stars = 0;
        }
    }
}
