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

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            
        }
#endif
    }
}