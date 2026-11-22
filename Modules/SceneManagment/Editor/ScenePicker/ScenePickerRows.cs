using System;
using System.Collections.Generic;
using System.Text;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Builds the rows of the scene picker's two tabs from plain project data (decisions 4 and 5 of session 5A).
    /// <list type="bullet">
    /// <item>Scenes: every scene under Assets/ plus sample scenes, by name. A row shows the scene's group, "No group",
    /// or for a manager or shared scene its scene set. Sample scenes sit under a "Samples" heading at the bottom.
    /// Unity's InitTestScene files and other packages' scenes are left out.</item>
    /// <item>Scene Groups: game groups by name with their main scene and scene set, then sample scene groups under
    /// "Samples". A group with no main scene can be selected but not opened.</item>
    /// </list>
    /// </summary>
    public static class ScenePickerRows
    {
        public const string SamplesHeading = "Samples";
        public const string NoGroupHeading = "Scenes in no group";
        public const string NoGroupText = "No group";
        public const string NoMainSceneText = "No main scene";
        private const string TestScenePrefix = "InitTestScene";
        private const string DetailSeparator = "  ·  ";

        /// <summary>The library's package root; a scene in one of its Samples folders counts as a sample scene.</summary>
        public const string LibraryPackageRoot = "Packages/com.abstractpixel.library/";

        public static List<ScenePickerRow> ForScenes(List<ScenePickerSceneInfo> _scenes, List<ScenePickerGroupInfo> _groups, List<ScenePickerSetInfo> _sets)
        {
            List<ScenePickerGroupInfo> groups = _groups ?? new List<ScenePickerGroupInfo>();
            List<ScenePickerSetInfo> sets = _sets ?? new List<ScenePickerSetInfo>();
            HashSet<string> sampleSceneGuids = SampleSceneGuids(groups);

            List<ScenePickerRow> gameRows = new List<ScenePickerRow>();
            List<ScenePickerRow> noGroupRows = new List<ScenePickerRow>();
            List<ScenePickerRow> sampleRows = new List<ScenePickerRow>();
            foreach (ScenePickerSceneInfo scene in _scenes ?? new List<ScenePickerSceneInfo>())
            {
                if (scene == null || IsLeftOut(scene, sampleSceneGuids))
                {
                    continue;
                }
                bool isSample = IsSampleScene(scene, sampleSceneGuids);
                ScenePickerRow row = SceneRow(scene, groups, sets, isSample);
                if (isSample)
                {
                    sampleRows.Add(row);
                }
                else if (row.Detail == NoGroupText)
                {
                    noGroupRows.Add(row);
                }
                else
                {
                    gameRows.Add(row);
                }
            }
            // The game's scenes first, as they are the ones opened most; asset store demos and tests follow.
            List<ScenePickerRow> rows = Join(gameRows, new List<ScenePickerRow>());
            AddSection(rows, NoGroupHeading, noGroupRows);
            AddSection(rows, SamplesHeading, sampleRows);
            return rows;
        }

        private static void AddSection(List<ScenePickerRow> _rows, string _heading, List<ScenePickerRow> _section)
        {
            if (_section.Count == 0)
            {
                return;
            }
            _section.Sort(CompareByTitle);
            _rows.Add(ScenePickerRow.Heading(_heading));
            _rows.AddRange(_section);
        }

        public static List<ScenePickerRow> ForGroups(List<ScenePickerGroupInfo> _groups)
        {
            List<ScenePickerRow> gameRows = new List<ScenePickerRow>();
            List<ScenePickerRow> sampleRows = new List<ScenePickerRow>();
            foreach (ScenePickerGroupInfo group in _groups ?? new List<ScenePickerGroupInfo>())
            {
                if (group == null)
                {
                    continue;
                }
                ScenePickerRow row = GroupRow(group);
                if (group.IsSample)
                {
                    sampleRows.Add(row);
                }
                else
                {
                    gameRows.Add(row);
                }
            }
            return Join(gameRows, sampleRows);
        }

        // Unity's test runner scenes and other packages' scenes are left out; the library's sample scenes stay.
        private static bool IsLeftOut(ScenePickerSceneInfo _scene, HashSet<string> _sampleSceneGuids)
        {
            if (string.IsNullOrEmpty(_scene.Path))
            {
                return true;
            }
            bool isTestRunnerScene = !string.IsNullOrEmpty(_scene.Name) && _scene.Name.StartsWith(TestScenePrefix, StringComparison.Ordinal);
            if (isTestRunnerScene)
            {
                return true;
            }
            if (_scene.Path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return false;
            }
            return !IsSampleScene(_scene, _sampleSceneGuids);
        }

        private static bool IsSampleScene(ScenePickerSceneInfo _scene, HashSet<string> _sampleSceneGuids)
        {
            if (!string.IsNullOrEmpty(_scene.Guid) && _sampleSceneGuids.Contains(_scene.Guid))
            {
                return true;
            }
            bool inLibrary = _scene.Path.StartsWith(LibraryPackageRoot, StringComparison.Ordinal);
            return inLibrary && _scene.Path.Contains("/Samples/");
        }

        private static HashSet<string> SampleSceneGuids(List<ScenePickerGroupInfo> _groups)
        {
            HashSet<string> guids = new HashSet<string>();
            foreach (ScenePickerGroupInfo group in _groups)
            {
                if (group == null || !group.IsSample)
                {
                    continue;
                }
                AddIfSet(guids, group.MainSceneGuid);
                foreach (string guid in group.OtherSceneGuids)
                {
                    AddIfSet(guids, guid);
                }
            }
            return guids;
        }

        private static ScenePickerRow SceneRow(ScenePickerSceneInfo _scene, List<ScenePickerGroupInfo> _groups, List<ScenePickerSetInfo> _sets, bool _isSample)
        {
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = ScenePickerRowKind.Scene;
            row.Title = _scene.Name;
            row.Detail = SceneDetail(_scene, _groups, _sets);
            row.Folder = FolderOf(_scene.Path);
            row.AssetGuid = _scene.Guid;
            row.AssetPath = _scene.Path;
            row.IsSample = _isSample;
            row.Status = _scene.Status;
            row.SearchText = SearchTextOf(row.Title, row.Folder, row.Detail);
            return row;
        }

        // The group whose main scene this is; else the scene sets (or groups) that list it; else "No group".
        private static string SceneDetail(ScenePickerSceneInfo _scene, List<ScenePickerGroupInfo> _groups, List<ScenePickerSetInfo> _sets)
        {
            if (string.IsNullOrEmpty(_scene.Guid))
            {
                return NoGroupText;
            }
            foreach (ScenePickerGroupInfo group in _groups)
            {
                if (group != null && group.MainSceneGuid == _scene.Guid)
                {
                    return group.Name;
                }
            }

            List<string> setNames = new List<string>();
            foreach (ScenePickerSetInfo set in _sets)
            {
                if (set != null && set.SceneGuids.Contains(_scene.Guid))
                {
                    AddOnce(setNames, set.Name + " (scene set)");
                }
            }
            if (setNames.Count > 0)
            {
                return string.Join(", ", setNames);
            }

            List<string> listingGroups = new List<string>();
            foreach (ScenePickerGroupInfo group in _groups)
            {
                if (group != null && group.OtherSceneGuids.Contains(_scene.Guid))
                {
                    AddOnce(listingGroups, group.Name);
                }
            }
            if (listingGroups.Count > 0)
            {
                return "Shared by " + string.Join(", ", listingGroups);
            }
            return NoGroupText;
        }

        private static ScenePickerRow GroupRow(ScenePickerGroupInfo _group)
        {
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = ScenePickerRowKind.SceneGroup;
            row.Title = _group.Name;
            row.Folder = FolderOf(_group.Path);
            row.AssetGuid = _group.Guid;
            row.AssetPath = _group.Path;
            row.IsSample = _group.IsSample;
            row.CanOpen = _group.HasMainScene;
            row.Status = _group.MainSceneStatus;

            StringBuilder detail = new StringBuilder();
            if (_group.HasMainScene)
            {
                detail.Append(_group.MainSceneName);
            }
            else
            {
                detail.Append(NoMainSceneText);
            }
            if (!string.IsNullOrEmpty(_group.SceneSetName))
            {
                detail.Append(DetailSeparator);
                detail.Append(_group.SceneSetName);
            }
            row.Detail = detail.ToString();
            row.SearchText = SearchTextOf(row.Title, row.Folder, string.Empty);
            return row;
        }

        private static List<ScenePickerRow> Join(List<ScenePickerRow> _gameRows, List<ScenePickerRow> _sampleRows)
        {
            _gameRows.Sort(CompareByTitle);
            _sampleRows.Sort(CompareByTitle);
            List<ScenePickerRow> rows = new List<ScenePickerRow>(_gameRows);
            if (_sampleRows.Count > 0)
            {
                rows.Add(ScenePickerRow.Heading(SamplesHeading));
                rows.AddRange(_sampleRows);
            }
            return rows;
        }

        private static int CompareByTitle(ScenePickerRow _a, ScenePickerRow _b)
        {
            int byName = NaturalOrder.Compare(_a.Title, _b.Title);
            if (byName != 0)
            {
                return byName;
            }
            return string.CompareOrdinal(_a.AssetPath, _b.AssetPath);
        }

        private static string SearchTextOf(string _title, string _folder, string _detail)
        {
            return (_title + " " + _folder + " " + _detail).ToLowerInvariant();
        }

        /// <summary>The folder part of an asset path, such as "Assets/Levels" for "Assets/Levels/Level 3.unity".</summary>
        public static string FolderOf(string _assetPath)
        {
            if (string.IsNullOrEmpty(_assetPath))
            {
                return string.Empty;
            }
            int lastSlash = _assetPath.LastIndexOf('/');
            if (lastSlash < 0)
            {
                return string.Empty;
            }
            return _assetPath.Substring(0, lastSlash);
        }

        private static void AddIfSet(HashSet<string> _set, string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                _set.Add(_value);
            }
        }

        private static void AddOnce(List<string> _list, string _value)
        {
            if (!_list.Contains(_value))
            {
                _list.Add(_value);
            }
        }
    }
}
