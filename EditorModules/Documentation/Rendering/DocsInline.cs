using System.Text;
using System.Text.RegularExpressions;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// Turns inline Markdown into UI Toolkit rich text: `code`, **bold**, *italic* and [text](target) links. A link
    /// becomes a &lt;link&gt; tag whose id is the target, so <see cref="DocsLinkEvents"/> can route the click. A term
    /// link is written [scene group](term:scene-group).
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsInline
    {
        private static readonly Regex CodePattern = new Regex(@"`([^`]+)`");
        private static readonly Regex LinkPattern = new Regex(@"\[([^\]]+)\]\(([^)\s]+)\)");
        private static readonly Regex BoldPattern = new Regex(@"\*\*(.+?)\*\*");
        private static readonly Regex ItalicPattern = new Regex(@"(?<![*\w])\*(?!\s)(.+?)(?<!\s)\*(?![*\w])");

        /// <summary>The rich text for one line of inline Markdown.</summary>
        public static string ToRichText(string _markdown)
        {
            return ToRichText(_markdown, null);
        }

        /// <summary>
        /// The rich text for one line of inline Markdown. A code span links when <paramref name="_codeLink"/> returns a
        /// target for its text (such as "type:SomeClass"); a null resolver, or a null target, leaves it plain.
        /// </summary>
        public static string ToRichText(string _markdown, System.Func<string, string> _codeLink)
        {
            if (string.IsNullOrEmpty(_markdown))
            {
                return "";
            }
            // Code spans first, so nothing inside them is read as Markdown or as a rich-text tag.
            StringBuilder result = new StringBuilder();
            int position = 0;
            foreach (Match code in CodePattern.Matches(_markdown))
            {
                result.Append(FormatText(_markdown.Substring(position, code.Index - position)));
                string target = _codeLink == null ? null : _codeLink(code.Groups[1].Value);
                result.Append(target == null ? CodeSpan(code.Groups[1].Value) : LinkedCodeSpan(code.Groups[1].Value, target));
                position = code.Index + code.Length;
            }
            result.Append(FormatText(_markdown.Substring(position)));
            return result.ToString();
        }

        /// <summary>The text with all Markdown removed, for search and tooltips.</summary>
        public static string ToPlainText(string _markdown)
        {
            if (string.IsNullOrEmpty(_markdown))
            {
                return "";
            }
            string withoutLinks = LinkPattern.Replace(_markdown, "$1");
            string withoutCode = CodePattern.Replace(withoutLinks, "$1");
            string withoutBold = BoldPattern.Replace(withoutCode, "$1");
            return ItalicPattern.Replace(withoutBold, "$1");
        }

        // A muted accent colour on a faint box: it reads as code without shouting. (Forced monospace spacing made wide letters overlap.)
        private static string CodeSpan(string _code)
        {
            string code = _code.Replace("</noparse>", "");
            string padded = NoBreakSpace + code + NoBreakSpace;
            // Wrapped in a link that goes nowhere, only so the span can change colour under the pointer.
            return "<link=\"" + DocsLinkEvents.HoverOnlyPrefix + "\"><mark=" + DocsTheme.InlineCodeBackground + "><color=" + DocsTheme.InlineCode + "><noparse>" + padded + "</noparse></color></mark></link>";
        }

        // A code span that names a documented type: the link colour on the same faint box, so it reads as code you can click.
        private static string LinkedCodeSpan(string _code, string _target)
        {
            string code = _code.Replace("</noparse>", "");
            string padded = NoBreakSpace + code + NoBreakSpace;
            return "<link=\"" + _target + "\"><mark=" + DocsTheme.InlineCodeBackground + "><color=" + DocsTheme.Link + "><noparse>" + padded + "</noparse></color></mark></link>";
        }

        private static readonly string NoBreakSpace = ((char)0xA0).ToString();

        // Links, bold and italic, outside code spans. A "<" that is not ours is kept out of the tag parser.
        private static string FormatText(string _text)
        {
            string escaped = _text.Replace("<", "<noparse><</noparse>");
            string linked = LinkPattern.Replace(escaped, LinkTag);
            string bold = BoldPattern.Replace(linked, "<b>$1</b>");
            return ItalicPattern.Replace(bold, "<i>$1</i>");
        }

        private static string LinkTag(Match _match)
        {
            string text = _match.Groups[1].Value;
            string target = _match.Groups[2].Value;
            bool isTerm = target.StartsWith("term:");
            if (isTerm)
            {
                return "<link=\"" + target + "\"><color=" + DocsTheme.Term + "><u>" + text + "</u></color></link>";
            }
            return "<link=\"" + target + "\"><color=" + DocsTheme.Link + ">" + text + "</color></link>";
        }
    }
}
