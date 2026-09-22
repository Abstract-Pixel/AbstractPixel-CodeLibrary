using UnityEngine;
using AbstractPixel.Core;
using System.Collections.Generic;

namespace AbstractPixel.Settings
{
    public class DeviceFamilyVisibilityToggler : MonoBehaviour
    {
        [Header("UI Target")]
        [Tooltip("Assign the child GameObject containing the actual UI element (e.g., the Slider row). Do NOT assign the GameObject this script is attached to.")]
        [SerializeField] private GameObject targetUIGameObject;

        [Header("Device Configuration")]
        [SerializeField] private List<DeviceFamily> visibleForDevices = new List<DeviceFamily>();

        private void OnEnable()
        {
            // EDGE CASE FIX: Prevent errors if the designer forgets to assign the target UI
            if (targetUIGameObject == null)
            {
                return;
            }
            
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;
            HandleDeviceFamilyChanged(InputDeviceTracker.CurrentDeviceFamily);
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
        }

        private void HandleDeviceFamilyChanged(DeviceFamily currentDeviceFamily)
        {
            if (targetUIGameObject == null)
            {
                return;
            }

            // EDGE CASE FIX: If the list is empty, it safely evaluates to false rather than crashing.
            bool shouldBeVisible = visibleForDevices.Contains(currentDeviceFamily);

            if (targetUIGameObject.activeSelf != shouldBeVisible)
            {
                targetUIGameObject.SetActive(shouldBeVisible);
            }
        }
    }
}