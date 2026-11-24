using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Reads declarations from the library's source files: the line that declares a type or a member, the modifiers
    /// written on it ("public static", "partial"), and the /// comment above it as a summary and a list of remarks.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsDeclarations
    {
        private static readonly HashSet<string> ModifierWords = new HashSet<string>
        {
            "public", "protected", "internal", "private", "static", "async", "virtual", "override", "abstract", "sealed",
            "readonly", "const", "extern", "new", "partial", "unsafe", "volatile", "event", "required"
        };

        private static readonly Regex AttributePrefix = new Regex(@"^\s*(\[[^\]]*\]\s*)+");
        private static readonly Regex CrefPattern = new Regex(@"<see cref=""(?:[A-Z]:)?([^""]+)""\s*/>");
        private static readonly Regex InlineCodePattern = new Regex(@"<c>(.*?)</c>");
        private static readonly Regex ParamRefPattern = new Regex(@"<(?:paramref|typeparamref) name=""([^""]+)""\s*/>");
        private static readonly Regex AnyTag = new Regex(@"<[^>]+>");

        /// <summary>The file's lines, or an empty array when it cannot be read.</summary>
        public static string[] LinesOf(string _assetPath)
        {
            string text = _assetPath == null ? null : SceneDocsPaths.Read(_assetPath);
            if (text == null)
            {
                return new string[0];
            }
            return text.Replace("\r\n", "\n").Split('\n');
        }

        /// <summary>
        /// The index of the line that declares this member, or -1. Every parameter name must be on the line, which
        /// tells overloads apart. Interface members have no access word, so <paramref name="_inInterface"/> drops it.
        /// </summary>
        public static int MemberLine(string[] _lines, string _name, IList<string> _parameterNames, bool _inInterface)
        {
            string access = _inInterface ? "" : @"\b(public|protected|internal|private)\b[^=(]*";
            Regex declaration = new Regex(@"^\s*(\[[^\]]*\]\s*)*" + access + @"\b" + Regex.Escape(_name) + @"\s*(\(|<|\{|=|;|,|$)");
            for (int index = 0; index < _lines.Length; index++)
            {
                string line = _lines[index];
                bool isComment = line.TrimStart().StartsWith("//");
                if (isComment || !declaration.IsMatch(line))
                {
                    continue;
                }
                if (HasEveryName(line, _parameterNames))
                {
                    return index;
                }
            }
            return -1;
        }

        private static bool HasEveryName(string _line, IList<string> _names)
        {
            if (_names == null)
            {
                return true;
            }
            foreach (string name in _names)
            {
                if (!Regex.IsMatch(_line, @"\b" + Regex.Escape(name) + @"\b"))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>The modifier words at the start of a declaration line, in the order written. Attributes are skipped.</summary>
        public static List<string> ModifiersOn(string _line)
        {
            List<string> modifiers = new List<string>();
            string withoutAttributes = AttributePrefix.Replace(_line, "");
            foreach (string word in withoutAttributes.Trim().Split(' '))
            {
                if (!ModifierWords.Contains(word))
                {
                    break;
                }
                modifiers.Add(word);
            }
            return modifiers;
        }

        /// <summary>
        /// The type's declaration as written, from its modifiers to its name: "public static class SceneActions". Falls
        /// back to reflection when the source cannot be found.
        /// </summary>
        public static string TypeDeclaration(Type _type)
        {
            string[] lines = LinesOf(SceneDocsSource.FileDeclaring(_type.Name));
            Regex declaration = new Regex(@"\b(class|struct|enum|interface)\s+" + Regex.Escape(_type.Name) + @"\b");
            foreach (string line in lines)
            {
                Match match = declaration.Match(line);
                bool isComment = line.TrimStart().StartsWith("//");
                if (match.Success && !isComment)
                {
                    string withoutAttributes = AttributePrefix.Replace(line, "");
                    int end = withoutAttributes.IndexOf(_type.Name, withoutAttributes.IndexOf(match.Groups[1].Value, StringComparison.Ordinal), StringComparison.Ordinal);
                    return withoutAttributes.Substring(0, end + _type.Name.Length).Trim();
                }
            }
            return ReflectedDeclaration(_type);
        }

        private static string ReflectedDeclaration(Type _type)
        {
            StringBuilder text = new StringBuilder(_type.IsPublic || _type.IsNestedPublic ? "public " : "internal ");
            bool isStatic = _type.IsAbstract && _type.IsSealed;
            if (isStatic)
            {
                text.Append("static ");
            }
            text.Append(KindWord(_type)).Append(' ').Append(_type.Name);
            return text.ToString();
        }

        private static string KindWord(Type _type)
        {
            if (_type.IsEnum)
            {
                return "enum";
            }
            if (_type.IsInterface)
            {
                return "interface";
            }
            return _type.IsValueType ? "struct" : "class";
        }

        /// <summary>
        /// The /// comment above a line: the summary as one line of inline Markdown, and the remarks as a list. Each
        /// remark starts with "- " in the source; a remarks block with no dashes is one remark. See-cref names become
        /// code spans, so they link.
        /// </summary>
        public static void CommentAbove(string[] _lines, int _index, out string _summary, out List<string> _remarks)
        {
            List<string> comment = new List<string>();
            for (int line = _index - 1; line >= 0; line--)
            {
                string trimmed = _lines[line].Trim();
                bool isAttribute = trimmed.StartsWith("[") && trimmed.EndsWith("]");
                if (isAttribute)
                {
                    continue;
                }
                if (!trimmed.StartsWith("///"))
                {
                    break;
                }
                comment.Insert(0, trimmed.Substring(3).Trim());
            }
            string text = string.Join("\n", comment);
            _summary = Clean(Between(text, "<summary>", "</summary>", true).Replace('\n', ' '));
            _remarks = RemarkList(Between(text, "<remarks>", "</remarks>", false));
        }

        // The text between two tags, or the whole comment when it has no tags and _wholeWhenMissing is set.
        private static string Between(string _text, string _open, string _close, bool _wholeWhenMissing)
        {
            int start = _text.IndexOf(_open, StringComparison.Ordinal);
            int end = _text.IndexOf(_close, StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                return _text.Substring(start + _open.Length, end - start - _open.Length).Trim();
            }
            bool hasNoTags = _text.IndexOf('<') < 0;
            return _wholeWhenMissing && hasNoTags ? _text : "";
        }

        private static List<string> RemarkList(string _remarks)
        {
            List<string> items = new List<string>();
            foreach (string raw in _remarks.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                bool startsItem = line.StartsWith("- ") || items.Count == 0;
                if (startsItem)
                {
                    items.Add(line.StartsWith("- ") ? line.Substring(2) : line);
                    continue;
                }
                items[items.Count - 1] = items[items.Count - 1] + " " + line;
            }
            return items.ConvertAll(Clean);
        }

        // XML doc tags become inline Markdown: see-cref and c become code spans; other tags go.
        private static string Clean(string _comment)
        {
            string text = CrefPattern.Replace(_comment, _match => "`" + _match.Groups[1].Value + "`");
            text = InlineCodePattern.Replace(text, "`$1`");
            text = ParamRefPattern.Replace(text, "`$1`");
            text = AnyTag.Replace(text, "");
            return text.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&").Trim();
        }
    }
}
