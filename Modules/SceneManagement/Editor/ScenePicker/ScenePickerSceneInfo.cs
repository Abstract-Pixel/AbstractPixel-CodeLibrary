using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>One scene asset as the scene picker reads it from the project. Plain data, so rows can be tested.</summary>
    public class ScenePickerSceneInfo
    {
        public string Name;
        public string Path;
        public string Guid;
        public SceneStatus Status = SceneStatus.NotInBuild;

        public ScenePickerSceneInfo(string _name, string _path, string _guid)
        {
            Name = _name;
            Path = _path;
            Guid = _guid;
        }
    }
}
