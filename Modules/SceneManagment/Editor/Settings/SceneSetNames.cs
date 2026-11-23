using System;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Whether a name works for a scene set: it becomes the asset's file name, so it must be a valid file name and
    /// differ from the other scene sets' names. Used by setup's Scene Sets step and the rename on the Scene System page.
    /// </summary>
    public static class SceneSetNames
    {
        private static readonly char[] InvalidCharacters = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

        /// <summary>Why this name can't be used, in one sentence; empty when it can.</summary>
        /// <param name="_takenNames">The other scene sets' names (compared ignoring case, as file names are on Windows).</param>
        public static string Problem(string _name, IEnumerable<string> _takenNames)
        {
            string name = _name ?? string.Empty;
            if (name.Trim().Length == 0)
            {
                return "Give the scene set a name.";
            }
            if (name.IndexOfAny(InvalidCharacters) >= 0)
            {
                return "A scene set name can't contain / \\ : * ? \" < > or |.";
            }
            bool hasControlCharacter = false;
            foreach (char character in name)
            {
                if (char.IsControl(character))
                {
                    hasControlCharacter = true;
                }
            }
            if (hasControlCharacter)
            {
                return "A scene set name can't contain a line break or tab.";
            }
            bool badEnds = name.StartsWith(" ") || name.EndsWith(" ") || name.EndsWith(".");
            if (badEnds)
            {
                return "A scene set name can't start or end with a space, or end with a dot.";
            }
            if (_takenNames != null)
            {
                foreach (string taken in _takenNames)
                {
                    if (string.Equals(taken, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return "Another scene set is already named '" + taken + "'.";
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>"Scene Set", or "Scene Set 2", "Scene Set 3"… whichever is not taken yet.</summary>
        public static string FirstFree(string _baseName, IEnumerable<string> _takenNames)
        {
            List<string> taken = new List<string>(_takenNames ?? new List<string>());
            string candidate = _baseName;
            int number = 2;
            while (string.IsNullOrEmpty(Problem(candidate, null)) && !string.IsNullOrEmpty(Problem(candidate, taken)))
            {
                candidate = _baseName + " " + number;
                number++;
            }
            return candidate;
        }
    }
}
