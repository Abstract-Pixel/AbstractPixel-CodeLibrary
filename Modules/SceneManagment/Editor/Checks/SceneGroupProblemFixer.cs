using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Applies a group problem's one-click fix, with Undo and one console line naming the group and the scene.</summary>
    public static class SceneGroupProblemFixer
    {
        private const string LogPrefix = "[Scene system]";

        public static string FixLabel(SceneGroupProblemFix _fix)
        {
            switch (_fix)
            {
                case SceneGroupProblemFix.AddToBuild:
                    return SceneStatusRules.FixLabel(SceneFix.AddToBuild);
                case SceneGroupProblemFix.MakeAddressable:
                    return SceneStatusRules.FixLabel(SceneFix.MakeAddressable);
                case SceneGroupProblemFix.RemoveFromGroup:
                    return "Remove from group";
                default:
                    return string.Empty;
            }
        }

        public static string FixTooltip(SceneGroupProblemFix _fix)
        {
            switch (_fix)
            {
                case SceneGroupProblemFix.AddToBuild:
                    return "Adds this scene to Build Settings and removes its Addressables entry, so it loads from one place.";
                case SceneGroupProblemFix.MakeAddressable:
                    return "Gives this scene an Addressables entry and removes it from Build Settings, so it loads from one place.";
                case SceneGroupProblemFix.RemoveFromGroup:
                    return "Removes this entry from the scene group. Undo brings it back.";
                default:
                    return string.Empty;
            }
        }

        public static void Apply(SceneGroup _group, SceneGroupProblem _problem)
        {
            if (_group == null || _problem == null)
            {
                return;
            }
            switch (_problem.Fix)
            {
                case SceneGroupProblemFix.AddToBuild:
                case SceneGroupProblemFix.MakeAddressable:
                    SceneStatusReport report = SceneStatusReader.Read(_problem.Scene.SceneFieldReference);
                    SceneStatusReader.ApplyFix(report);
                    break;
                case SceneGroupProblemFix.RemoveFromGroup:
                    RemoveFromGroup(_group, _problem);
                    break;
            }
        }

        private static void RemoveFromGroup(SceneGroup _group, SceneGroupProblem _problem)
        {
            List<SceneReference> list = ListOf(_group, _problem.List);
            bool indexIsValid = list != null && _problem.Index >= 0 && _problem.Index < list.Count;
            if (!indexIsValid)
            {
                Debug.LogError(LogPrefix + " Scene group '" + _group.name + "': can't remove " + SceneGroupCheck.Where(_problem.List, _problem.Index) + ", the entry is gone. Check the group again.", _group);
                return;
            }

            Undo.RecordObject(_group, "Remove scene from scene group");
            list.RemoveAt(_problem.Index);
            EditorUtility.SetDirty(_group);
            AssetDatabase.SaveAssetIfDirty(_group);
            Debug.Log(LogPrefix + " Scene group '" + _group.name + "': removed '" + _problem.Scene.SceneName + "' from " + SceneGroupCheck.Where(_problem.List, _problem.Index) + ".", _group);
        }

        private static List<SceneReference> ListOf(SceneGroup _group, SceneGroupListKind _list)
        {
            switch (_list)
            {
                case SceneGroupListKind.ManagerScenes:
                    return _group.ManagerialBootScenesList;
                case SceneGroupListKind.SharedScenes:
                    return _group.ContextualBootScenesList;
                default:
                    return null;
            }
        }
    }
}
