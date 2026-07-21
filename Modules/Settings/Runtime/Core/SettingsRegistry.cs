using System.Collections.Generic;
using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [CreateAssetMenu(fileName = "SettingsRegistry", menuName = "Settings/Settings Registry")]
    public class SettingsRegistry : ScriptableObject
    {
        public List<SettingsCategoryGroup> AllSettingsList = new List<SettingsCategoryGroup>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (AllSettingsList == null)
            {
                return;
            }

            foreach (SettingsCategoryGroup group in AllSettingsList)
            {
                if (group.Settings == null)
                {
                    continue;
                }

                foreach (ISettingBackend setting in group.Settings)
                {
                    if (setting != null)
                    {
                        setting.ValidateInEditor(false);
                    }
                }
            }
        }
#endif
    }
}