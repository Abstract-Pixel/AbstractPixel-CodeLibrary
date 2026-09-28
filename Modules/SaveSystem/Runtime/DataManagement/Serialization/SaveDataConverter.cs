using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public static class SaveDataConverter
    {
        public static object Convert(object _data, Type _targetType)
        {
            if (_data == null) return null;

            if (_targetType.IsAssignableFrom(_data.GetType())) return _data;

            if (_data is JObject jObject)
            {
                // [MODIFIED]: Safe fallback prevents NullReferenceExceptions if schema drifted
                try
                {
                    return jObject.ToObject(_targetType);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[SaveDataConverter] Deserialization fallback for {_targetType}: {exception.Message}");
                    return Activator.CreateInstance(_targetType);
                }
            }

            if (_data is JArray jArray)
            {
                return jArray.ToObject(_targetType);
            }

            try
            {
                return System.Convert.ChangeType(_data, _targetType);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveDataConverter] Conversion failed for {_targetType}: {exception.Message}");
                return Activator.CreateInstance(_targetType);
            }
        }

        public static T Convert<T>(object _data)
        {
            return (T)Convert(_data, typeof(T));
        }

         // [MODIFIED]: Exception-proof default instance factory
        private static object CreateSafeDefault(Type _type)
        {
            if (_type == typeof(string)) return string.Empty;
            if (_type.IsValueType) return Activator.CreateInstance(_type);

            if (_type.GetConstructor(Type.EmptyTypes) != null)
            {
                return Activator.CreateInstance(_type);
            }

            return null;
        }
    }
}