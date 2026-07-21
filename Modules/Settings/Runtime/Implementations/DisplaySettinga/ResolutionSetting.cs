using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ResolutionSetting : IntOptionsSetting
    {
        private Resolution[] availableResolutions;

        protected override void OnInitialize()
        {
            GenerateResolutions();
        }

        private void GenerateResolutions()
        {
            Resolution[] rawHardwareResolutions = Screen.resolutions;

            List<Resolution> uniqueResolutionsList = new List<Resolution>();
            List<int> resolutionIndicesList = new List<int>();
            List<string> resolutionNamesList = new List<string>();

            // We iterate BACKWARDS through the raw resolutions array 
            // so higher resolutions are added first in our list!
            for (int i = rawHardwareResolutions.Length - 1; i >= 0; i--)
            {
                Resolution hardwareResolution = rawHardwareResolutions[i];
                bool alreadyExists = false;

                foreach (Resolution uniqueResolution in uniqueResolutionsList)
                {
                    if (uniqueResolution.width == hardwareResolution.width &&
                        uniqueResolution.height == hardwareResolution.height)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (alreadyExists == false)
                {
                    uniqueResolutionsList.Add(hardwareResolution);
                }
            }

            // Now we build our Option arrays from the reversed list
            int currentIndex = 0;
            int defaultResolutionIndex = 0; // Default fallback to highest resolution (index 0)

            foreach (Resolution resolution in uniqueResolutionsList)
            {
                resolutionIndicesList.Add(currentIndex);
                resolutionNamesList.Add($"{resolution.width} x {resolution.height}");

                // Check if this specific element is 1920x1080
                if (resolution.width == 1920 && resolution.height == 1080)
                {
                    defaultResolutionIndex = currentIndex;
                }

                currentIndex++;
            }

            availableResolutions = uniqueResolutionsList.ToArray();
            OptionValues = resolutionIndicesList.ToArray();
            OptionDisplayNames = resolutionNamesList.ToArray();

            // Set the dynamic default index
            DefaultValue = defaultResolutionIndex;
        }

        public override void ApplySettingLogic()
        {
            if (availableResolutions == null || CurrentValue < 0 || CurrentValue >= availableResolutions.Length)
            {
                return;
            }

            Resolution targetResolution = availableResolutions[CurrentValue];
            Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreenMode);
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor() => GenerateResolutions();
#endif
    }
}