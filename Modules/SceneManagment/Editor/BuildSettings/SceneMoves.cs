using System.Collections.Generic;
using System.Text;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Moves the scenes of scene groups to Addressables or Build Settings (decision Q4). Each move removes the other
    /// entry, through the scene field's own fix. Without the Addressables package nothing moves to Addressables.
    /// </summary>
    public static class SceneMoves
    {
        public const string NeedsAddressablesTooltip = "Needs the Addressables package (com.unity.addressables).";
        private const string LogPrefix = "[Scene system]";
        private const int ScenesListedInConfirmation = 30;

        /// <summary>Every scene of these groups (main, manager and shared) that can move to the place, once each.</summary>
        public static List<SceneReference> ScenesToMove(IEnumerable<SceneGroup> _groups, ScenePlace _place)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            HashSet<string> seenPaths = new HashSet<string>();
            foreach (SceneGroup group in _groups)
            {
                if (group == null)
                {
                    continue;
                }
                foreach (SceneReference scene in ScenesOf(group))
                {
                    SceneStatusReport report = SceneStatusReader.Read(scene.SceneFieldReference);
                    if (!SceneStatusReader.CanMoveTo(report, _place))
                    {
                        continue;
                    }
                    if (!seenPaths.Add(report.ScenePath))
                    {
                        continue;
                    }
                    scenes.Add(scene);
                }
            }
            return scenes;
        }

        public static void MoveGroupToAddressables(SceneGroup _group)
        {
            List<SceneReference> scenes = ScenesToMove(new[] { _group }, ScenePlace.Addressables);
            if (scenes.Count == 0)
            {
                Debug.Log(LogPrefix + " Scene group '" + _group.name + "': every scene is already Addressable (or Missing).", _group);
                return;
            }
            Move(scenes, ScenePlace.Addressables);
        }

        /// <summary>"Move all scenes of all groups to Addressables…", after a confirmation that lists the scenes.</summary>
        public static void MoveAllGroupsToAddressablesAfterConfirmation()
        {
            List<SceneReference> scenes = ScenesToMove(SceneGroupAssets.FindAll(), ScenePlace.Addressables);
            if (scenes.Count == 0)
            {
                EditorUtility.DisplayDialog("Move scenes to Addressables", "Every scene of every scene group is already Addressable (or Missing).", "OK");
                return;
            }
            string message = ConfirmationText(scenes);
            bool confirmed = EditorUtility.DisplayDialog("Move scenes to Addressables", message, "Move " + scenes.Count + " scene(s)", "Cancel");
            if (!confirmed)
            {
                return;
            }
            Move(scenes, ScenePlace.Addressables);
        }

        public static string ConfirmationText(List<SceneReference> _scenes)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("These scenes get an Addressables entry and leave Build Settings, so each loads from one place:");
            text.AppendLine();
            int listed = 0;
            foreach (SceneReference scene in _scenes)
            {
                if (listed == ScenesListedInConfirmation)
                {
                    text.AppendLine("…and " + (_scenes.Count - listed) + " more.");
                    break;
                }
                text.AppendLine("• " + scene.SceneName);
                listed++;
            }
            return text.ToString();
        }

        private static void Move(List<SceneReference> _scenes, ScenePlace _place)
        {
            foreach (SceneReference scene in _scenes)
            {
                SceneStatusReport report = SceneStatusReader.Read(scene.SceneFieldReference);
                SceneStatusReader.MoveTo(report, _place);
            }
        }

        private static IEnumerable<SceneReference> ScenesOf(SceneGroup _group)
        {
            if (_group.MainScene != null)
            {
                yield return _group.MainScene;
            }
            if (_group.ManagerialBootScenesList != null)
            {
                foreach (SceneReference scene in _group.ManagerialBootScenesList)
                {
                    if (scene != null)
                    {
                        yield return scene;
                    }
                }
            }
            if (_group.ContextualBootScenesList != null)
            {
                foreach (SceneReference scene in _group.ContextualBootScenesList)
                {
                    if (scene != null)
                    {
                        yield return scene;
                    }
                }
            }
        }
    }
}
