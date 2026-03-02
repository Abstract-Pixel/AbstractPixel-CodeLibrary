using Newtonsoft.Json.Linq;
using System;

namespace AbstractPixel.SaveSystem
{
    public static class SaveDataConverter
    {
        public static object Convert(object data, Type targetType)
        {
            if(data == null) return null;

            if(targetType.IsAssignableFrom(data.GetType())) return data;

            if(data is JObject jObject)
            {
                return jObject.ToObject(targetType);
            }

            //Fallback 
            try
            {
                return System.Convert.ChangeType(data, targetType);
            }
            catch
            {
                return default;
            }
           
        }

        public static T Convert<T>(object data)
        {
            return (T)Convert(data, typeof(T));
        }
    }
}