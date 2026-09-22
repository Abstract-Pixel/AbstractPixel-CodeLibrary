using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class BaseCameraInversionSetting : BoolSetting
    {
        // EDGE CASE FIX: Same as sensitivity, the axis is hardcoded by the concrete class.
        public abstract CameraAxisTarget TargetAxis { get; }

        [field: Header("Hardware Defaults")]
        [field: Tooltip("True if the input system natively reports this axis inverted for Mouse.")]
        [field: SerializeField] public bool InvertMouseByDefault { get; private set; } = false;
        
        [field: Tooltip("True if the input system natively reports this axis inverted for Gamepad.")]
        [field: SerializeField] public bool InvertGamepadByDefault { get; private set; } = true;

        private List<CinemachineCameraAxisApplier> registeredAppliersList = new List<CinemachineCameraAxisApplier>();

        public void RegisterApplier(CinemachineCameraAxisApplier applierToRegister)
        {
            if (applierToRegister != null && registeredAppliersList.Contains(applierToRegister) == false)
            {
                registeredAppliersList.Add(applierToRegister);
            }
        }

        public void UnregisterApplier(CinemachineCameraAxisApplier applierToRemove)
        {
            if (applierToRemove != null && registeredAppliersList.Contains(applierToRemove) == true)
            {
                registeredAppliersList.Remove(applierToRemove);
            }
        }

        protected override void OnInitialize()
        {
            DefaultValue = false;
        }

        protected override void OnApplySettingLogic()
        {
            // EDGE CASE FIX: Stale reference cleanup for inversion list.
            for (int i = registeredAppliersList.Count - 1; i >= 0; i--)
            {
                if (registeredAppliersList[i] == null)
                {
                    registeredAppliersList.RemoveAt(i);
                }
            }

            foreach (CinemachineCameraAxisApplier applier in registeredAppliersList)
            {
                applier.RecalculateGain(TargetAxis);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            DefaultValue = false;
        }
#endif
    }
}