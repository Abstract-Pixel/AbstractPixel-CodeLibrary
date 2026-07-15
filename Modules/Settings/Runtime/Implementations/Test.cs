using AbstractPixel.Core.AbstractPixel.Core;
using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public class Test : MonoBehaviour
    {
        [SerializeField]PolymorphicType<ISettingBackend> settingType;
    }
}
