using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public enum FOVTargetType
    {
        Camera,
        MainCamera,
        Cinemachine
    }

    [Serializable]
    public class FOVSetting : FloatSliderSetting
    {
        [Header("FOV Configuration")]
        [SerializeField] private FOVTargetType targetType = FOVTargetType.MainCamera;

        private List<Camera> registeredCameras = new List<Camera>();
        private List<CinemachineCamera> registeredVirtualCameras = new List<CinemachineCamera>();

        private const float DEFAULT_FOV_ON_RESET = 60.0f;

        public void RegisterCamera(Camera cameraToRegister)
        {
            if (cameraToRegister == null)
            {
                return;
            }

            if (registeredCameras.Contains(cameraToRegister) == false)
            {
                registeredCameras.Add(cameraToRegister);

                if (targetType == FOVTargetType.Camera)
                {
                    cameraToRegister.fieldOfView = CurrentValue;
                }
            }
        }

        public void UnregisterCamera(Camera cameraToRemove)
        {
            if (cameraToRemove != null && registeredCameras.Contains(cameraToRemove) == true)
            {
                registeredCameras.Remove(cameraToRemove);
            }
        }

        public void RegisterVirtualCamera(CinemachineCamera virtualCameraToRegister)
        {
            if (virtualCameraToRegister == null)
            {
                return;
            }

            if (registeredVirtualCameras.Contains(virtualCameraToRegister) == false)
            {
                registeredVirtualCameras.Add(virtualCameraToRegister);

                if (targetType == FOVTargetType.Cinemachine)
                {
                    virtualCameraToRegister.Lens.FieldOfView = CurrentValue;
                }
            }
        }

        public void UnregisterVirtualCamera(CinemachineCamera virtualCameraToRemove)
        {
            if (virtualCameraToRemove != null && registeredVirtualCameras.Contains(virtualCameraToRemove) == true)
            {
                registeredVirtualCameras.Remove(virtualCameraToRemove);
            }
        }

        protected override void OnInitialize()
        {
            if(MinValue ==0 || MaxValue ==0f || DefaultValue ==0f )
            {
                ConfigureSliderLimits();
            }
        }

        private void ConfigureSliderLimits()
        {
            MinValue = 40.0f;
            MaxValue = 120.0f;
            DisplayMinValue = 30.0f;
            DisplayMaxValue = 120.0f;
            DefaultValue = DEFAULT_FOV_ON_RESET;
        }

        protected override void OnApplySettingLogic()
        {
            if (targetType == FOVTargetType.MainCamera)
            {
                if (Camera.main != null)
                {
                    Camera.main.fieldOfView = CurrentValue;
                }
            }
            else if (targetType == FOVTargetType.Camera)
            {
                foreach (Camera cameraInstance in registeredCameras)
                {
                    if (cameraInstance != null)
                    {
                        cameraInstance.fieldOfView = CurrentValue;
                    }
                }
            }
            else if (targetType == FOVTargetType.Cinemachine)
            {
                foreach (CinemachineCamera virtualCameraInstance in registeredVirtualCameras)
                {
                    if (virtualCameraInstance != null)
                    {
                        virtualCameraInstance.Lens.FieldOfView = CurrentValue;
                    }
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            ConfigureSliderLimits();
        }
#endif
    }
}