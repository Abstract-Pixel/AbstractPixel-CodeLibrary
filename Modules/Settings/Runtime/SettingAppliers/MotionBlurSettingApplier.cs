using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(Volume))]
    public class MotionBlurSettingApplier : AbstractSettingApplier<float>
    {
        private Volume postProcessingVolume;
        private VolumeProfile settingsProfile;

        private void Awake()
        {
            TryGetComponent(out postProcessingVolume);
        }

        protected override void OnLiveSettingBinded(BaseSetting<float> _bindedSetting)
        {
            MotionBlurSetting motionBlurSetting = _bindedSetting as MotionBlurSetting;

            if (motionBlurSetting != null)
            {
                settingsProfile = SettingsOverlayVolume.GetProfileForOverride<MotionBlur>(postProcessingVolume);
                motionBlurSetting.RegisterProfile(settingsProfile);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<float> _unbindedSetting)
        {
            MotionBlurSetting motionBlurSetting = _unbindedSetting as MotionBlurSetting;

            if (motionBlurSetting != null)
            {
                motionBlurSetting.UnRegisterProfile(settingsProfile);
            }
        }
    }
}