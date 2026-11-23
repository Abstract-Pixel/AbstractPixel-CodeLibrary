using System.Collections.Generic;
using System.Text;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A small C# tokenizer that wraps keywords, type names (words starting with a capital), strings, comments and
    /// numbers in &lt;color&gt; tags. One line at a time, so a block comment is coloured per line.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsCodeHighlighter
    {
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "using", "namespace", "public", "private", "protected", "internal", "static", "class", "struct", "enum",
            "interface", "void", "return", "if", "else", "for", "foreach", "in", "while", "new", "null", "true", "false",
            "var", "int", "float", "bool", "string", "async", "await", "this", "base", "readonly", "const", "override",
            "virtual", "abstract", "sealed", "out", "ref", "break", "continue", "switch", "case", "default", "try",
            "catch", "finally", "throw", "get", "set", "is", "as", "typeof", "yield", "event", "delegate", "params"
        };

        public static string Highlight(string _line)
        {
            StringBuilder result = new StringBuilder();
            int index = 0;
            while (index < _line.Length)
            {
                index = AppendToken(_line, index, result);
            }
            return result.ToString();
        }

        // Appends the token that starts at this index and returns the index after it.
        private static int AppendToken(string _line, int _index, StringBuilder _result)
        {
            char character = _line[_index];
            bool startsComment = character == '/' && _index + 1 < _line.Length && _line[_index + 1] == '/';
            if (startsComment)
            {
                AppendColoured(_result, _line.Substring(_index), DocsTheme.Comment);
                return _line.Length;
            }
            if (character == '"')
            {
                int end = StringEnd(_line, _index);
                AppendColoured(_result, _line.Substring(_index, end - _index), DocsTheme.String);
                return end;
            }
            if (char.IsLetter(character) || character == '_')
            {
                int end = _index;
                while (end < _line.Length && (char.IsLetterOrDigit(_line[end]) || _line[end] == '_'))
                {
                    end++;
                }
                string word = _line.Substring(_index, end - _index);
                AppendWord(_result, word);
                return end;
            }
            if (char.IsDigit(character))
            {
                int end = _index;
                while (end < _line.Length && (char.IsLetterOrDigit(_line[end]) || _line[end] == '.'))
                {
                    end++;
                }
                AppendColoured(_result, _line.Substring(_index, end - _index), DocsTheme.Number);
                return end;
            }
            AppendPlain(_result, character.ToString());
            return _index + 1;
        }

        private static void AppendWord(StringBuilder _result, string _word)
        {
            if (Keywords.Contains(_word))
            {
                AppendColoured(_result, _word, DocsTheme.Keyword);
                return;
            }
            if (char.IsUpper(_word[0]))
            {
                AppendColoured(_result, _word, DocsTheme.TypeName);
                return;
            }
            AppendPlain(_result, _word);
        }

        private static int StringEnd(string _line, int _start)
        {
            int index = _start + 1;
            while (index < _line.Length)
            {
                if (_line[index] == '\\')
                {
                    index += 2;
                    continue;
                }
                if (_line[index] == '"')
                {
                    return index + 1;
                }
                index++;
            }
            return _line.Length;
        }

        private static void AppendColoured(StringBuilder _result, string _text, string _colour)
        {
            _result.Append("<color=").Append(_colour).Append(">");
            AppendPlain(_result, _text);
            _result.Append("</color>");
        }

        // "<" would start a rich-text tag, so it goes inside noparse.
        private static void AppendPlain(StringBuilder _result, string _text)
        {
            _result.Append(_text.Replace("<", "<noparse><</noparse>"));
        }
    }
}
