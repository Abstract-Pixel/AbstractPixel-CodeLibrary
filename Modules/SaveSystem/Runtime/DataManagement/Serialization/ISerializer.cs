using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public interface ISerializer
    {
        bool TrySerialize<T>(T _data, out string _output);
        bool TryDeserialize<T>(string _data, out T _output);
    }
}
