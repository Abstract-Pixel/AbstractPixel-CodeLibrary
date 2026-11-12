namespace AbstractPixel.SaveSystem
{
    public interface ISerializer
    {
        bool TrySerialize<T>(T _data, out string _output);
        /// <summary>Returns false for empty, corrupt or "null" text. It doesn't log, so the caller can name the save key.</summary>
        bool TryDeserialize<T>(string _data, out T _output);
    }
}
