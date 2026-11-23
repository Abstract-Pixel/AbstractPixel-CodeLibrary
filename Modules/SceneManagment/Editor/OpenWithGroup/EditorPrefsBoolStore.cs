using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>On/off preferences kept in EditorPrefs: per user, shared by every project on this machine.</summary>
    public sealed class EditorPrefsBoolStore : IBoolPreferenceStore
    {
        public static readonly EditorPrefsBoolStore Instance = new EditorPrefsBoolStore();

        public bool HasKey(string _key)
        {
            return EditorPrefs.HasKey(_key);
        }

        public bool GetBool(string _key, bool _defaultValue)
        {
            return EditorPrefs.GetBool(_key, _defaultValue);
        }

        public void SetBool(string _key, bool _value)
        {
            EditorPrefs.SetBool(_key, _value);
        }
    }
}
