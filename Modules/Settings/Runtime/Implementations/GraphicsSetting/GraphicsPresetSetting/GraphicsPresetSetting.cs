using System;
using UnityEngine;
using System.Collections.Generic;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class GraphicsPresetSetting : BaseOptionsSetting<int, QualityPresetTier>
    {
        [Header("Preset Configuration")]
        [Tooltip("The index in the OptionValues array that represents 'Custom'.")]
        [SerializeField] private int customTierIndex = -1;

        // Locks to prevent infinite loops between the Preset and its child settings.
        private bool isPushingPresetToChildren = false;
        private bool isSilentlyUpdatingPresetUI = false;

        protected override void OnInitialize()
        {
            // 1. Auto-generate defaults if the developer hasn't configured them yet
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultTiers();
            }
            else
            {
                // Sync display names with the tier names configured in the inspector
                SyncDisplayNames();
            }

            // 2. Wait for all saved data to load before we check the state of the children
            SettingsActions.OnSettingsLoaded -= HandleSettingsLoaded;
            SettingsActions.OnSettingsLoaded += HandleSettingsLoaded;

            SubscribeToChildSettings();
        }

        private void GenerateDefaultTiers()
        {
            string[] unityQualityNames = QualitySettings.names;
            int totalTiers = unityQualityNames.Length + 1; // +1 for the "Custom" tier

            OptionValues = new QualityPresetTier[totalTiers];
            OptionDisplayNames = new string[totalTiers];

            // Generate a tier for every native Unity Quality Level
            for (int i = 0; i < unityQualityNames.Length; i++)
            {
                OptionValues[i] = new QualityPresetTier
                {
                    TierName = unityQualityNames[i],
                    UnityQualityIndex = i,
                    Mappings = new List<SettingOverrideMapping>()
                };
                OptionDisplayNames[i] = unityQualityNames[i];
            }

            // Append the "Custom" tier at the very end
            customTierIndex = unityQualityNames.Length;
            OptionValues[customTierIndex] = new QualityPresetTier
            {
                TierName = "Custom",
                UnityQualityIndex = unityQualityNames.Length - 1, // Baseline to the highest quality
                Mappings = new List<SettingOverrideMapping>()
            };
            OptionDisplayNames[customTierIndex] = "Custom";

            // Set a sensible default (e.g., Index 2 is usually "High", fallback to 0 if not enough tiers)
            DefaultValue = unityQualityNames.Length > 2 ? 2 : 0;
        }

        private void SyncDisplayNames()
        {
            if (OptionValues != null && OptionValues.Length > 0)
            {
                OptionDisplayNames = new string[OptionValues.Length];
                for (int i = 0; i < OptionValues.Length; i++)
                {
                    OptionDisplayNames[i] = OptionValues[i].TierName;
                }
            }
        }

        private void HandleSettingsLoaded()
        {
            SubscribeToChildSettings();
            UpdatePresetStateBasedOnChildSettings();
        }

        public override void Deconstruct()
        {
            base.Deconstruct();
            SettingsActions.OnSettingsLoaded -= HandleSettingsLoaded;
        }

        // =========================================================
        // TOP-DOWN FLOW (User selects a Preset, we push it to children)
        // =========================================================
        protected override void OnApplySettingLogic()
        {
            // If we are just changing the UI to say "Custom", do NOT push data down to the children!
            if (isSilentlyUpdatingPresetUI == true)
            {
                return;
            }

            // Safety check
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            // If the user selected "Custom", we don't force any values down. Let them tweak freely.
            if (CurrentValue == customTierIndex)
            {
                return;
            }

            // Lock the system so children don't try to talk back to us while we are updating them
            isPushingPresetToChildren = true;

            QualityPresetTier selectedTier = OptionValues[CurrentValue];

            // 1. Apply Unity's Baseline First
            QualitySettings.SetQualityLevel(selectedTier.UnityQualityIndex, true);

            // 2. Apply Custom Overrides Second (Our settings are the Ultimate Authority)
            foreach (SettingOverrideMapping mapping in selectedTier.Mappings)
            {
                if (mapping.TargetSettingType == null || mapping.TargetSettingType.TBaseType == null)
                {
                    continue;
                }

                ISettingBackend backend = SettingsManager.Instance.GetSetting(mapping.TargetSettingType.TBaseType);

                if (backend is BaseSetting<int> intSetting)
                {
                    intSetting.SetValue(mapping.IntValue);
                    intSetting.ApplySettingLogic();
                }
                else if (backend is BaseSetting<float> floatSetting)
                {
                    floatSetting.SetValue(mapping.FloatValue);
                    floatSetting.ApplySettingLogic();
                }
                else if (backend is BaseSetting<bool> boolSetting)
                {
                    boolSetting.SetValue(mapping.BoolValue);
                    boolSetting.ApplySettingLogic();
                }
                else if (backend is BaseSetting<string> stringSetting)
                {
                    stringSetting.SetValue(mapping.StringValue);
                    stringSetting.ApplySettingLogic();
                }
            }

            // Unlock the system now that we are done pushing values
            isPushingPresetToChildren = false;
        }

        // =========================================================
        // BOTTOM-UP FLOW (User changes a child setting, we update the Preset UI)
        // =========================================================
        private void SubscribeToChildSettings()
        {
            if (OptionValues == null)
            {
                return;
            }

            foreach (QualityPresetTier tier in OptionValues)
            {
                foreach (SettingOverrideMapping mapping in tier.Mappings)
                {
                    if (mapping.TargetSettingType == null || mapping.TargetSettingType.TBaseType == null)
                    {
                        continue;
                    }

                    ISettingBackend backend = SettingsManager.Instance.GetSetting(mapping.TargetSettingType.TBaseType);

                    if (backend is BaseSetting<int> intSetting)
                    {
                        intSetting.OnValueChanged -= OnChildSettingChanged;
                        intSetting.OnValueChanged += OnChildSettingChanged;
                    }
                    else if (backend is BaseSetting<float> floatSetting)
                    {
                        floatSetting.OnValueChanged -= OnChildSettingChanged;
                        floatSetting.OnValueChanged += OnChildSettingChanged;
                    }
                    else if (backend is BaseSetting<bool> boolSetting)
                    {
                        boolSetting.OnValueChanged -= OnChildSettingChanged;
                        boolSetting.OnValueChanged += OnChildSettingChanged;
                    }
                    else if (backend is BaseSetting<string> stringSetting)
                    {
                        stringSetting.OnValueChanged -= OnChildSettingChanged;
                        stringSetting.OnValueChanged += OnChildSettingChanged;
                    }
                }
            }
        }

        private void OnChildSettingChanged(int value) { UpdatePresetStateBasedOnChildSettings(); }
        private void OnChildSettingChanged(float value) { UpdatePresetStateBasedOnChildSettings(); }
        private void OnChildSettingChanged(bool value) { UpdatePresetStateBasedOnChildSettings(); }
        private void OnChildSettingChanged(string value) { UpdatePresetStateBasedOnChildSettings(); }

        /// <summary>
        /// Looks at all the individual child settings (Shadows, Textures, etc.) and checks if their 
        /// current combination matches "Low", "Medium", "High", or if it should be changed to "Custom".
        /// </summary>
        private void UpdatePresetStateBasedOnChildSettings()
        {
            // If we are currently pushing a preset down, ignore the events echoing back up to us.
            if (isPushingPresetToChildren == true)
            {
                return;
            }

            // Assume the settings are "Custom" unless we find a perfect match below
            int newPresetIndex = customTierIndex;

            // Loop through all our defined presets (Low, Medium, High, etc.)
            for (int i = 0; i < OptionValues.Length; i++)
            {
                if (i == customTierIndex)
                {
                    continue;
                }

                QualityPresetTier presetToCheck = OptionValues[i];

                if (DoesPresetTierMatchAllChildSettings(presetToCheck) == true)
                {
                    newPresetIndex = i;
                    break;
                }
            }

            // If the preset dropdown needs to change (e.g., to "Custom")
            if (CurrentValue != newPresetIndex)
            {
                // Lock the Top-Down flow so we don't accidentally overwrite the child settings
                isSilentlyUpdatingPresetUI = true;

                // DYNAMIC BASELINE MATCHING:
                // If transitioning to "Custom", swap Unity's underlying URP Asset to match 
                // the user's average choices so hidden engine settings scale properly!
                if (newPresetIndex == customTierIndex)
                {
                    int dominantBaselineIndex = CalculateDominantBaselineTierIndex();
                    QualitySettings.SetQualityLevel(dominantBaselineIndex, true);

                    // Swapping the QualityLevel resets URP overrides back to asset defaults.
                    // We must re-apply the live custom settings on top of the new baseline asset!
                    ReapplyAllLiveCustomSettings();
                }

                // Update the UI Dropdown visually to "Custom"
                SetValue(newPresetIndex);

                // Unlock
                isSilentlyUpdatingPresetUI = false;
            }
        }

        /// <summary>
        /// Calculates the average quality level of all mapped child settings (0 = Low, 1 = Med, 2 = High).
        /// Returns the nearest QualityLevel index so Unity's URP asset matches the user's overall intent.
        /// </summary>
        private int CalculateDominantBaselineTierIndex()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                return 0;
            }

            int totalScore = 0;
            int settingCount = 0;

            // Look at the first valid non-custom tier to inspect mapped settings
            QualityPresetTier sampleTier = OptionValues[0];

            foreach (SettingOverrideMapping mapping in sampleTier.Mappings)
            {
                if (mapping.TargetSettingType == null || mapping.TargetSettingType.TBaseType == null)
                {
                    continue;
                }

                ISettingBackend backend = SettingsManager.Instance.GetSetting(mapping.TargetSettingType.TBaseType);

                // Check option settings (0 = Low, 1 = Medium, 2 = High, etc.)
                if (backend is BaseSetting<int> intSetting)
                {
                    totalScore += intSetting.CurrentValue;
                    settingCount++;
                }
            }

            if (settingCount == 0)
            {
                return 0;
            }

            // Round the average to the nearest integer index
            float averageScore = (float)totalScore / settingCount;
            int roundedIndex = Mathf.RoundToInt(averageScore);

            int maxUnityQualityIndex = QualitySettings.names.Length - 1;
            return Mathf.Clamp(roundedIndex, 0, maxUnityQualityIndex);
        }

        /// <summary>
        /// Re-applies the live custom values of all mapped child settings.
        /// Called right after swapping QualitySettings.SetQualityLevel() to preserve custom tweaks.
        /// </summary>
        private void ReapplyAllLiveCustomSettings()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                return;
            }

            QualityPresetTier sampleTier = OptionValues[0];

            foreach (SettingOverrideMapping mapping in sampleTier.Mappings)
            {
                if (mapping.TargetSettingType == null || mapping.TargetSettingType.TBaseType == null)
                {
                    continue;
                }

                ISettingBackend backend = SettingsManager.Instance.GetSetting(mapping.TargetSettingType.TBaseType);

                if (backend != null)
                {
                    backend.ApplySettingLogic();
                }
            }
        }

        private bool DoesPresetTierMatchAllChildSettings(QualityPresetTier tier)
        {
            foreach (SettingOverrideMapping mapping in tier.Mappings)
            {
                if (mapping.TargetSettingType == null || mapping.TargetSettingType.TBaseType == null)
                {
                    continue;
                }

                ISettingBackend backend = SettingsManager.Instance.GetSetting(mapping.TargetSettingType.TBaseType);

                if (backend is BaseSetting<int> intSetting && intSetting.CurrentValue != mapping.IntValue)
                {
                    return false;
                }

                if (backend is BaseSetting<float> floatSetting && Mathf.Abs(floatSetting.CurrentValue - mapping.FloatValue) > 0.001f)
                {
                    return false;
                }

                if (backend is BaseSetting<bool> boolSetting && boolSetting.CurrentValue != mapping.BoolValue)
                {
                    return false;
                }

                if (backend is BaseSetting<string> stringSetting && stringSetting.CurrentValue != mapping.StringValue)
                {
                    return false;
                }
            }

            return true;
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateDefaultTiers();
        }
#endif
    }
}