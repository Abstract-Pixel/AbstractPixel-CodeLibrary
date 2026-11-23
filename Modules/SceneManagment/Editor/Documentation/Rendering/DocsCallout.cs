using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A callout box: Note and Warning with Unity's own console icons; "You should see" and "Why it works" with a flat
    /// glyph, so only messages that need attention carry a picture icon.
    /// </summary>
    public static class DocsCallout
    {
        public static VisualElement Create(string _kind, out VisualElement _body)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-callout");
            box.AddToClassList("docs-callout-" + _kind);
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-callout-header");
            header.Add(IconFor(_kind));
            Label title = new Label(TitleFor(_kind));
            title.AddToClassList("docs-callout-title");
            header.Add(title);
            box.Add(header);
            _body = new VisualElement();
            _body.AddToClassList("docs-callout-body");
            box.Add(_body);
            return box;
        }

        public static string TitleFor(string _kind)
        {
            switch (_kind)
            {
                case "warning": return "Warning";
                case "tip": return "You should see";
                case "why": return "Why it works";
                default: return "Note";
            }
        }

        private static VisualElement IconFor(string _kind)
        {
            bool usesGlyph = _kind == "tip" || _kind == "why";
            if (usesGlyph)
            {
                Label glyph = new Label(_kind == "tip" ? DocsIcons.GlyphTip : DocsIcons.GlyphWhy);
                glyph.AddToClassList("docs-glyph");
                glyph.AddToClassList("docs-callout-glyph");
                return glyph;
            }
            Image icon = new Image();
            icon.image = DocsIcons.Load(_kind == "warning" ? DocsIcons.Warning : DocsIcons.Note);
            icon.AddToClassList("docs-icon");
            return icon;
        }
    }
}
