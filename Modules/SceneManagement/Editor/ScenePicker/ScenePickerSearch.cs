using System;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene picker's search: the text splits on spaces, case is ignored, and every word must appear in the row's
    /// name, folder or (for a scene) group name. A heading stays only while a row under it matches.
    /// </summary>
    public static class ScenePickerSearch
    {
        private static readonly char[] WordSeparators = { ' ', '\t' };

        public static string[] Words(string _query)
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                return new string[0];
            }
            return _query.ToLowerInvariant().Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        // Within a section, rows whose name holds every word come first; each part keeps its order.
        private static void AddSection(List<ScenePickerRow> _result, ScenePickerRow _heading, List<ScenePickerRow> _section, string[] _words)
        {
            if (_section.Count == 0)
            {
                return;
            }
            if (_heading != null)
            {
                _result.Add(_heading);
            }
            List<ScenePickerRow> elsewhere = new List<ScenePickerRow>();
            foreach (ScenePickerRow row in _section)
            {
                if (NameHoldsEveryWord(row, _words))
                {
                    _result.Add(row);
                }
                else
                {
                    elsewhere.Add(row);
                }
            }
            _result.AddRange(elsewhere);
        }

        private static bool NameHoldsEveryWord(ScenePickerRow _row, string[] _words)
        {
            string name = (_row.Title ?? string.Empty).ToLowerInvariant();
            foreach (string word in _words)
            {
                if (name.IndexOf(word, StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Matches(ScenePickerRow _row, string[] _words)
        {
            if (_row == null || _row.IsHeading)
            {
                return false;
            }
            foreach (string word in _words)
            {
                if (_row.SearchText.IndexOf(word, StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The title with each searched word shown in bold where it first appears, ignoring case, as UI Toolkit rich
        /// text. A title that holds a rich text tag character is returned as it is, so a name never turns into markup.
        /// </summary>
        public static string BoldMatches(string _title, string[] _words)
        {
            if (string.IsNullOrEmpty(_title) || _words == null || _words.Length == 0)
            {
                return _title ?? string.Empty;
            }
            if (_title.IndexOf('<') >= 0 || _title.IndexOf('>') >= 0)
            {
                return _title;
            }
            bool[] bold = new bool[_title.Length];
            string lower = _title.ToLowerInvariant();
            foreach (string word in _words)
            {
                int start = lower.IndexOf(word, StringComparison.Ordinal);
                for (int index = start; start >= 0 && index < start + word.Length; index++)
                {
                    bold[index] = true;
                }
            }
            return WrapBoldRuns(_title, bold);
        }

        private static string WrapBoldRuns(string _title, bool[] _bold)
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            for (int index = 0; index < _title.Length; index++)
            {
                bool opens = _bold[index] && (index == 0 || !_bold[index - 1]);
                bool closes = _bold[index] && (index == _title.Length - 1 || !_bold[index + 1]);
                if (opens)
                {
                    text.Append("<b>");
                }
                text.Append(_title[index]);
                if (closes)
                {
                    text.Append("</b>");
                }
            }
            return text.ToString();
        }

        /// <summary>The rows that match, in order, each heading kept only above at least one matching row.</summary>
        public static List<ScenePickerRow> Filter(List<ScenePickerRow> _rows, string _query)
        {
            List<ScenePickerRow> result = new List<ScenePickerRow>();
            if (_rows == null)
            {
                return result;
            }
            string[] words = Words(_query);
            ScenePickerRow heading = null;
            List<ScenePickerRow> section = new List<ScenePickerRow>();
            foreach (ScenePickerRow row in _rows)
            {
                if (row.IsHeading)
                {
                    AddSection(result, heading, section, words);
                    heading = row;
                    section = new List<ScenePickerRow>();
                    continue;
                }
                if (Matches(row, words))
                {
                    section.Add(row);
                }
            }
            AddSection(result, heading, section, words);
            return result;
        }
    }
}
