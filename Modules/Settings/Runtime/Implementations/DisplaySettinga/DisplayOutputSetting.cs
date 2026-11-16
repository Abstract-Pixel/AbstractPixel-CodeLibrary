using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DisplayOutputSetting : BaseOptionsSetting<int, int>
    {
        private const string PREFIX_TEXT = "Display ";

        // The default would move the game off the monitor it opened on.
        public override bool AppliesDefaultWithoutSave
        {
            get
            {
                return false;
            }
        }

        protected override void OnInitialize()
        {       
                GenerateDisplayData();
        }

        private void GenerateDisplayData()
        {
            List<DisplayInfo> displayLayout = ReadDisplayLayout();

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

            List<DisplayInfo> displayLayoutList = ReadDisplayLayout();

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

        /// <summary>The monitors, in order. Empty where Unity cannot list them (WebGL throws), so no option is offered.</summary>
        private static List<DisplayInfo> ReadDisplayLayout()
        {
            List<DisplayInfo> displayLayout = new List<DisplayInfo>();
#if UNITY_EDITOR || UNITY_STANDALONE
            Screen.GetDisplayLayout(displayLayout);
#endif
            return displayLayout;
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