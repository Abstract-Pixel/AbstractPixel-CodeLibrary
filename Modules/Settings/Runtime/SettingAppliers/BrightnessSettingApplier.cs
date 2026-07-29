using UnityEngine;
using UnityEngine.Rendering;

namespace AbstractPixel.Settings
{
    public class BrightnessSettingApplier : AbstractSettingApplier<float>
    {
        Volume postProcessingVolume;
      
        private void Awake()
        {
            TryGetComponent(out postProcessingVolume);
        }
        protected override void OnLiveSettingBinded(BaseSetting<float> _bindedSetting)
        {
            BrightnessSetting setting = _bindedSetting as BrightnessSetting;

            if(setting!=null)
            {
                setting.RegisterProfile(postProcessingVolume.profile);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<float> _unbindedSetting)
        {
            BrightnessSetting setting = _unbindedSetting as BrightnessSetting;

            if (setting != null)
            {
                setting.UnRegisterProfile(postProcessingVolume.profile);
            }
        }
    }
}
