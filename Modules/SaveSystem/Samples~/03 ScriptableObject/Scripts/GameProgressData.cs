using System;

namespace SaveSystemSamples.ScriptableObjectSample
{
    // The data that goes into the save file.
    [Serializable]
    public class GameProgressData
    {
        public int LevelsCompleted;
        public int Stars;
    }
}
