using AbstractPixel.Core;
using AbstractPixel.Core.AbstractPixel.Core;
using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public class Test : MonoBehaviour
    {
        [SerializeField] PolymorphicType<ISettingBackend> settingType;
        [SerializeReference, Polymorphic] ISettingBackend settingBackend;
        [SerializeReference, Polymorphic] ISettingBackend settingBros;
    }
}
