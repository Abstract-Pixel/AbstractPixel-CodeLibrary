using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ResolutionSetting : BaseOptionsSetting<int, Resolution>
    {
        protected override void OnInitialize()
        {
            // Hardware resolutions must always be generated on boot to query the local monitor!
            GenerateResolutions();
        }

        private void GenerateResolutions()
        {
            Resolution[] rawHardwareResolutions = Screen.resolutions;

            List<Resolution> uniqueResolutionsList = new List<Resolution>();
            List<int> resolutionIndicesList = new List<int>();
            List<string> resolutionNamesList = new List<string>();

            // Iterate backwards so high resolutions are listed first
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

            int currentIndex = 0;
            int defaultResolutionIndex = 0;

            foreach (Resolution resolution in uniqueResolutionsList)
            {
                resolutionIndicesList.Add(currentIndex);
                resolutionNamesList.Add($"{resolution.width} x {resolution.height}");

                // Find 1920x1080 as default
                if (resolution.width == 1920 && resolution.height == 1080)
                {
                    defaultResolutionIndex = currentIndex;
                }

                currentIndex++;
            }

            OptionValues = uniqueResolutionsList.ToArray();
            OptionDisplayNames = resolutionNamesList.ToArray();

            DefaultValue = defaultResolutionIndex;
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            Resolution targetResolution = OptionValues[CurrentValue];
            Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreenMode);
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateResolutions();
        }
#endif
    }
}