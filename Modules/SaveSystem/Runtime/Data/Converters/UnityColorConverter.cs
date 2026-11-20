using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    internal sealed class UnityColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter _writer, Color _value, Newtonsoft.Json.JsonSerializer _serializer)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("r");
            _writer.WriteValue(_value.r);
            _writer.WritePropertyName("g");
            _writer.WriteValue(_value.g);
            _writer.WritePropertyName("b");
            _writer.WriteValue(_value.b);
            _writer.WritePropertyName("a");
            _writer.WriteValue(_value.a);
            _writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader _reader, Type _objectType, Color _existingValue, bool _hasExistingValue, Newtonsoft.Json.JsonSerializer _serializer)
        {
            if (_reader.TokenType == JsonToken.Null)
            {
                return Color.black;
            }

            JObject jsonObject = JObject.Load(_reader);
            float red = jsonObject["r"] != null ? jsonObject["r"].Value<float>() : 0f;
            float green = jsonObject["g"] != null ? jsonObject["g"].Value<float>() : 0f;
            float blue = jsonObject["b"] != null ? jsonObject["b"].Value<float>() : 0f;
            float alpha = jsonObject["a"] != null ? jsonObject["a"].Value<float>() : 1f;

            return new Color(red, green, blue, alpha);
        }
    }
}
