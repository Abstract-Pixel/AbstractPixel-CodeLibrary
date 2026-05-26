using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    /// <summary>
    /// A non-generic contract for UI Buttons and simple Unity Events to trigger level flow.
    /// </summary>
    public abstract class MonoLevelActions : MonoBehaviour
    {
        public abstract void LoadNextLevel();
        public abstract void UnlockNextLevel();
        public abstract void ResetManager();
    }
}