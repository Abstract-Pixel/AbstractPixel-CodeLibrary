using System;

namespace AbstractPixel.LevelFramework
{
    [Serializable]
    public class BaseLevelData
    {
        public string levelGUID;
        public bool IsUnlocked;
        public LevelStatus LevelStatus;
    }
}