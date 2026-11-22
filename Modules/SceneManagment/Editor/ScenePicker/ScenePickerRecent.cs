using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scenes and groups opened last from the scene picker, newest first, per project and per user. Before any
    /// search the picker shows them in a "Recent" section at the top, because they are the ones opened most.
    /// </summary>
    public static class ScenePickerRecent
    {
        public const string Heading = "Recent";
        public const int MostRecent = 5;
        private const string KeyPrefix = "AbstractPixel.SceneSystem.ScenePickerRecent.";

        public static List<string> Guids()
        {
            string stored = EditorPrefs.GetString(Key, string.Empty);
            List<string> guids = new List<string>();
            foreach (string guid in stored.Split(';'))
            {
                if (guid.Length > 0)
                {
                    guids.Add(guid);
                }
            }
            return guids;
        }

        public static void Remember(string _guid)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return;
            }
            List<string> guids = Guids();
            guids.Remove(_guid);
            guids.Insert(0, _guid);
            int keep = System.Math.Min(guids.Count, MostRecent * 2);
            EditorPrefs.SetString(Key, string.Join(";", guids.GetRange(0, keep)));
        }

        /// <summary>
        /// The rows with a "Recent" section on top holding copies of the rows opened last (at most MostRecent), newest
        /// first. A recent asset that is not in these rows (the other tab's, or deleted) is skipped.
        /// </summary>
        public static List<ScenePickerRow> WithRecentFirst(List<ScenePickerRow> _rows, List<string> _recentGuids)
        {
            List<ScenePickerRow> recent = new List<ScenePickerRow>();
            if (_rows == null)
            {
                return new List<ScenePickerRow>();
            }
            foreach (string guid in _recentGuids ?? new List<string>())
            {
                ScenePickerRow row = FindByGuid(_rows, guid);
                if (row != null && recent.Count < MostRecent)
                {
                    recent.Add(row);
                }
            }
            if (recent.Count == 0)
            {
                return new List<ScenePickerRow>(_rows);
            }
            List<ScenePickerRow> result = new List<ScenePickerRow>();
            result.Add(ScenePickerRow.Heading(Heading));
            result.AddRange(recent);
            bool startsWithHeading = _rows.Count > 0 && _rows[0].IsHeading;
            if (!startsWithHeading)
            {
                result.Add(ScenePickerRow.Heading(AllHeading(_rows)));
            }
            result.AddRange(_rows);
            return result;
        }

        // Under a Recent section the rest of the list gets a heading too, unless it already starts with one.
        private static string AllHeading(List<ScenePickerRow> _rows)
        {
            bool hasGroups = _rows.Exists(_row => _row.Kind == ScenePickerRowKind.SceneGroup);
            return hasGroups ? "All scene groups" : "All scenes";
        }

        private static ScenePickerRow FindByGuid(List<ScenePickerRow> _rows, string _guid)
        {
            foreach (ScenePickerRow row in _rows)
            {
                if (!row.IsHeading && row.AssetGuid == _guid)
                {
                    return row;
                }
            }
            return null;
        }

        // Per project: EditorPrefs are per user and shared by every project.
        private static string Key
        {
            get { return KeyPrefix + PlayerSettings.productGUID.ToString(); }
        }
    }
}
