using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public sealed class UnityQuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter _writer, Quaternion _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("x");
            _writer.WriteValue(_value.x);
            _writer.WritePropertyName("y");
            _writer.WriteValue(_value.y);
            _writer.WritePropertyName("z");
            _writer.WriteValue(_value.z);
            _writer.WritePropertyName("w");
            _writer.WriteValue(_value.w);
            _writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader _reader, Type _objectType, Quaternion _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Quaternion.identity;
            }

            JObject jsonObject = JObject.Load(_reader);
            float rotationX = jsonObject["x"] != null ? jsonObject["x"].Value<float>() : 0f;
            float rotationY = jsonObject["y"] != null ? jsonObject["y"].Value<float>() : 0f;
            float rotationZ = jsonObject["z"] != null ? jsonObject["z"].Value<float>() : 0f;
            float rotationW = jsonObject["w"] != null ? jsonObject["w"].Value<float>() : 1f;

            // [MODIFIED]: Defensive check against (0,0,0,0) corrupting engine transform matrices
            if (rotationX == 0f && rotationY == 0f && rotationZ == 0f && rotationW == 0f)
            {
                return Quaternion.identity;
            }

            return new Quaternion(rotationX, rotationY, rotationZ, rotationW);
        }
    }
}