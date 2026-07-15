using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ScreenTypeSetting : IntOptionsSetting
    {
        public override void Initialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                // We map directly to Unity's native FullScreenMode enum integers
                OptionValues = new int[] 
                { 
                    (int)FullScreenMode.ExclusiveFullScreen, 
                    (int)FullScreenMode.FullScreenWindow, 
                    (int)FullScreenMode.Windowed 
                };
                
                OptionDisplayNames = new string[] 
                { 
                    "Exclusive Fullscreen", 
                    "Borderless Window", 
                    "Windowed" 
                };
            }

            base.Initialize();
        }

        public override void ApplyLogic()
        {
            // We cast the integer back to the Unity enum
            FullScreenMode selectedMode = (FullScreenMode)CurrentValue;
            
            Screen.fullScreenMode = selectedMode;
        }
    }
}