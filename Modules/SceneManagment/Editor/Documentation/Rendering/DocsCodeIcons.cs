using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Small vector icons for the code block's actions, drawn in the action's text colour.</summary>
    /// <remarks>
    /// - Copy: two overlapping rectangles.
    /// - Open in IDE: Visual Studio's icon in grey, with an angled arrow.
    /// </remarks>
    public static class DocsCodeIcons
    {
        private const float Size = 14f;

        public static VisualElement Copy()
        {
            VisualElement icon = NewIcon();
            icon.generateVisualContent += _context =>
            {
                Painter2D paint = _context.painter2D;
                paint.strokeColor = icon.resolvedStyle.color;
                paint.lineWidth = 1.2f;
                // The back sheet, then the front sheet overlapping its lower right.
                StrokeRect(paint, 1.5f, 1.5f, 8f, 9f);
                StrokeRect(paint, 4.5f, 4.5f, 8f, 9f);
            };
            return icon;
        }

        // Visual Studio's icon, in grey so it suits the muted header (Images/Icons/visual-studio-grey.png, made from
        // the public-domain Wikimedia Commons file "Visual Studio Icon 2022.svg"), with an angled arrow drawn over its corner.
        private const string VisualStudioIconPath = SceneDocsPaths.Module + "Editor/Documentation/Images/Icons/visual-studio-grey.png";

        public static VisualElement OpenInIde()
        {
            VisualElement icon = NewIcon();
            Texture2D logo = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(VisualStudioIconPath);
            if (logo != null)
            {
                icon.style.backgroundImage = logo;
            }
            icon.generateVisualContent += _context =>
            {
                Painter2D paint = _context.painter2D;
                paint.strokeColor = icon.resolvedStyle.color;
                paint.lineWidth = 1.3f;
                paint.BeginPath();
                paint.MoveTo(new Vector2(9.5f, 4.5f));
                paint.LineTo(new Vector2(13.5f, 0.5f));
                paint.MoveTo(new Vector2(10.5f, 0.5f));
                paint.LineTo(new Vector2(13.5f, 0.5f));
                paint.LineTo(new Vector2(13.5f, 3.5f));
                paint.Stroke();
            };
            return icon;
        }

        // An action: the icon alone, and its text slides out beside it on hover.
        public static Button Action(VisualElement _icon, string _text, string _tooltip, System.Action _clicked)
        {
            Button button = new Button(_clicked);
            button.AddToClassList("docs-code-action");
            button.tooltip = _tooltip;
            button.Add(_icon);
            Label label = new Label(_text);
            label.AddToClassList("docs-code-action-label");
            button.Add(label);
            return button;
        }

        private static VisualElement NewIcon()
        {
            VisualElement icon = new VisualElement();
            icon.AddToClassList("docs-code-icon");
            icon.style.width = Size;
            icon.style.height = Size;
            return icon;
        }

        private static void StrokeRect(Painter2D _paint, float _x, float _y, float _width, float _height)
        {
            _paint.BeginPath();
            _paint.MoveTo(new Vector2(_x, _y));
            _paint.LineTo(new Vector2(_x + _width, _y));
            _paint.LineTo(new Vector2(_x + _width, _y + _height));
            _paint.LineTo(new Vector2(_x, _y + _height));
            _paint.ClosePath();
            _paint.Stroke();
        }
    }
}
