using System;
using Newtonsoft.Json.Linq;

namespace AbstractPixel.SaveSystem
{
    public interface ISerializer
    {
        bool TrySerialize<T>(T _data, out string _output);
        /// <summary>Returns false for empty, corrupt or "null" text. It doesn't log, so the caller can name the save key.</summary>
        bool TryDeserialize<T>(string _data, out T _output);

        /// <summary>Turns captured data into a detached JSON value, so later changes to the object don't change it.</summary>
        JToken ToToken(object data);

        /// <summary>Turns a stored JSON value back into the given type. Throws if the data doesn't fit the type.</summary>
        object FromToken(JToken token, Type type);
    }
}
