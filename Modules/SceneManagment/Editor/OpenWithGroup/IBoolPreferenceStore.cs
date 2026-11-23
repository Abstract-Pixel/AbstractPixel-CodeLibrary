namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Where on/off preferences are kept: EditorPrefs in the Editor, a dictionary in tests.</summary>
    public interface IBoolPreferenceStore
    {
        bool HasKey(string _key);
        bool GetBool(string _key, bool _defaultValue);
        void SetBool(string _key, bool _value);
    }
}
