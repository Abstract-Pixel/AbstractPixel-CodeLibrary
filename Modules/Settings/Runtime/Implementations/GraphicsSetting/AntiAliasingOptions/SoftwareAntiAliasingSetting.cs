using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class SoftwareAntiAliasingSetting : BaseOptionsSetting<int, SoftwareAntiAliasingOptionData>
    {
        private List<Camera> registeredCameras = new List<Camera>();

        public void RegisterCamera(Camera cameraToRegister)
        {
            if (cameraToRegister == null) return;

            if (registeredCameras.Contains(cameraToRegister) == false)
            {
                registeredCameras.Add(cameraToRegister);
                
                if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
                {
                    ApplyAntiAliasingToCamera(cameraToRegister, OptionValues[CurrentValue]);
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

        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }

        private void GenerateDefaultOptions()
        {
            OptionDisplayNames = new string[]
            {
                "Disabled",
                "FXAA",
                "SMAA (Low)",
                "SMAA (Medium)",
                "SMAA (High)",
                "TAA"
            };

            DefaultValue = 0; // Default to "Disabled"

            OptionValues = new SoftwareAntiAliasingOptionData[]
            {
                // Index 0: Disabled
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.None,
                    Quality = AntialiasingQuality.Low
                },
                // Index 1: FXAA
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.FastApproximateAntialiasing,
                    Quality = AntialiasingQuality.Low
                },
                // Index 2: SMAA (Low)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.Low
                },
                // Index 3: SMAA (Medium)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.Medium
                },
                // Index 4: SMAA (High)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.High
                },
                // Index 5: TAA (Quality is ignored by URP for TAA)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.TemporalAntiAliasing,
                    Quality = AntialiasingQuality.Low
                }
            };
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            SoftwareAntiAliasingOptionData selectedOption = OptionValues[CurrentValue];

            // If no cameras are explicitly registered, default to Main Camera
            if (registeredCameras.Count == 0)
            {
                if (Camera.main != null)
                {
                    ApplyAntiAliasingToCamera(Camera.main, selectedOption);
                }
            }
            else
            {
                foreach (Camera cameraInstance in registeredCameras)
                {
                    if (cameraInstance != null)
                    {
                        ApplyAntiAliasingToCamera(cameraInstance, selectedOption);
                    }
                }
            }
        }

        private void ApplyAntiAliasingToCamera(Camera cameraInstance, SoftwareAntiAliasingOptionData option)
        {
            if (cameraInstance.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                // Setting antialiasing mode automatically disables all other software AA types 
                // on this camera while leaving hardware MSAA completely untouched!
                cameraData.antialiasing = option.Mode;
                cameraData.antialiasingQuality = option.Quality;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }
#endif
    }
}