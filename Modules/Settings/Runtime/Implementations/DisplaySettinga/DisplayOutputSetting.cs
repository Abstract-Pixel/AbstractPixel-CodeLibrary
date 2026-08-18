using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DisplayOutputSetting : BaseOptionsSetting<int, int>
    {
        private const string PREFIX_TEXT = "Display ";

        protected override void OnInitialize()
        {       
                GenerateDisplayData();
        }

        private void GenerateDisplayData()
        {
            List<DisplayInfo> displayLayout = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displayLayout);

            int displaysLength = displayLayout.Count;
            OptionValues = new int[displaysLength];
            OptionDisplayNames = new string[displaysLength];

            for (int i = 0; i < displaysLength; i++)
            {
                OptionValues[i] = i;
                OptionDisplayNames[i] = PREFIX_TEXT + i;
            }
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                List<DisplayInfo> displayLayout = new List<DisplayInfo>();
                Screen.GetDisplayLayout(displayLayout);

                if (CurrentValue < displayLayout.Count)
                {
                    DisplayInfo targetDisplay = displayLayout[CurrentValue];
                    DisplayInfo currentDisplay = Screen.mainWindowDisplayInfo;

                    // FIX: If the game window is ALREADY on this monitor, DO NOT call MoveMainWindowTo!
                    // Calling this when already on the monitor causes Windows to minimize the game.
                    if (currentDisplay.name == targetDisplay.name ||
                       (currentDisplay.width == targetDisplay.width && currentDisplay.height == targetDisplay.height && currentDisplay.workArea == targetDisplay.workArea))
                    {
                        return;
                    }

                    Screen.MoveMainWindowTo(targetDisplay, new Vector2Int(0, 0));
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateDisplayData();
            DefaultValue = 0;
        }
#endif
    }
}