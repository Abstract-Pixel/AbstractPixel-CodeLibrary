using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Turns save data into JSON text and back. The only serializer; the save system always writes JSON.</summary>
    internal sealed class SaveJson
    {
        JsonSerializerSettings settings;
        // The same settings as a Newtonsoft serializer, for converting to and from JToken.
        JsonSerializer tokenSerializer;

        public SaveJson()
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
                // NONE: each saved object restores into its own known type (see FromToken)
                TypeNameHandling = TypeNameHandling.None,

                // A field renamed or removed between builds is skipped, so older save files still load.
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            // Vector3 and Quaternion are written as plain x, y, z (and w) numbers. The converters live in these
            // settings only, never in JsonConvert.DefaultSettings, so nothing global changes when the save system runs.
            settings.Converters.Add(new UnityVector3Converter());
            settings.Converters.Add(new UnityQuaternionConverter());
            tokenSerializer = JsonSerializer.Create(settings);
        }
        public bool TrySerialize<T>(T _data, out string _output)
        {
            if (_data == null)
            {
                Debug.LogError("[SaveSystem] SaveJson: Cannot serialize null data.");
                _output = null;
                return false;
            }
            if (_data is UnityEngine.Object)
            {
                Debug.LogError("[SaveSystem] SaveJson: Cannot serialize UnityEngine.Object types.");
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
                Debug.LogError($"[SaveSystem] SaveJson: Serialization failed with exception: {e.Message} {e.StackTrace}");
                _output = null;
                return false;
            }

        }
        /// <summary>Turns captured data into a detached JSON value, so later changes to the object don't change it.</summary>
        public JToken ToToken(object data)
        {
            return JToken.FromObject(data, tokenSerializer);
        }

        /// <summary>Turns a stored JSON value back into the given type. Throws if the data doesn't fit the type.</summary>
        public object FromToken(JToken token, Type type)
        {
            return token.ToObject(type, tokenSerializer);
        }

        // Returns false without logging: only the caller knows which save key the text came from, so it logs.
        /// <summary>Returns false for empty, corrupt or "null" text. It doesn't log, so the caller can name the save key.</summary>
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
