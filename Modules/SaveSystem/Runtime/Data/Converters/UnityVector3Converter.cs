using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public sealed class UnityVector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter _writer, Vector3 _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("x");
            _writer.WriteValue(_value.x);
            _writer.WritePropertyName("y");
            _writer.WriteValue(_value.y);
            _writer.WritePropertyName("z");
            _writer.WriteValue(_value.z);
            _writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader _reader, Type _objectType, Vector3 _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Vector3.zero;
            }

            JObject jsonObject = JObject.Load(_reader);
            float positionX = jsonObject["x"] != null ? jsonObject["x"].Value<float>() : 0f;
            float positionY = jsonObject["y"] != null ? jsonObject["y"].Value<float>() : 0f;
            float positionZ = jsonObject["z"] != null ? jsonObject["z"].Value<float>() : 0f;

            return new Vector3(positionX, positionY, positionZ);
        }
    }
}