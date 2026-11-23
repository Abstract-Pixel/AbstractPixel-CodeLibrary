using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>On/off preferences kept in a dictionary, standing in for EditorPrefs.</summary>
    public sealed class FakeBoolPreferenceStore : IBoolPreferenceStore
    {
        public readonly Dictionary<string, bool> Values = new Dictionary<string, bool>();

        public bool HasKey(string _key)
        {
            return Values.ContainsKey(_key);
        }

        public bool GetBool(string _key, bool _defaultValue)
        {
            bool value;
            if (Values.TryGetValue(_key, out value))
            {
                return value;
            }
            return _defaultValue;
        }

        public void SetBool(string _key, bool _value)
        {
            Values[_key] = _value;
        }
    }
}
