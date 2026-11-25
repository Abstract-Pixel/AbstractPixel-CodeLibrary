using System;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// An action chip: a small rounded button with a flat glyph and a verb, for actions that leave the page. The glyph
    /// is text in the label colour, so it stays flat and follows the skin.
    /// </summary>
    public static class DocsChip
    {
        public const string ClassName = "docs-chip";

        public static Button Create(string _glyph, string _text, string _tooltip, Action _clicked)
        {
            Button chip = new Button(_clicked);
            chip.AddToClassList(ClassName);
            chip.tooltip = _tooltip;
            if (!string.IsNullOrEmpty(_glyph))
            {
                Label glyph = new Label(_glyph);
                glyph.AddToClassList("docs-glyph");
                chip.Add(glyph);
            }
            Label label = new Label(_text);
            label.AddToClassList("docs-chip-label");
            chip.Add(label);
            return chip;
        }
    }
}
