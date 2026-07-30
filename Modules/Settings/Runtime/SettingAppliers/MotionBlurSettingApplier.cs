using UnityEngine;
using UnityEngine.Rendering;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(Volume))]
    public class MotionBlurSettingApplier : AbstractSettingApplier<float>
    {
        private Volume postProcessingVolume;

        private void Awake()
        {
            TryGetComponent(out postProcessingVolume);
        }

        protected override void OnLiveSettingBinded(BaseSetting<float> _bindedSetting)
        {
            MotionBlurSetting motionBlurSetting = _bindedSetting as MotionBlurSetting;

            if (motionBlurSetting != null && postProcessingVolume != null && postProcessingVolume.profile != null)
            {
                motionBlurSetting.RegisterProfile(postProcessingVolume.profile);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<float> _unbindedSetting)
        {
            MotionBlurSetting motionBlurSetting = _unbindedSetting as MotionBlurSetting;

            if (motionBlurSetting != null && postProcessingVolume != null && postProcessingVolume.profile != null)
            {
                motionBlurSetting.UnRegisterProfile(postProcessingVolume.profile);
            }
        }
    }
}