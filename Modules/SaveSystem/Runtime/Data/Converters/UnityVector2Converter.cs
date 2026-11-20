using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    internal sealed class UnityVector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter _writer, Vector2 _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("x");
            _writer.WriteValue(_value.x);
            _writer.WritePropertyName("y");
            _writer.WriteValue(_value.y);
            _writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader _reader, Type _objectType, Vector2 _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Vector2.zero;
            }

            JObject jsonObject = JObject.Load(_reader);
            float positionX = jsonObject["x"] != null ? jsonObject["x"].Value<float>() : 0f;
            float positionY = jsonObject["y"] != null ? jsonObject["y"].Value<float>() : 0f;

            return new Vector2(positionX, positionY);
        }
    }
}
