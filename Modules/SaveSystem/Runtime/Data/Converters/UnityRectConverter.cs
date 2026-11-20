using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    internal sealed class UnityRectConverter : JsonConverter<Rect>
    {
        public override void WriteJson(JsonWriter _writer, Rect _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("x");
            _writer.WriteValue(_value.x);
            _writer.WritePropertyName("y");
            _writer.WriteValue(_value.y);
            _writer.WritePropertyName("width");
            _writer.WriteValue(_value.width);
            _writer.WritePropertyName("height");
            _writer.WriteValue(_value.height);
            _writer.WriteEndObject();
        }

        public override Rect ReadJson(JsonReader _reader, Type _objectType, Rect _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Rect.zero;
            }

            JObject jsonObject = JObject.Load(_reader);
            float positionX = jsonObject["x"] != null ? jsonObject["x"].Value<float>() : 0f;
            float positionY = jsonObject["y"] != null ? jsonObject["y"].Value<float>() : 0f;
            float width = jsonObject["width"] != null ? jsonObject["width"].Value<float>() : 0f;
            float height = jsonObject["height"] != null ? jsonObject["height"].Value<float>() : 0f;

            return new Rect(positionX, positionY, width, height);
        }
    }
}
