using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    internal sealed class UnityVector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter _writer, Vector2Int _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("x");
            _writer.WriteValue(_value.x);
            _writer.WritePropertyName("y");
            _writer.WriteValue(_value.y);
            _writer.WriteEndObject();
        }

        public override Vector2Int ReadJson(JsonReader _reader, Type _objectType, Vector2Int _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Vector2Int.zero;
            }

            JObject jsonObject = JObject.Load(_reader);
            int positionX = jsonObject["x"] != null ? jsonObject["x"].Value<int>() : 0;
            int positionY = jsonObject["y"] != null ? jsonObject["y"].Value<int>() : 0;

            return new Vector2Int(positionX, positionY);
        }
    }
}
