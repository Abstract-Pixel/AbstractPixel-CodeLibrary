using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Keeps saved text in memory instead of on disk. Tests use it so they never touch real save files.</summary>
    internal sealed class MemoryStorageService : IDataStorageService
    {
        readonly Dictionary<string, string> textByKey = new Dictionary<string, string>();

        public bool Write(string key, string text)
        {
            if (!StorageKey.IsValid(key))
            {
                return false;
            }
            textByKey[key] = text;
            return true;
        }

        public bool TryRead(string key, out string text)
        {
            return textByKey.TryGetValue(key, out text);
        }

        public bool Exists(string key)
        {
            return textByKey.ContainsKey(key);
        }

        public bool Delete(string key)
        {
            textByKey.Remove(key);
            return true;
        }

        public bool DeleteAll(string keyPrefix)
        {
            foreach (string key in ListKeys(keyPrefix))
            {
                textByKey.Remove(key);
            }
            return true;
        }

        public IReadOnlyList<string> ListKeys(string keyPrefix)
        {
            return textByKey.Keys
                .Where(key => key.StartsWith(keyPrefix, System.StringComparison.Ordinal))
                .OrderBy(key => key, System.StringComparer.Ordinal)
                .ToList();
        }
    }
}
