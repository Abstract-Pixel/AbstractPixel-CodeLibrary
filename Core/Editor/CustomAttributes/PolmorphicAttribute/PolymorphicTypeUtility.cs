using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AbstractPixel.Core.Editor
{
    public static class PolymorphicTypeUtility
    {

        public static List<Type> GetPropertyCompatibleTypes(SerializedProperty _property)
        {
            Type propertyType = GetPropertyTypeFromManagedReferenceFieldTypeName(_property);
            TypeCache.TypeCollection potentialTypes = TypeCache.GetTypesDerivedFrom(propertyType);
            List<Type> filteredTypes = potentialTypes.Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericType && x.IsSerializable)
                                                      .Where(x => x.GetCustomAttribute<SerializableAttribute>() != null)
                                                      .OrderBy(type => type.Name)
                                                      .ToList();
            return filteredTypes;
        }

        public static Type GetPropertyTypeFromManagedReferenceFieldTypeName(SerializedProperty _property)
        {
            if (_property == null) return null;
            string[] splitPrportyParts = _property.managedReferenceFieldTypename.Split(" ");
            if (splitPrportyParts.Length != 2) return null;
            string assemblyName = splitPrportyParts[0];
            string typeName = splitPrportyParts[1];
            string properFormatedName = $"{typeName}, {assemblyName}";
            Type finalType = Type.GetType(properFormatedName);
            return finalType;
        }

        public static Type GetPropertyFromManagedReferenceFullTypeName(SerializedProperty _property)
        {
            if (_property == null) return null;
            string[] splitPrportyParts = _property.managedReferenceFullTypename.Split(" ");
            if (splitPrportyParts.Length != 2) return null;
            string assemblyName = splitPrportyParts[0];
            string typeName = splitPrportyParts[1];
            string properFormatedName = $"{typeName}, {assemblyName}";
            Type finalType = Type.GetType(properFormatedName);
            return finalType;

        }

    }
}