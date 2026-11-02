using System;
using AbstractPixel.Settings;

namespace AbstractPixel.Rumble
{
    /// <summary>"Adaptive Triggers" on/off. Off: the DualSense triggers stay loose (no resistance, shudder or buzz). Rumble is not affected.</summary>
    [Serializable]
    public class AdaptiveTriggersSetting : BoolSetting
    {
        protected override void OnInitialize()
        {
        }

        protected override void OnApplySettingLogic()
        {
            RumbleService.AreTriggersEnabled = CurrentValue;
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            DefaultValue = true;
        }
#endif
    }
}
