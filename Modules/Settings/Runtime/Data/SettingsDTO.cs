using System;
using System.Collections.Generic;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class SettingsDTO
    {
        public Dictionary<string, int> IntegerSettings;
        public Dictionary<string, float> FloatSettings;
        public Dictionary<string, string> StringSettings;
        public Dictionary<string, bool> BooleanSettings;

        public SettingsDTO()
        {
            IntegerSettings = new Dictionary<string, int>();
            FloatSettings = new Dictionary<string, float>();
            StringSettings = new Dictionary<string, string>();
            BooleanSettings = new Dictionary<string, bool>();
        }
    }
}