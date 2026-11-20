using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// One thing wrong with a scene group: what it is, where it is (list and entry), the scene, a message naming the
    /// group and the scene, and its one-click fix.
    /// </summary>
    public class SceneGroupProblem
    {
        public SceneGroupProblemKind Kind { get; }
        public SceneGroupProblemFix Fix { get; }
        public SceneGroupListKind List { get; }

        /// <summary>The entry in the manager or shared scenes list; 0 for the main scene.</summary>
        public int Index { get; }

        /// <summary>The scene the problem is about; null for a group with no main scene.</summary>
        public SceneReference Scene { get; }
        public string Message { get; }

        public SceneGroupProblem(SceneGroupProblemKind _kind, SceneGroupProblemFix _fix, SceneGroupListKind _list, int _index, SceneReference _scene, string _message)
        {
            Kind = _kind;
            Fix = _fix;
            List = _list;
            Index = _index;
            Scene = _scene;
            Message = _message;
        }

        public bool HasFix
        {
            get { return Fix != SceneGroupProblemFix.None; }
        }
    }
}
