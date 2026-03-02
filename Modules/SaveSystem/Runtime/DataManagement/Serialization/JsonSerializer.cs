using UnityEngine;
using Newtonsoft.Json;
using System;

namespace AbstractPixel.SaveSystem
{
    public class JsonSerializer : ISerializer
    {
        JsonSerializerSettings settings;

        public JsonSerializer()
        {
            settings = new JsonSerializerSettings()
            {
                // READABILITY (Debug vs Release) LATER : have this as a option in config
                Formatting = Formatting.Indented,

                // SAFETY (Prevents Unity Crashes)
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                // OPTIMIZATION (Disk Space)
                NullValueHandling = NullValueHandling.Ignore,

                // LIST LOGIC (Prevents Duplicate Items)
                ObjectCreationHandling = ObjectCreationHandling.Replace,

                // TYPE HANDLING (The "Bridge" Strategy)
                // NONE because SaveDataConverter + SaveableBridge is used
                TypeNameHandling = TypeNameHandling.None
            };
        }
        public bool TrySerialize<T>(T _data, out string _output)
        {
            if (_data == null)
            {
                Debug.LogError("JsonSerializer: Cannot serialize null data.");
                _output = null;
                return false;
            }
            if(_data is UnityEngine.Object)
            {
                Debug.LogError("JsonSerializer: Cannot serialize UnityEngine.Object types.");
                _output = null;
                return false;
            }

            try
            {
                _output = JsonConvert.SerializeObject(_data,settings);
                return true;

            }
            catch (Exception e)
            {
                Debug.LogError($"JsonSerializer: Serialization failed with exception: {e.Message} {e.StackTrace}");
                _output = null;
                return false;
            }

        }
        public bool TryDeserialize<T>(string _data, out T _output)
        {
            if (string.IsNullOrEmpty(_data))
            {
                Debug.LogError("JsonSerializer: Cannot deserialize null or empty string.");
                _output = default;
                return false;
            }
            try
            {
                _output = JsonConvert.DeserializeObject<T>(_data,settings);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"JsonSerializer: Deserialization failed with exception: {e.Message} {e.StackTrace}");
                _output = default;
                return false;
            }
        }
    }
}
