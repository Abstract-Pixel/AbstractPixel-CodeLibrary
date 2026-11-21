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

        /// <summary>The object a Select fix selects; null for scene problems.</summary>
        public UnityEngine.Object Target { get; }

        /// <summary>
        /// False for warnings, such as relocation setup mistakes, which show in the group check but do not stop a
        /// build: the build check cannot see inside scenes that are not open.
        /// </summary>
        public bool BlocksBuild { get; }

        public SceneGroupProblem(SceneGroupProblemKind _kind, SceneGroupProblemFix _fix, SceneGroupListKind _list, int _index, SceneReference _scene, string _message)
        {
            Kind = _kind;
            Fix = _fix;
            List = _list;
            Index = _index;
            Scene = _scene;
            Message = _message;
            BlocksBuild = true;
        }

        /// <summary>A warning about an object in an open scene, fixed by selecting it.</summary>
        public static SceneGroupProblem Warning(SceneGroupProblemKind _kind, UnityEngine.Object _target, string _message)
        {
            return new SceneGroupProblem(_kind, _target, _message);
        }

        private SceneGroupProblem(SceneGroupProblemKind _kind, UnityEngine.Object _target, string _message)
        {
            Kind = _kind;
            Fix = SceneGroupProblemFix.None;
            if (_target != null)
            {
                Fix = SceneGroupProblemFix.Select;
            }
            List = SceneGroupListKind.MainScene;
            Message = _message;
            Target = _target;
            BlocksBuild = false;
        }

        public bool HasFix
        {
            get { return Fix != SceneGroupProblemFix.None; }
        }
    }
}
