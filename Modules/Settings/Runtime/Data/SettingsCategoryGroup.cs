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
        public PolymorphicList<ISettingBackend> Settings = new PolymorphicList<ISettingBackend>();
    }
}