using UnityEngine;
using Unity.Cinemachine;

namespace AbstractPixel.Settings.Gameplay
{
    public class FOVSettingApplier : AbstractSettingApplier<float>
    {
        private Camera targetCamera;
        private CinemachineCamera targetVirtualCamera;

        private void Awake()
        {
            TryGetComponent(out  targetCamera);
            TryGetComponent(out targetVirtualCamera);
        }

        protected override void OnLiveSettingBinded(BaseSetting<float> setting)
        {
            FOVSetting fovSetting = setting as FOVSetting;

            if (fovSetting == null)
            {
                return;
            }

            // Register standard camera if present
            if (targetCamera != null)
            {
                fovSetting.RegisterCamera(targetCamera);
            }

            // Register Cinemachine virtual camera if present
            if (targetVirtualCamera != null)
            {
                fovSetting.RegisterVirtualCamera(targetVirtualCamera);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<float> setting)
        {
            FOVSetting fovSetting = setting as FOVSetting;

            if (fovSetting == null)
            {
                return;
            }

            // Unregister standard camera
            if (targetCamera != null)
            {
                fovSetting.UnregisterCamera(targetCamera);
            }

            // Unregister Cinemachine virtual camera
            if (targetVirtualCamera != null)
            {
                fovSetting.UnregisterVirtualCamera(targetVirtualCamera);
            }
        }
    }
}