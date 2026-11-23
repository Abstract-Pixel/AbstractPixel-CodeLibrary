using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>One scene set as the scene picker reads it: its name and the GUIDs of its manager and shared scenes.</summary>
    public class ScenePickerSetInfo
    {
        public string Name;
        public string Guid = string.Empty;
        public List<string> SceneGuids = new List<string>();

        public ScenePickerSetInfo(string _name)
        {
            Name = _name;
        }

        public ScenePickerSetInfo(string _name, string _guid)
        {
            Name = _name;
            Guid = _guid ?? string.Empty;
        }
    }
}
