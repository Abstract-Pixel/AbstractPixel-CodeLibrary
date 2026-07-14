using System;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// DropDown property to choose a Setting Type Inheriting from BaseSetting<T>
    /// </summary>
    [Serializable]
    public class SettingTypeReference
    {
        private string targetClassName;

        public Type GetSettingTargetType()
        {
            if (string.IsNullOrEmpty(targetClassName))
            {
                return null;
            }

            Assembly[] allAssembliesArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in allAssembliesArray)
            {
                Type[] allTypesInThisAssembly = assembly.GetTypes();
                foreach (Type type in allTypesInThisAssembly)
                {
                    if (type.FullName == targetClassName)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        public void SetSettingTargetType(Type _targetType)
        {
            if(_targetType == null)
            {
                return;
            }
            targetClassName = _targetType.Name;
        }
    }
}