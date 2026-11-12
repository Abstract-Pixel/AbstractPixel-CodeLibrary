using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public class JsonSerializer : ISerializer
    {
        JsonSerializerSettings settings;
        // The same settings as a Newtonsoft serializer, for converting to and from JToken.
        Newtonsoft.Json.JsonSerializer tokenSerializer;

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
            tokenSerializer = Newtonsoft.Json.JsonSerializer.Create(settings);
        }
        public bool TrySerialize<T>(T _data, out string _output)
        {
            if (_data == null)
            {
                Debug.LogError("JsonSerializer: Cannot serialize null data.");
                _output = null;
                return false;
            }
            if (_data is UnityEngine.Object)
            {
                Debug.LogError("JsonSerializer: Cannot serialize UnityEngine.Object types.");
                _output = null;
                return false;
            }

            try
            {
                _output = JsonConvert.SerializeObject(_data, settings);
                return true;

            }
            catch (Exception e)
            {
                Debug.LogError($"JsonSerializer: Serialization failed with exception: {e.Message} {e.StackTrace}");
                _output = null;
                return false;
            }

        }
        public JToken ToToken(object data)
        {
            return JToken.FromObject(data, tokenSerializer);
        }

        public object FromToken(JToken token, Type type)
        {
            return token.ToObject(type, tokenSerializer);
        }

        // Returns false without logging: only the caller knows which save key the text came from, so it logs.
        public bool TryDeserialize<T>(string _data, out T _output)
        {
            _output = default;
            if (string.IsNullOrEmpty(_data))
            {
                return false;
            }
            try
            {
                _output = JsonConvert.DeserializeObject<T>(_data, settings);
                // The text "null" parses without error but holds no data.
                return _output != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
