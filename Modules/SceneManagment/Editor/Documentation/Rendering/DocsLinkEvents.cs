using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The one place that uses UI Toolkit's rich-text link events, so a later API change touches one file.</summary>
    /// <remarks>
    /// - Click calls the click handler with the link id; hover sets the label's tooltip.
    /// - Hover also brightens the link under the pointer: the label's text is redrawn with that link's colours raised,
    ///   and put back when the pointer leaves.
    /// - A link whose id starts with HoverOnlyPrefix (plain inline code) only changes colour; a click does nothing.
    /// </remarks>
    public static class DocsLinkEvents
    {
        public const string HoverOnlyPrefix = "hover:";

        public static void Register(TextElement _label, Action<string> _clicked, Func<string, string> _tooltipFor)
        {
            _label.RegisterCallback<PointerUpLinkTagEvent>(_event => OnClicked(_event.linkID, _clicked));
            _label.RegisterCallback<PointerOverLinkTagEvent>(_event => OnOver(_label, _event.linkID, _tooltipFor));
            _label.RegisterCallback<PointerOutLinkTagEvent>(_event => OnOut(_label));
        }

        private static void OnClicked(string _linkId, Action<string> _clicked)
        {
            bool isHoverOnly = _linkId.StartsWith(HoverOnlyPrefix, StringComparison.Ordinal);
            if (isHoverOnly || _clicked == null)
            {
                return;
            }
            _clicked(_linkId);
        }

        private static void OnOver(TextElement _label, string _linkId, Func<string, string> _tooltipFor)
        {
            bool isHoverOnly = _linkId.StartsWith(HoverOnlyPrefix, StringComparison.Ordinal);
            _label.tooltip = isHoverOnly ? "" : TooltipOrEmpty(_tooltipFor, _linkId);
            string original = OriginalText(_label);
            // Unity's text selection breaks if the text changes under it, so selection pauses while a link is lit.
            if (_label is Label selectable)
            {
                selectable.selection.isSelectable = false;
            }
            _label.text = Brightened(original, _linkId);
        }

        private static void OnOut(TextElement _label)
        {
            _label.tooltip = "";
            if (_label.userData is HoverText saved)
            {
                _label.text = saved.Original;
            }
            if (_label is Label selectable)
            {
                selectable.selection.isSelectable = true;
            }
        }

        // The text as built, kept on the label the first time it is hovered.
        private static string OriginalText(TextElement _label)
        {
            if (_label.userData is HoverText saved && _label.text != null)
            {
                return saved.Original;
            }
            HoverText fresh = new HoverText(_label.text);
            if (_label.userData == null)
            {
                _label.userData = fresh;
            }
            return fresh.Original;
        }

        // Raises the colours inside every <link="id"> ... </link> with this id; everything else is left as it was.
        private static string Brightened(string _text, string _linkId)
        {
            string open = "<link=\"" + _linkId + "\">";
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            int position = 0;
            while (true)
            {
                int start = _text.IndexOf(open, position, StringComparison.Ordinal);
                if (start < 0)
                {
                    break;
                }
                int end = _text.IndexOf("</link>", start, StringComparison.Ordinal);
                if (end < 0)
                {
                    break;
                }
                result.Append(_text, position, start - position);
                string inside = _text.Substring(start, end - start);
                result.Append(RaiseColours(inside));
                position = end;
            }
            result.Append(_text, position, _text.Length - position);
            return result.ToString();
        }

        private static string RaiseColours(string _span)
        {
            string raised = _span.Replace("<color=" + DocsTheme.Link + ">", "<color=" + DocsTheme.LinkHover + ">");
            raised = raised.Replace("<color=" + DocsTheme.InlineCode + ">", "<color=" + DocsTheme.InlineCodeHover + ">");
            raised = raised.Replace("<color=" + DocsTheme.Term + ">", "<color=" + DocsTheme.TermHover + ">");
            raised = raised.Replace("<mark=" + DocsTheme.InlineCodeBackground + ">", "<mark=" + DocsTheme.InlineCodeBackgroundHover + ">");
            return raised;
        }

        private static string TooltipOrEmpty(Func<string, string> _tooltipFor, string _linkId)
        {
            if (_tooltipFor == null)
            {
                return "";
            }
            string tooltip = _tooltipFor(_linkId);
            return tooltip ?? "";
        }

        private sealed class HoverText
        {
            public readonly string Original;

            public HoverText(string _original)
            {
                Original = _original;
            }
        }
    }
}
