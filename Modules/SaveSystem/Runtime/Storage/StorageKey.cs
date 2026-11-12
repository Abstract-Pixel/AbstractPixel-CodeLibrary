using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Checks that a storage key is safe to turn into a file name.</summary>
    internal static class StorageKey
    {
        /// <summary>Returns true when the key is valid. Otherwise logs an error that names the key.</summary>
        public static bool IsValid(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[SaveSystem] A save key is empty. Keys are built by SavePathGenerator; check the save file name in the config.");
                return false;
            }

            // ".." or a leading "/" could point outside the save folder.
            bool leavesSaveFolder = key.Contains("..") || key.StartsWith("/") || key.EndsWith("/");
            if (leavesSaveFolder || HasInvalidCharacter(key))
            {
                Debug.LogError($"[SaveSystem] The save key '{key}' is not allowed. Use letters, digits, '-', '_' and '/' only, for example 'Global/Settings'.");
                return false;
            }

            return true;
        }

        static bool HasInvalidCharacter(string key)
        {
            foreach (char character in key)
            {
                bool isAllowed = char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == '/' || character == '.';
                if (!isAllowed)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
