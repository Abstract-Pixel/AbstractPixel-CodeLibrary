using AbstractPixel.EditorModules.Documentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Reads the library's C# files for the reference pages: which file declares a type, and the /// summary above a
    /// declaration. Files are listed once per domain load.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsSource
    {
        private static List<string> files;
        private static readonly Regex TagPattern = new Regex(@"<see cref=""(?:[A-Z]:)?([^""]+)""\s*/>|<[^>]+>");

        /// <summary>Every .cs file of the scene system and the transitions module, as asset paths (no tests, no samples).</summary>
        public static List<string> Files()
        {
            if (files != null)
            {
                return files;
            }
            files = new List<string>();
            AddFolder(SceneDocsPaths.Module);
            AddFolder(SceneDocsPaths.Library + "Modules/SceneTransitionSystem/");
            files.Sort(StringComparer.Ordinal);
            return files;
        }

        private static void AddFolder(string _assetFolder)
        {
            string root = DocsFiles.Physical(_assetFolder);
            if (!Directory.Exists(root))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string relative = file.Substring(root.Length).Replace('\\', '/').TrimStart('/');
                bool isTestOrSample = relative.Contains("Tests/") || relative.StartsWith("Samples/");
                if (!isTestOrSample)
                {
                    files.Add(_assetFolder + relative);
                }
            }
        }

        /// <summary>The asset path of the file that declares a type by this name, or null.</summary>
        public static string FileDeclaring(string _typeName)
        {
            Regex declaration = new Regex(@"\b(class|struct|enum|interface)\s+" + Regex.Escape(_typeName) + @"\b");
            string sameName = null;
            foreach (string path in Files())
            {
                if (Path.GetFileNameWithoutExtension(path) == _typeName)
                {
                    sameName = path;
                    break;
                }
            }
            if (sameName != null)
            {
                return sameName;
            }
            foreach (string path in Files())
            {
                string text = DocsFiles.Read(path) ?? "";
                if (declaration.IsMatch(text))
                {
                    return path;
                }
            }
            return null;
        }

        /// <summary>The 1-based line of the first line in a file that matches, or 0.</summary>
        public static int LineOf(string _assetPath, Regex _pattern)
        {
            string text = DocsFiles.Read(_assetPath);
            if (text == null)
            {
                return 0;
            }
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                if (_pattern.IsMatch(lines[index]))
                {
                    return index + 1;
                }
            }
            return 0;
        }

        /// <summary>The /// summary right above the first line that declares this name (a type or a public member).</summary>
        public static string SummaryAbove(string _assetPath, string _name)
        {
            string text = DocsFiles.Read(_assetPath);
            if (text == null)
            {
                return "";
            }
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            Regex declaration = new Regex(@"\b(public|protected)\b.*\b" + Regex.Escape(_name) + @"\b\s*(\(|\{|=|;|:|<|$)");
            for (int index = 0; index < lines.Length; index++)
            {
                if (!declaration.IsMatch(lines[index]))
                {
                    continue;
                }
                return CommentAbove(lines, index);
            }
            return "";
        }

        private static string CommentAbove(string[] _lines, int _index)
        {
            List<string> comment = new List<string>();
            int line = _index - 1;
            while (line >= 0)
            {
                string trimmed = _lines[line].Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    line--;
                    continue;
                }
                if (!trimmed.StartsWith("///"))
                {
                    break;
                }
                comment.Insert(0, trimmed.Substring(3).Trim());
                line--;
            }
            return Clean(string.Join(" ", comment));
        }

        // Keeps the summary only, with see-cref names as plain words and no other XML tags.
        private static string Clean(string _comment)
        {
            int start = _comment.IndexOf("<summary>", StringComparison.Ordinal);
            int end = _comment.IndexOf("</summary>", StringComparison.Ordinal);
            string summary = start >= 0 && end > start ? _comment.Substring(start + 9, end - start - 9) : _comment;
            string withoutTags = TagPattern.Replace(summary, _match => _match.Groups[1].Success ? ShortName(_match.Groups[1].Value) : "");
            return withoutTags.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&").Trim();
        }

        private static string ShortName(string _cref)
        {
            int dot = _cref.LastIndexOf('.');
            return dot < 0 ? _cref : _cref.Substring(dot + 1);
        }

        /// <summary>The first sentence of a type's summary, for the code map.</summary>
        public static string FirstSentence(string _summary)
        {
            if (string.IsNullOrEmpty(_summary))
            {
                return "";
            }
            int stop = _summary.IndexOf(". ", StringComparison.Ordinal);
            return stop < 0 ? _summary : _summary.Substring(0, stop + 1);
        }

        /// <summary>The type names declared in a file (public or not), in order.</summary>
        public static List<string> TypesIn(string _assetPath)
        {
            List<string> names = new List<string>();
            string text = DocsFiles.Read(_assetPath) ?? "";
            foreach (Match match in Regex.Matches(text, @"\b(?:class|struct|enum|interface)\s+([A-Z]\w*)"))
            {
                if (!names.Contains(match.Groups[1].Value))
                {
                    names.Add(match.Groups[1].Value);
                }
            }
            return names;
        }
    }
}
