using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DisplayOutputSetting : IntOptionsSetting
    {
        const string PREFIX_TEXT = "Display ";
        public override void ApplySettingLogic()
        {
            Display.displays[CurrentValue].Activate();
        }

        protected override void OnInitialize()
        {
            GenerateDisplayData();
        }

        void GenerateDisplayData()
        {
            int displaysLength = Display.displays.Length;
            OptionValues = new int[displaysLength];
            OptionDisplayNames  = new string[displaysLength];
            for (int i = 0; i < displaysLength; i++)
            {
                OptionValues[i] = 0;
                OptionDisplayNames[i]= PREFIX_TEXT +i;
            }
            DefaultValue = 0;
        }

        protected override void OnValidateInEditor()
        {
            GenerateDisplayData();
        }   
    }
}
