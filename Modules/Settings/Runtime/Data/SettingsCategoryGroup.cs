using System;
using System.Collections.Generic;
using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class SettingsCategoryGroup
    {
        [ReadOnly(true)]public string GroupName;
        public SettingCategory Category;

        [SerializeReference, Polymorphic] 
        public List<ISettingBackend> Settings = new List<ISettingBackend>();
    }
}