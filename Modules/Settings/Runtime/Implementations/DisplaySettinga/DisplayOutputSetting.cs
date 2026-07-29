using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DisplayOutputSetting : BaseOptionsSetting<int, int>
    {
        private const string PREFIX_TEXT = "Display ";

        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDisplayData();
            }
        }

        private void GenerateDisplayData()
        {
            int displaysLength = Display.displays.Length;
            OptionValues = new int[displaysLength];
            OptionDisplayNames = new string[displaysLength];

            for (int i = 0; i < displaysLength; i++)
            {
                OptionValues[i] = i;
                OptionDisplayNames[i] = PREFIX_TEXT + i;
            }

            DefaultValue = 0;
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                Display.displays[CurrentValue].Activate();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateDisplayData();
        }
#endif
    }
}