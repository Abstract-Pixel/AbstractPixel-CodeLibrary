using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class FOVSetting : FloatSliderSetting
    {
        [Header("FOV Configuration")]
        [SerializeField] private FOVTargetType targetType = FOVTargetType.MainCamera;
        // Runtime lists holding injected references from the scene
        private List<Camera> registeredCameras = new List<Camera>();
        private List<CinemachineCamera> registeredVirtualCameras = new List<CinemachineCamera>();

        private const int DEFAULT_FOV_ON_RESET = 60;

        // =========================================================
        // REGISTRATION API
        // =========================================================

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

        }

        public override void ApplySettingLogic()
        {
            // Mode 1: Automatically find and apply to Camera.main (No Applier needed in scene)
            if (targetType == FOVTargetType.MainCamera)
            {
                if (Camera.main == null) { return; }
                Camera.main.fieldOfView = CurrentValue;
            }
            // Mode 2: Apply to specific registered Standard Cameras
            else if (targetType == FOVTargetType.Camera)
            {
                foreach (Camera cameraInstance in registeredCameras)
                {
                    if (cameraInstance == null) { continue; }
                    cameraInstance.fieldOfView = CurrentValue;
                }
            }
            // Mode 3: Apply to specific registered Cinemachine Virtual Cameras
            else if (targetType == FOVTargetType.Cinemachine)
            {
                foreach (CinemachineCamera virtualCameraInstance in registeredVirtualCameras)
                {
                    if (virtualCameraInstance == null) { continue; }
                    virtualCameraInstance.Lens.FieldOfView = CurrentValue;
                }
            }
        }

        protected override void OnValidateInEditor()
        {
            DefaultValue = DEFAULT_FOV_ON_RESET;
        }

        public enum FOVTargetType
        {
            Camera,
            MainCamera,
            Cinemachine
        }
    }
}