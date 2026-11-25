using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Turns scene groups without a scene set into groups that use one (decision Q8): one scene set per distinct
    /// (manager scenes, shared scenes, Reset shared scenes) combination among groups that have a main scene. Each group
    /// points at its set with no overrides. Groups without a main scene are listed, never deleted. A markdown report next
    /// to the scene system settings asset lists every group before and after.
    /// </summary>
    public static class SceneGroupMigration
    {
        public const string ReportFileName = "Scene group migration report.md";
        private const string SetsFolderName = "Scene Sets";
        private const string LogPrefix = "[Scene system]";

        public static SceneGroupMigrationPlan Plan(IReadOnlyList<SceneGroup> _groups)
        {
            SceneGroupMigrationPlan plan = new SceneGroupMigrationPlan();
            if (_groups == null)
            {
                return plan;
            }

            Dictionary<string, int> setIndexByKey = new Dictionary<string, int>();
            foreach (SceneGroup group in _groups)
            {
                if (group == null)
                {
                    continue;
                }
                if (group.SceneSet != null)
                {
                    plan.GroupsAlreadyInASet.Add(group);
                    continue;
                }
                if (!HasMainScene(group))
                {
                    plan.GroupsWithoutMainScene.Add(group);
                    continue;
                }

                string key = CombinationKey(group);
                if (!setIndexByKey.TryGetValue(key, out int setIndex))
                {
                    setIndex = plan.GroupsBySet.Count;
                    setIndexByKey[key] = setIndex;
                    plan.GroupsBySet.Add(new List<SceneGroup>());
                }
                plan.GroupsBySet[setIndex].Add(group);
            }

            NameSets(plan);
            plan.DefaultSetIndex = IndexOfLargestSet(plan);
            return plan;
        }

        /// <summary>
        /// A set's name from its groups' main scenes: their common first words ("Level 2", "Level 13" give "Level";
        /// "MainMenu" gives "Main Menu"), or "Scene set N" when they share none.
        /// </summary>
        public static string SetName(IReadOnlyList<string> _mainSceneNames, int _index)
        {
            string fallback = "Scene set " + (_index + 1);
            if (_mainSceneNames == null || _mainSceneNames.Count == 0)
            {
                return fallback;
            }

            List<string> readableNames = new List<string>();
            foreach (string name in _mainSceneNames)
            {
                string spaced = (name ?? string.Empty).Replace('_', ' ').Replace('-', ' ');
                readableNames.Add(ObjectNames.NicifyVariableName(spaced));
            }

            string commonStart = CommonWholeWordStart(readableNames);
            string trimmed = commonStart.TrimEnd(' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '(', ')');
            if (trimmed.Length < 2)
            {
                return fallback;
            }
            return trimmed;
        }

        /// <summary>The report in markdown. &lt;paramref name="_referencesOf"/&gt; names the files that still use a group without a main scene.</summary>
        public static string Report(SceneGroupMigrationPlan _plan, bool _isPreview, Func<SceneGroup, string> _whereIsGroup, Func<SceneGroup, string> _referencesOf)
        {
            StringBuilder report = new StringBuilder();
            string title = _isPreview ? "# Scene group migration (Preview: nothing changed yet)" : "# Scene group migration";
            report.AppendLine(title);
            report.AppendLine();
            report.AppendLine("Written " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ". "
                + _plan.SetNames.Count + " scene set(s) for " + _plan.GroupsToMigrateCount + " scene group(s); "
                + _plan.GroupsWithoutMainScene.Count + " scene group(s) without a main scene are left as they are.");
            report.AppendLine();

            report.AppendLine("## Scene sets");
            report.AppendLine();
            for (int index = 0; index < _plan.SetNames.Count; index++)
            {
                SceneGroup sample = _plan.GroupsBySet[index][0];
                string defaultMark = index == _plan.DefaultSetIndex ? " (default)" : string.Empty;
                report.AppendLine("- **" + _plan.SetNames[index] + "**" + defaultMark + ": " + DescribeParts(sample) + ". Used by " + _plan.GroupsBySet[index].Count + " scene group(s).");
            }
            report.AppendLine();

            report.AppendLine("## Scene groups");
            report.AppendLine();
            report.AppendLine("| Scene group | Main scene | Before | After |");
            report.AppendLine("|---|---|---|---|");
            for (int index = 0; index < _plan.SetNames.Count; index++)
            {
                foreach (SceneGroup group in _plan.GroupsBySet[index])
                {
                    string after = "Scene set '" + _plan.SetNames[index] + "', no overrides; the same scenes and Reset shared scenes.";
                    report.AppendLine("| " + group.name + " (" + _whereIsGroup(group) + ") | " + group.MainScene.SceneName + " | " + DescribeParts(group) + " | " + after + " |");
                }
            }
            foreach (SceneGroup group in _plan.GroupsAlreadyInASet)
            {
                report.AppendLine("| " + group.name + " (" + _whereIsGroup(group) + ") | " + MainSceneName(group) + " | Scene set '" + group.SceneSet.name + "' | Unchanged: it already uses a scene set. |");
            }
            report.AppendLine();

            report.AppendLine("## Scene groups without a main scene");
            report.AppendLine();
            if (_plan.GroupsWithoutMainScene.Count == 0)
            {
                report.AppendLine("None.");
            }
            foreach (SceneGroup group in _plan.GroupsWithoutMainScene)
            {
                string references = _referencesOf(group);
                string line = "- " + group.name + " (" + _whereIsGroup(group) + "): " + DescribeParts(group) + ". Not a place the player can go, so no scene set uses it: unused, can be deleted";
                if (string.IsNullOrEmpty(references))
                {
                    line += ". Nothing in Assets/ refers to it.";
                }
                else
                {
                    line += " once these files stop referring to it: " + references + ".";
                }
                report.AppendLine(line);
            }
            return report.ToString();
        }

        /// <summary>
        /// Runs the migration in the project, or only writes the report when &lt;paramref name="_isPreview"/&gt;. Created
        /// asset paths are added to &lt;paramref name="_createdPaths"/&gt; and changed groups to &lt;paramref name="_changedGroups"/&gt;
        /// so a failing setup can undo them. Returns the report's path.
        /// </summary>
        public static string Run(SceneSystemSettings _settings, bool _isPreview, List<string> _createdPaths, List<SceneGroup> _changedGroups)
        {
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
            SceneGroupMigrationPlan plan = Plan(groups);
            string settingsFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_settings)).Replace('\\', '/');

            if (!_isPreview && plan.HasWork)
            {
                CreateSetsAndPointGroups(_settings, plan, settingsFolder, _createdPaths, _changedGroups);
            }

            Dictionary<string, List<string>> referencesByGuid = FindReferencesToGroupsWithoutMainScene(plan);
            string report = Report(plan, _isPreview, _group => AssetDatabase.GetAssetPath(_group), _group => ReferencesText(_group, referencesByGuid));
            string reportPath = settingsFolder + "/" + ReportFileName;
            File.WriteAllText(reportPath, report);
            AssetDatabase.ImportAsset(reportPath);

            string mode = _isPreview ? "preview" : "done";
            Debug.Log(LogPrefix + " Migration " + mode + ": " + plan.SetNames.Count + " scene set(s) (" + string.Join(", ", plan.SetNames) + ") for "
                + plan.GroupsToMigrateCount + " scene group(s); " + plan.GroupsWithoutMainScene.Count + " group(s) without a main scene listed as unused. Report: " + reportPath,
                AssetDatabase.LoadAssetAtPath<TextAsset>(reportPath));
            return reportPath;
        }

        public static bool HasMainScene(SceneGroup _group)
        {
            SceneReference mainScene = _group.MainScene;
            if (mainScene == null)
            {
                return false;
            }
            bool hasName = !string.IsNullOrEmpty(mainScene.SceneName);
            bool hasGuid = !string.IsNullOrEmpty(mainScene.SceneGuid);
            return hasName || hasGuid;
        }

        private static void CreateSetsAndPointGroups(SceneSystemSettings _settings, SceneGroupMigrationPlan _plan, string _settingsFolder, List<string> _createdPaths, List<SceneGroup> _changedGroups)
        {
            string setsFolder = _settingsFolder + "/" + SetsFolderName;
            if (!AssetDatabase.IsValidFolder(setsFolder))
            {
                AssetDatabase.CreateFolder(_settingsFolder, SetsFolderName);
                AddIfTracked(_createdPaths, setsFolder);
            }

            List<SceneSet> sets = new List<SceneSet>();
            for (int index = 0; index < _plan.SetNames.Count; index++)
            {
                SceneGroup sample = _plan.GroupsBySet[index][0];
                SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
                set.ManagerScenes.AddRange(SceneSetSync.CopyOf(sample.ManagerialBootScenesList));
                set.SharedScenes.AddRange(SceneSetSync.CopyOf(sample.ContextualBootScenesList));
                set.ResetSharedScenes = sample.ForceReloadContextualScenes;
                string setPath = AssetDatabase.GenerateUniqueAssetPath(setsFolder + "/" + _plan.SetNames[index] + ".asset");
                AssetDatabase.CreateAsset(set, setPath);
                AddIfTracked(_createdPaths, setPath);
                sets.Add(set);
                _settings.SceneSets.Add(set);
            }
            if (_plan.DefaultSetIndex >= 0)
            {
                _settings.DefaultSceneSet = sets[_plan.DefaultSetIndex];
            }
            EditorUtility.SetDirty(_settings);

            // Groups change last, after every set exists, so a failure above leaves no group pointing at a half-made set.
            List<string> groupPaths = new List<string>();
            for (int index = 0; index < sets.Count; index++)
            {
                foreach (SceneGroup group in _plan.GroupsBySet[index])
                {
                    group.SceneSet = sets[index];
                    group.SetOverride(SceneSetPart.ManagerScenes, false);
                    group.SetOverride(SceneSetPart.SharedScenes, false);
                    group.SetOverride(SceneSetPart.ResetSharedScenes, false);
                    SceneSetSync.ApplySet(group);
                    EditorUtility.SetDirty(group);
                    groupPaths.Add(AssetDatabase.GetAssetPath(group));
                    if (_changedGroups != null)
                    {
                        _changedGroups.Add(group);
                    }
                }
            }
            AssetDatabase.SaveAssets();

            // The rename refresh, once over every group: re-saving copies each scene's current name and path.
            AssetDatabase.ForceReserializeAssets(groupPaths, ForceReserializeAssetsOptions.ReserializeAssets);
        }

        private static Dictionary<string, List<string>> FindReferencesToGroupsWithoutMainScene(SceneGroupMigrationPlan _plan)
        {
            List<string> guids = new List<string>();
            foreach (SceneGroup group in _plan.GroupsWithoutMainScene)
            {
                guids.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(group)));
            }
            if (guids.Count == 0)
            {
                return new Dictionary<string, List<string>>();
            }
            return SceneRenameTracking.PathsContainingGuids(SceneRenameTracking.ProjectFiles(), guids, SceneRenameTracking.ReadIfSmallText);
        }

        private static string ReferencesText(SceneGroup _group, Dictionary<string, List<string>> _referencesByGuid)
        {
            string groupPath = AssetDatabase.GetAssetPath(_group);
            string guid = AssetDatabase.AssetPathToGUID(groupPath);
            if (!_referencesByGuid.TryGetValue(guid, out List<string> paths))
            {
                return string.Empty;
            }
            List<string> others = new List<string>();
            foreach (string path in paths)
            {
                if (path != groupPath)
                {
                    others.Add(path);
                }
            }
            return string.Join(", ", others);
        }

        private static void AddIfTracked(List<string> _createdPaths, string _path)
        {
            if (_createdPaths != null)
            {
                _createdPaths.Add(_path);
            }
        }

        private static string DescribeParts(SceneGroup _group)
        {
            string reset = _group.ForceReloadContextualScenes ? "on" : "off";
            return "manager scenes: " + SceneSetSync.Describe(_group.ManagerialBootScenesList)
                + "; shared scenes: " + SceneSetSync.Describe(_group.ContextualBootScenesList)
                + "; Reset shared scenes: " + reset;
        }

        private static string MainSceneName(SceneGroup _group)
        {
            if (!HasMainScene(_group))
            {
                return "(none)";
            }
            return _group.MainScene.SceneName;
        }

        private static string CombinationKey(SceneGroup _group)
        {
            StringBuilder key = new StringBuilder();
            key.Append("managers:");
            AppendSceneKeys(key, _group.ManagerialBootScenesList);
            key.Append("|shared:");
            AppendSceneKeys(key, _group.ContextualBootScenesList);
            key.Append("|reset:").Append(_group.ForceReloadContextualScenes);
            return key.ToString();
        }

        private static void AppendSceneKeys(StringBuilder _key, List<SceneReference> _scenes)
        {
            if (_scenes == null)
            {
                return;
            }
            foreach (SceneReference scene in _scenes)
            {
                if (scene == null)
                {
                    _key.Append("(empty),");
                    continue;
                }
                if (scene.HasGuid)
                {
                    _key.Append(scene.SceneGuid).Append(',');
                    continue;
                }
                _key.Append("name:").Append((scene.SceneName ?? string.Empty).ToLowerInvariant()).Append(',');
            }
        }

        private static void NameSets(SceneGroupMigrationPlan _plan)
        {
            HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < _plan.GroupsBySet.Count; index++)
            {
                List<string> mainScenes = new List<string>();
                foreach (SceneGroup group in _plan.GroupsBySet[index])
                {
                    mainScenes.Add(group.MainScene.SceneName);
                }
                string name = SetName(mainScenes, index);
                string uniqueName = name;
                int suffix = 2;
                while (usedNames.Contains(uniqueName))
                {
                    uniqueName = name + " " + suffix;
                    suffix++;
                }
                usedNames.Add(uniqueName);
                _plan.SetNames.Add(uniqueName);
            }
        }

        private static int IndexOfLargestSet(SceneGroupMigrationPlan _plan)
        {
            int bestIndex = -1;
            int bestCount = 0;
            for (int index = 0; index < _plan.GroupsBySet.Count; index++)
            {
                int count = _plan.GroupsBySet[index].Count;
                if (count > bestCount)
                {
                    bestCount = count;
                    bestIndex = index;
                }
            }
            return bestIndex;
        }

        // The longest start every name shares, cut back to whole words: "Arena 01" and "Arena 02" give "Arena ".
        private static string CommonWholeWordStart(List<string> _names)
        {
            string first = _names[0];
            int length = first.Length;
            foreach (string name in _names)
            {
                int shared = 0;
                int limit = Math.Min(length, name.Length);
                while (shared < limit && char.ToLowerInvariant(first[shared]) == char.ToLowerInvariant(name[shared]))
                {
                    shared++;
                }
                length = shared;
            }

            string start = first.Substring(0, length);
            bool endsAtWordBoundary = true;
            foreach (string name in _names)
            {
                bool nameContinuesTheWord = name.Length > length && name[length] != ' ' && length > 0 && start[length - 1] != ' ';
                if (nameContinuesTheWord)
                {
                    endsAtWordBoundary = false;
                }
            }
            if (endsAtWordBoundary)
            {
                return start;
            }
            int lastSpace = start.LastIndexOf(' ');
            if (lastSpace < 0)
            {
                return string.Empty;
            }
            return start.Substring(0, lastSpace + 1);
        }
    }
}
