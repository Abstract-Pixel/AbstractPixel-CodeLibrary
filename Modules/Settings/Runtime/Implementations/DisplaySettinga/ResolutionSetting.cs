using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ResolutionSetting : IntOptionsSetting
    {
        private Resolution[] availableResolutions;

        public override void Initialize()
        {
            GenerateResolutions();
            base.Initialize();
        }

        private void GenerateResolutions()
        {
            Resolution[] rawHardwareResolutions = Screen.resolutions;
            
            List<Resolution> uniqueResolutionsList = new List<Resolution>();
            List<int> resolutionIndicesList = new List<int>();
            List<string> resolutionNamesList = new List<string>();

            int currentIndex = 0;

            foreach (Resolution hardwareResolution in rawHardwareResolutions)
            {
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
                    
                    resolutionIndicesList.Add(currentIndex);
                    resolutionNamesList.Add($"{hardwareResolution.width} x {hardwareResolution.height}");
                    
                    currentIndex++;
                }
            }

            availableResolutions = uniqueResolutionsList.ToArray();
            
            // Because we changed it to 'protected set', we can assign these directly now!
            OptionValues = resolutionIndicesList.ToArray();
            OptionDisplayNames = resolutionNamesList.ToArray();
        }

        public override void ApplyLogic()
        {
            if (availableResolutions == null || CurrentValue < 0 || CurrentValue >= availableResolutions.Length)
            {
                return;
            }

            Resolution targetResolution = availableResolutions[CurrentValue];
            
            Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreenMode);
        }

#if UNITY_EDITOR
        public override void ValidateInEditor(bool _forceRevalidation=false)
        {
            base.ValidateInEditor();
            if(CanProceedWithValidation(_forceRevalidation))
            {
                GenerateResolutions();
            }
        }
#endif
    }
}