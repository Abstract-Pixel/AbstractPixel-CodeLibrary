using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class BaseCameraSensitivitySetting : FloatSliderSetting
    {
        // EDGE CASE FIX: TargetAxis is now abstract. Designers cannot accidentally mismatch 
        // a "Mouse X" setting to a "Y Axis" in the inspector. The concrete class enforces it.
        public abstract CameraAxisTarget TargetAxis { get; }

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

            ConfigureDefaults();
        }

        private void ConfigureDefaults()
        {
            if (MinValue == 0f && MaxValue == 0f)
            {
                MinValue = 0.1f;
                MaxValue = 5.0f;
                DisplayMinValue = 1.0f;
                DisplayMaxValue = 10.0f;
                DefaultValue = 1.0f;
            }
        }

        protected override void OnApplySettingLogic()
        {
            // EDGE CASE FIX: If an applier was destroyed abruptly without unregistering, 
            // purge it from the list backwards to prevent NullReferenceExceptions.
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
            ConfigureDefaults();
        }
#endif
    }
}