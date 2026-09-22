using UnityEngine;
using Unity.Cinemachine;
using AbstractPixel.Core;
using System;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class CinemachineCameraAxisApplier : MonoBehaviour
    {
        private CinemachineInputAxisController axisController;

        // Cached X-Axis Settings
        private BaseCameraSensitivitySetting mouseXSensitivitySetting;
        private BaseCameraSensitivitySetting gamepadXSensitivitySetting;
        private BaseCameraInversionSetting horizontalInversionSetting;

        // Cached Y-Axis Settings
        private BaseCameraSensitivitySetting mouseYSensitivitySetting;
        private BaseCameraSensitivitySetting gamepadYSensitivitySetting;
        private BaseCameraInversionSetting verticalInversionSetting;

        private void Awake()
        {
            TryGetComponent(out axisController);
        }

        private void OnEnable()
        {
            BindToSettingsBackend();
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;
        }

        private void OnDisable()
        {
            UnbindFromSettingsBackend();
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
        }

        private void BindToSettingsBackend()
        {
            if (SettingsManager.Instance == null)
            {
                return;
            }

            // Fetch specific types from the SettingsManager
            mouseXSensitivitySetting = SettingsManager.Instance.GetSetting(typeof(MouseXSensitivitySetting)) as BaseCameraSensitivitySetting;
            mouseYSensitivitySetting = SettingsManager.Instance.GetSetting(typeof(MouseYSensitivitySetting)) as BaseCameraSensitivitySetting;
            gamepadXSensitivitySetting = SettingsManager.Instance.GetSetting(typeof(GamepadXSensitivitySetting)) as BaseCameraSensitivitySetting;
            gamepadYSensitivitySetting = SettingsManager.Instance.GetSetting(typeof(GamepadYSensitivitySetting)) as BaseCameraSensitivitySetting;
            horizontalInversionSetting = SettingsManager.Instance.GetSetting(typeof(HorizontalCameraInversionSetting)) as BaseCameraInversionSetting;
            verticalInversionSetting = SettingsManager.Instance.GetSetting(typeof(VerticalCameraInversionSetting)) as BaseCameraInversionSetting;

            if (mouseXSensitivitySetting != null)
            {
                mouseXSensitivitySetting.RegisterApplier(this);
                mouseXSensitivitySetting.OnValueChanged += HandleHorizontalFloatValueChanged;
            }

            if (gamepadXSensitivitySetting != null)
            {
                gamepadXSensitivitySetting.RegisterApplier(this);
                gamepadXSensitivitySetting.OnValueChanged += HandleHorizontalFloatValueChanged;
            }

            if (horizontalInversionSetting != null)
            {
                horizontalInversionSetting.RegisterApplier(this);
                horizontalInversionSetting.OnValueChanged += HandleHorizontalBoolValueChanged;
            }

            if (mouseYSensitivitySetting != null)
            {
                mouseYSensitivitySetting.RegisterApplier(this);
                mouseYSensitivitySetting.OnValueChanged += HandleVerticalFloatValueChanged;
            }

            if (gamepadYSensitivitySetting != null)
            {
                gamepadYSensitivitySetting.RegisterApplier(this);
                gamepadYSensitivitySetting.OnValueChanged += HandleVerticalFloatValueChanged;
            }

            if (verticalInversionSetting != null)
            {
                verticalInversionSetting.RegisterApplier(this);
                verticalInversionSetting.OnValueChanged += HandleVerticalBoolValueChanged;
            }

            // Initial calculation for both axes upon enabling
            RecalculateGain(CameraAxisTarget.HorizontalX);
            RecalculateGain(CameraAxisTarget.VerticalY);
        }

        private void UnbindFromSettingsBackend()
        {
            if (mouseXSensitivitySetting != null)
            {
                mouseXSensitivitySetting.UnregisterApplier(this);
                mouseXSensitivitySetting.OnValueChanged -= HandleHorizontalFloatValueChanged;
            }

            if (gamepadXSensitivitySetting != null)
            {
                gamepadXSensitivitySetting.UnregisterApplier(this);
                gamepadXSensitivitySetting.OnValueChanged -= HandleHorizontalFloatValueChanged;
            }

            if (horizontalInversionSetting != null)
            {
                horizontalInversionSetting.UnregisterApplier(this);
                horizontalInversionSetting.OnValueChanged -= HandleHorizontalBoolValueChanged;
            }

            if (mouseYSensitivitySetting != null)
            {
                mouseYSensitivitySetting.UnregisterApplier(this);
                mouseYSensitivitySetting.OnValueChanged -= HandleVerticalFloatValueChanged;
            }

            if (gamepadYSensitivitySetting != null)
            {
                gamepadYSensitivitySetting.UnregisterApplier(this);
                gamepadYSensitivitySetting.OnValueChanged -= HandleVerticalFloatValueChanged;
            }

            if (verticalInversionSetting != null)
            {
                verticalInversionSetting.UnregisterApplier(this);
                verticalInversionSetting.OnValueChanged -= HandleVerticalBoolValueChanged;
            }
        }

        private void HandleHorizontalFloatValueChanged(float _newValue) => RecalculateGain(CameraAxisTarget.HorizontalX);
        private void HandleHorizontalBoolValueChanged(bool _newValue) => RecalculateGain(CameraAxisTarget.HorizontalX);
        private void HandleVerticalFloatValueChanged(float _newValue) => RecalculateGain(CameraAxisTarget.VerticalY);
        private void HandleVerticalBoolValueChanged(bool _newValue) => RecalculateGain(CameraAxisTarget.VerticalY);

        private void HandleDeviceFamilyChanged(DeviceFamily currentDeviceFamily)
        {
            RecalculateGain(CameraAxisTarget.HorizontalX);
            RecalculateGain(CameraAxisTarget.VerticalY);
        }

        public void RecalculateGain(CameraAxisTarget targetAxis)
        {
            int axisIndex = (int)targetAxis;

            if (axisController == null)
            {
                return;
            }

            bool isGamepadActive = InputDeviceTracker.IsLastUsedDeviceOnlyGamepad();

            float calculatedSensitivity = 1.0f;
            bool isUserInverting = false;
            bool baseHardwareInverted = false;

            if (targetAxis == CameraAxisTarget.HorizontalX)
            {
                if (isGamepadActive == true)
                {
                    if (gamepadXSensitivitySetting != null) calculatedSensitivity = gamepadXSensitivitySetting.CurrentValue;
                    if (horizontalInversionSetting != null)
                    {
                        isUserInverting = horizontalInversionSetting.CurrentValue;
                        baseHardwareInverted = horizontalInversionSetting.InvertGamepadByDefault;
                    }
                }
                else
                {
                    if (mouseXSensitivitySetting != null) calculatedSensitivity = mouseXSensitivitySetting.CurrentValue;
                    if (horizontalInversionSetting != null)
                    {
                        isUserInverting = horizontalInversionSetting.CurrentValue;
                        baseHardwareInverted = horizontalInversionSetting.InvertMouseByDefault;
                    }
                }
            }
            else // VerticalY
            {
                if (isGamepadActive == true)
                {
                    if (gamepadYSensitivitySetting != null) calculatedSensitivity = gamepadYSensitivitySetting.CurrentValue;
                    if (verticalInversionSetting != null)
                    {
                        isUserInverting = verticalInversionSetting.CurrentValue;
                        baseHardwareInverted = verticalInversionSetting.InvertGamepadByDefault;
                    }
                }
                else
                {
                    if (mouseYSensitivitySetting != null) calculatedSensitivity = mouseYSensitivitySetting.CurrentValue;
                    if (verticalInversionSetting != null)
                    {
                        isUserInverting = verticalInversionSetting.CurrentValue;
                        baseHardwareInverted = verticalInversionSetting.InvertMouseByDefault;
                    }
                }
            }

            // 1. Start with normal orientation (1.0 = standard, -1.0 = inverted)
            float inversionMultiplier = 1.0f;

            if (baseHardwareInverted == true || isUserInverting == true)
            {
                inversionMultiplier *= -1.0f;
            }
            float finalGainMultiplier = calculatedSensitivity * inversionMultiplier;

            var controllerData = axisController.Controllers[axisIndex];
            controllerData.Input.Gain= finalGainMultiplier;
        }
    }
}