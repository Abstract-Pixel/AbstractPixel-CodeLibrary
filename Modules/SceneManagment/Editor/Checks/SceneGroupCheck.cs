using System;
using System.Collections.Generic;
using System.Text;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The group check: finds what is wrong with a scene group (no main scene, a Missing scene, a scene Not in build,
    /// the same scene listed twice), each with a message naming the group and the scene and its one-click fix.
    /// The scene status comes in as a function, so the check runs in tests without assets.
    /// </summary>
    public static class SceneGroupCheck
    {
        private const string LogPrefix = "[Scene system]";

        /// <summary>The group's problems, reading each scene's status from the project.</summary>
        public static List<SceneGroupProblem> Find(SceneGroup _group)
        {
            return Find(_group, ReadStatus);
        }

        public static List<SceneGroupProblem> Find(SceneGroup _group, Func<SceneReference, SceneStatus> _statusOf)
        {
            List<SceneGroupProblem> problems = new List<SceneGroupProblem>();
            if (_group == null)
            {
                return problems;
            }

            List<SceneReference> seenScenes = new List<SceneReference>();
            SceneReference mainScene = _group.MainScene;
            bool noMainScene = IsEmpty(mainScene);
            if (noMainScene && IsBootGroup(_group))
            {
                // A boot group from before scene sets: not a place, but not a mistake either. Its scenes are still checked.
                CheckList(_group, _group.ManagerialBootScenesList, SceneGroupListKind.ManagerScenes, seenScenes, _statusOf, problems);
                CheckList(_group, _group.ContextualBootScenesList, SceneGroupListKind.SharedScenes, seenScenes, _statusOf, problems);
                return problems;
            }
            if (noMainScene)
            {
                string message = "Scene group '" + _group.name + "' has no main scene. Pick the scene that makes this place.";
                problems.Add(new SceneGroupProblem(SceneGroupProblemKind.NoMainScene, SceneGroupProblemFix.None, SceneGroupListKind.MainScene, 0, null, message));
            }
            else
            {
                CheckEntry(_group, mainScene, SceneGroupListKind.MainScene, 0, seenScenes, _statusOf, problems);
            }

            CheckList(_group, _group.ManagerialBootScenesList, SceneGroupListKind.ManagerScenes, seenScenes, _statusOf, problems);
            CheckList(_group, _group.ContextualBootScenesList, SceneGroupListKind.SharedScenes, seenScenes, _statusOf, problems);
            return problems;
        }

        /// <summary>
        /// A scene group with no main scene and no scene set that still lists manager or shared scenes: a boot group from
        /// before scene sets. Migration leaves it alone and lists it as unused. A group with a scene set always needs a
        /// main scene, so a forgotten main scene is still caught.
        /// </summary>
        public static bool IsBootGroup(SceneGroup _group)
        {
            if (_group == null || _group.SceneSet != null || !IsEmpty(_group.MainScene))
            {
                return false;
            }
            bool hasManagers = HasAnyScene(_group.ManagerialBootScenesList);
            bool hasShared = HasAnyScene(_group.ContextualBootScenesList);
            return hasManagers || hasShared;
        }

        /// <summary>
        /// The message that stops a build, naming each group asset and the scene of each problem. Empty when there are none.
        /// </summary>
        public static string BuildFailureMessage(IReadOnlyDictionary<string, List<SceneGroupProblem>> _problemsByGroupPath)
        {
            int problemCount = 0;
            StringBuilder lines = new StringBuilder();
            foreach (KeyValuePair<string, List<SceneGroupProblem>> group in _problemsByGroupPath)
            {
                if (group.Value == null)
                {
                    continue;
                }
                foreach (SceneGroupProblem problem in group.Value)
                {
                    lines.Append("\n- ").Append(group.Key).Append(": ").Append(problem.Message);
                    problemCount++;
                }
            }

            if (problemCount == 0)
            {
                return string.Empty;
            }
            string header = LogPrefix + " The build stopped: the scene group check found " + problemCount + " problem(s). "
                + "Fix them in each scene group's Inspector or in Project Settings > Abstract Pixel > Scene System > Scene Groups Debugger, "
                + "or turn off Check scene groups before a build on that page.";
            return header + lines;
        }

        /// <summary>Where a scene sits, in the words the inspector shows: "Main scene", "Shared scenes (Scene 1)". Rows count from 1.</summary>
        public static string Where(SceneGroupListKind _list, int _index)
        {
            switch (_list)
            {
                case SceneGroupListKind.ManagerScenes:
                    return "Manager scenes (Scene " + (_index + 1) + ")";
                case SceneGroupListKind.SharedScenes:
                    return "Shared scenes (Scene " + (_index + 1) + ")";
                default:
                    return "Main scene";
            }
        }

        private static void CheckList(SceneGroup _group, List<SceneReference> _scenes, SceneGroupListKind _list, List<SceneReference> _seenScenes, Func<SceneReference, SceneStatus> _statusOf, List<SceneGroupProblem> _problems)
        {
            if (_scenes == null)
            {
                return;
            }
            for (int index = 0; index < _scenes.Count; index++)
            {
                SceneReference scene = _scenes[index];
                if (IsEmpty(scene))
                {
                    continue;
                }
                CheckEntry(_group, scene, _list, index, _seenScenes, _statusOf, _problems);
            }
        }

        // The first time a scene appears its status is checked; any later entry of the same scene is a duplicate.
        private static void CheckEntry(SceneGroup _group, SceneReference _scene, SceneGroupListKind _list, int _index, List<SceneReference> _seenScenes, Func<SceneReference, SceneStatus> _statusOf, List<SceneGroupProblem> _problems)
        {
            string groupName = _group.name;
            string sceneName = _scene.SceneName;
            string where = Where(_list, _index);

            if (_seenScenes.Contains(_scene))
            {
                string message = "Scene group '" + groupName + "': scene '" + sceneName + "' is listed twice. " + where + " repeats it.";
                _problems.Add(new SceneGroupProblem(SceneGroupProblemKind.ListedTwice, SceneGroupProblemFix.RemoveFromGroup, _list, _index, _scene, message));
                return;
            }
            _seenScenes.Add(_scene);

            SceneStatus status = _statusOf(_scene);
            if (status == SceneStatus.Missing)
            {
                SceneGroupProblemFix fix = SceneGroupProblemFix.RemoveFromGroup;
                if (_list == SceneGroupListKind.MainScene)
                {
                    fix = SceneGroupProblemFix.None;
                }
                string message = "Scene group '" + groupName + "': scene '" + sceneName + "' in " + where + " is missing. There is no scene asset at '" + _scene.ScenePath + "'. Pick the scene again.";
                _problems.Add(new SceneGroupProblem(SceneGroupProblemKind.MissingScene, fix, _list, _index, _scene, message));
                return;
            }
            if (status == SceneStatus.NotInBuild)
            {
                SceneFix sceneFix = SceneStatusRules.FixFor(status, SceneStatusReader.ProjectDefaultPlace, AddressableScenes.IsInstalled);
                SceneGroupProblemFix fix = SceneGroupProblemFix.AddToBuild;
                if (sceneFix == SceneFix.MakeAddressable)
                {
                    fix = SceneGroupProblemFix.MakeAddressable;
                }
                string message = "Scene group '" + groupName + "': scene '" + sceneName + "' in " + where + " is not in build, so it can't load.";
                _problems.Add(new SceneGroupProblem(SceneGroupProblemKind.NotInBuild, fix, _list, _index, _scene, message));
            }
        }

        private static bool HasAnyScene(List<SceneReference> _scenes)
        {
            if (_scenes == null)
            {
                return false;
            }
            foreach (SceneReference scene in _scenes)
            {
                if (!IsEmpty(scene))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsEmpty(SceneReference _scene)
        {
            if (_scene == null)
            {
                return true;
            }
            bool noName = string.IsNullOrEmpty(_scene.SceneName);
            bool noGuid = string.IsNullOrEmpty(_scene.SceneGuid);
            bool noPath = string.IsNullOrEmpty(_scene.ScenePath);
            return noName && noGuid && noPath;
        }

        private static SceneStatus ReadStatus(SceneReference _scene)
        {
            SceneStatusReport report = SceneStatusReader.Read(_scene.SceneFieldReference);
            return report.Status;
        }
    }
}
