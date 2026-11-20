namespace AbstractPixel.Core.Editor
{
    /// <summary>One scene's status as read from the project: the status, its fix, the scene's name, path and GUID, and the message.</summary>
    public class SceneStatusReport
    {
        public SceneStatus Status { get; }
        public SceneFix Fix { get; }
        public string SceneName { get; }
        public string ScenePath { get; }
        public string SceneGuid { get; }
        public string Message { get; }

        public SceneStatusReport(SceneStatus _status, SceneFix _fix, string _sceneName, string _scenePath, string _sceneGuid, string _message)
        {
            Status = _status;
            Fix = _fix;
            SceneName = _sceneName;
            ScenePath = _scenePath;
            SceneGuid = _sceneGuid;
            Message = _message;
        }

        public bool HasFix
        {
            get { return Fix != SceneFix.None; }
        }

        public string FixLabel
        {
            get { return SceneStatusRules.FixLabel(Fix); }
        }
    }
}
