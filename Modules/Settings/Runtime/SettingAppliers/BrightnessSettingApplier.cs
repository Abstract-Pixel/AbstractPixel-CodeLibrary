using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    public class BrightnessSettingApplier : AbstractSettingApplier<float>
    {
        Volume postProcessingVolume;
        VolumeProfile settingsProfile;

        private void Awake()
        {
            TryGetComponent(out postProcessingVolume);
        }

        protected override void OnLiveSettingBinded(BaseSetting<float> _bindedSetting)
        {
            BrightnessSetting setting = _bindedSetting as BrightnessSetting;

            if (setting != null)
            {
                settingsProfile = SettingsOverlayVolume.GetProfileForOverride<ColorAdjustments>(postProcessingVolume);
                SeedAuthoredExposure();
                setting.RegisterProfile(settingsProfile);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<float> _unbindedSetting)
        {
            BrightnessSetting setting = _unbindedSetting as BrightnessSetting;

            if (setting != null)
            {
                setting.UnRegisterProfile(settingsProfile);
            }
        }

        // Brightness is an offset on the level's authored exposure, so the overlay starts from that value.
        private void SeedAuthoredExposure()
        {
            // Local volumes write into their own clone, which already holds the authored exposure.
            if (settingsProfile == null || !postProcessingVolume.isGlobal)
            {
                return;
            }

            settingsProfile.TryGet(out ColorAdjustments overlayAdjustments);
            if (overlayAdjustments.postExposure.overrideState)
            {
                return;
            }

            postProcessingVolume.sharedProfile.TryGet(out ColorAdjustments levelAdjustments);
            bool levelSetsExposure = levelAdjustments.active && levelAdjustments.postExposure.overrideState;
            overlayAdjustments.postExposure.value = levelSetsExposure ? levelAdjustments.postExposure.value : 0f;
        }
    }
}
