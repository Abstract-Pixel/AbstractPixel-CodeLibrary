using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>A small icon in the style sheet's text colour. Drawn, because a player's default font may lack the symbols.</summary>
    internal class DebuggerIcon : VisualElement
    {
        private readonly DebuggerIconShape shape;

        public DebuggerIcon(DebuggerIconShape _shape, string _class)
        {
            shape = _shape;
            AddToClassList(_class);
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext _context)
        {
            float width = contentRect.width;
            float height = contentRect.height;
            Painter2D painter = _context.painter2D;
            painter.fillColor = resolvedStyle.color;
            painter.BeginPath();
            switch (shape)
            {
                case DebuggerIconShape.Entered:
                    Polygon(painter, new Vector2(0f, 0f), new Vector2(width, height / 2f), new Vector2(0f, height));
                    break;
                case DebuggerIconShape.Applied:
                    Polygon(painter, new Vector2(width / 2f, 0f), new Vector2(width, height / 2f), new Vector2(width / 2f, height), new Vector2(0f, height / 2f));
                    break;
                case DebuggerIconShape.Left:
                    Polygon(painter, new Vector2(width, 0f), new Vector2(0f, height / 2f), new Vector2(width, height));
                    break;
                case DebuggerIconShape.Warning:
                    Polygon(painter, new Vector2(width / 2f, 0f), new Vector2(width, height), new Vector2(0f, height));
                    break;
            }
            painter.Fill();
        }

        private static void Polygon(Painter2D _painter, params Vector2[] _corners)
        {
            _painter.MoveTo(_corners[0]);
            for (int i = 1; i < _corners.Length; i++)
            {
                _painter.LineTo(_corners[i]);
            }
            _painter.ClosePath();
        }
    }
}
