using System;
using AbstractPixel.Settings;

namespace AbstractPixel.Rumble
{
    /// <summary>"Controller Vibration" slider: 0..1, shown 0..100 %. 0 turns rumble and PlayStation trigger effects off.</summary>
    [Serializable]
    public class ControllerVibrationSetting : FloatSliderSetting
    {
        private const float DefaultIntensity = 1f;

        protected override void OnInitialize()
        {
        }

        protected override void OnApplySettingLogic()
        {
            RumbleService.PlayerIntensity = CurrentValue;
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            MinValue = 0f;
            MaxValue = 1f;
            DisplayMinValue = 0f;
            DisplayMaxValue = 100f;
            DefaultValue = DefaultIntensity;
        }
#endif
    }
}
