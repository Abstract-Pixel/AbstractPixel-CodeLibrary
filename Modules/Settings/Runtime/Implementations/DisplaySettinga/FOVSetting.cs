using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class FOVSetting : FloatSliderSetting
    {
        
        protected override void OnInitialize()
        {
            
        }

        public override void ApplySettingLogic()
        {
            if (Camera.main != null)
            {
                Camera.main.fieldOfView = CurrentValue;
            }
            else
            {
                Debug.LogWarning("[FOVSetting] Could not find Camera.main to apply Field of View.");
            }
        }

        protected override void OnValidateInEditor()
        {
            
        }
    }
}