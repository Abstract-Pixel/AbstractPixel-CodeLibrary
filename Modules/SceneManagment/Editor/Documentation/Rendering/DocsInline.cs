using System.Text;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Turns inline Markdown into UI Toolkit rich text: `code`, **bold**, *italic* and [text](target) links. A link
    /// becomes a &lt;link&gt; tag whose id is the target, so <see cref="DocsLinkEvents"/> can route the click. A term
    /// link is written [scene group](term:scene-group).
    /// </summary>
    public static class DocsInline
    {
        private static readonly Regex CodePattern = new Regex(@"`([^`]+)`");
        private static readonly Regex LinkPattern = new Regex(@"\[([^\]]+)\]\(([^)\s]+)\)");
        private static readonly Regex BoldPattern = new Regex(@"\*\*(.+?)\*\*");
        private static readonly Regex ItalicPattern = new Regex(@"(?<![*\w])\*(?!\s)(.+?)(?<!\s)\*(?![*\w])");

        /// <summary>The rich text for one line of inline Markdown.</summary>
        public static string ToRichText(string _markdown)
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
                result.Append(CodeSpan(code.Groups[1].Value));
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
            return "<mark=" + DocsTheme.InlineCodeBackground + "><color=" + DocsTheme.InlineCode + "><noparse>" + padded + "</noparse></color></mark>";
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
