using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Stores text under keys. A key is a name such as <c>GameProfiles/a1b2c3/GameProfile</c>:
    /// forward slashes, letters, digits, <c>-</c> and <c>_</c>, with no file extension.
    /// A backup key starts with <c>Backups/</c>. The storage service decides where the text really goes.
    /// </summary>
    public interface IDataStorageService
    {
        /// <summary>Writes the text. A write either fully succeeds or leaves the previous text untouched.</summary>
        bool Write(string key, string text);

        bool TryRead(string key, out string text);

        bool Exists(string key);

        bool Delete(string key);

        /// <summary>Removes every key that starts with the prefix.</summary>
        bool DeleteAll(string keyPrefix);

        IReadOnlyList<string> ListKeys(string keyPrefix);
    }
}
