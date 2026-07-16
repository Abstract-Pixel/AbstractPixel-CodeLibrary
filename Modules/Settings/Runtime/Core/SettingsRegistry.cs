using System.Collections.Generic;
using UnityEngine;
using AbstractPixel.Core; // Assuming this is where [Polymorphic] lives

namespace AbstractPixel.Settings 
{
    [CreateAssetMenu(fileName = "SettingsRegistry", menuName = "Settings/Settings Registry")]
    public class SettingsRegistry : ScriptableObject
    {
        [SerializeReference, Polymorphic] 
        public List<ISettingBackend> AllSettings = new List<ISettingBackend>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            AllSettings.ForEach(setting => setting.ValidateInEditor());
        }
#endif
    }
}