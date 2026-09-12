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
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            List<DisplayInfo> displayLayoutList = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displayLayoutList);

            if (CurrentValue < displayLayoutList.Count)
            {
                DisplayInfo targetDisplay = displayLayoutList[CurrentValue];
                DisplayInfo currentDisplay = Screen.mainWindowDisplayInfo;

                // Check if the target index matches our current layout index
                int currentMonitorIndex = -1;
                for (int i = 0; i < displayLayoutList.Count; i++)
                {
                    if (displayLayoutList[i].Equals(currentDisplay))
                    {
                        currentMonitorIndex = i;
                        break;
                    }
                }

                // FIX: Verify if we are already displaying on this monitor
                if (CurrentValue == currentMonitorIndex)
                {
                    return;
                }
                Screen.MoveMainWindowTo( targetDisplay, new Vector2Int(0, 0));
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