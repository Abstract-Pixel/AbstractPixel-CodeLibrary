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
                "TAA (Low)",
                "TAA (Medium)",
                "TAA (High)"
            };

            DefaultValue = 0; // Default to "Disabled"

            OptionValues = new SoftwareAntiAliasingOptionData[]
            {
                // Index 0: Disabled
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.None,
                    Quality = AntialiasingQuality.Low,
                    TaaSharpening = 0.0f
                },
                // Index 1: FXAA
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.FastApproximateAntialiasing,
                    Quality = AntialiasingQuality.Low,
                    TaaSharpening = 0.0f
                },
                // Index 2: SMAA (Low)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.Low,
                    TaaSharpening = 0.0f
                },
                // Index 3: SMAA (Medium)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.Medium,
                    TaaSharpening = 0.0f
                },
                // Index 4: SMAA (High)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Quality = AntialiasingQuality.High,
                    TaaSharpening = 0.0f
                },
                // Index 5: TAA (Low)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.TemporalAntiAliasing,
                    Quality = AntialiasingQuality.Low, // Ignored by TAA
                    TaaSharpening = 0.25f
                },
                // Index 6: TAA (Medium)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.TemporalAntiAliasing,
                    Quality = AntialiasingQuality.Medium, // Ignored by TAA
                    TaaSharpening = 0.60f
                },
                // Index 7: TAA (High)
                new SoftwareAntiAliasingOptionData
                {
                    Mode = AntialiasingMode.TemporalAntiAliasing,
                    Quality = AntialiasingQuality.High, // Ignored by TAA
                    TaaSharpening = 1.0f
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
                // 1. Set the AA Mode and SMAA Quality
                cameraData.antialiasing = option.Mode;
                cameraData.antialiasingQuality = option.Quality;
                if(option.Mode is not AntialiasingMode.TemporalAntiAliasing)
                {
                    return;
                }
                // 2. Set the TAA Contrast Adaptive Sharpening (CAS)
                // Note: taaSettings is a 'ref struct' property in modern URP, so modifying it here directly modifies the camera!
                cameraData.taaSettings.contrastAdaptiveSharpening = option.TaaSharpening;
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