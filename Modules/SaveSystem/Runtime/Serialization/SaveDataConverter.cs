using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public static class SaveDataConverter
    {
        public static object Convert(object data, Type targetType)
        {
            if (data == null) return null;

            if (targetType.IsAssignableFrom(data.GetType())) return data;

            if (data is JObject jObject)
            {
                return jObject.ToObject(targetType);
            }

            if(data is JArray jArray)
            {
                return jArray.ToObject(targetType);
            }

            //Fallback 
            try
            {
                return System.Convert.ChangeType(data, targetType);
            }
            catch(Exception ex)
            {
                    Debug.LogError($"SaveDataConverter: Failed to convert data of type {data.GetType()} to target type" +
                        $" {targetType}. Exception: {ex.Message}");
                    return default;
            }
        }

        public static T Convert<T>(object data)
        {
            return (T)Convert(data, typeof(T));
        }
    }
}