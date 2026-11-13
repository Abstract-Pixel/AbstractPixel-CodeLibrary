using UnityEngine;

namespace AbstractPixel.Settings
{
    public class SoftwareAntiAliasingApplier : AbstractSettingApplier<int>
    {
        private Camera targetCamera;

        private void Awake()
        {
            TryGetComponent(out targetCamera);
        }

        protected override void OnLiveSettingBinded(BaseSetting<int> setting)
        {
            SoftwareAntiAliasingSetting aaSetting = setting as SoftwareAntiAliasingSetting;

            if (aaSetting != null && targetCamera != null)
            {
                aaSetting.RegisterCamera(targetCamera);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<int> setting)
        {
            SoftwareAntiAliasingSetting aaSetting = setting as SoftwareAntiAliasingSetting;

            if (aaSetting != null && targetCamera != null)
            {
                aaSetting.UnregisterCamera(targetCamera);
            }
        }
    }
}