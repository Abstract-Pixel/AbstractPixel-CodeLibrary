using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A screenshot with numbered marks drawn over it by the window (sharp at any size, themed), and beside it the mark
    /// labels and the alt text as a caption. The image keeps its aspect ratio and never grows past its own width.
    /// </summary>
    public static class DocsImage
    {
        public static VisualElement Create(DocsBlock _block, DocsRenderContext _context)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-figure");
            Texture2D texture = _context.Host.LoadImage(_block.ImagePath, _context.Page.Path);
            if (texture == null)
            {
                Label missing = new Label("Missing image: " + _block.ImagePath);
                missing.AddToClassList("docs-image-missing");
                box.Add(missing);
                return box;
            }
            VisualElement frame = new VisualElement();
            frame.AddToClassList("docs-image-frame");
            Image image = new Image();
            image.image = texture;
            image.scaleMode = ScaleMode.ScaleToFit;
            image.AddToClassList("docs-image");
            image.tooltip = _block.Text;
            frame.Add(image);
            foreach (DocsMark mark in _block.Marks)
            {
                frame.Add(MarkCircle(mark));
            }
            // Width and height follow the texture; positions are shares of the frame. USS cannot express either.
            frame.style.maxWidth = texture.width;
            frame.style.flexBasis = texture.width;
            frame.RegisterCallback<GeometryChangedEvent>(_event => KeepAspect(frame, texture));
            box.AddToClassList("docs-figure-row");
            box.Add(frame);

            // The explanation sits beside the image, where the page has room; a narrow page wraps it underneath.
            VisualElement side = new VisualElement();
            side.AddToClassList("docs-figure-side");
            if (_block.Marks.Count > 0)
            {
                side.Add(MarkLabels(_block));
            }
            Label caption = new Label(_block.Text);
            caption.AddToClassList("docs-caption");
            side.Add(caption);
            box.Add(side);
            return box;
        }

        private static void KeepAspect(VisualElement _frame, Texture2D _texture)
        {
            float width = _frame.resolvedStyle.width;
            if (float.IsNaN(width) || width <= 0f)
            {
                return;
            }
            float height = width * _texture.height / _texture.width;
            if (Mathf.Abs(_frame.resolvedStyle.height - height) > 0.5f)
            {
                _frame.style.height = height;
            }
        }

        private static VisualElement MarkCircle(DocsMark _mark)
        {
            Label circle = new Label(_mark.Number.ToString());
            circle.AddToClassList("docs-mark");
            circle.tooltip = _mark.Label;
            circle.style.left = Length.Percent(_mark.X * 100f);
            circle.style.top = Length.Percent(_mark.Y * 100f);
            return circle;
        }

        private static VisualElement MarkLabels(DocsBlock _block)
        {
            VisualElement list = new VisualElement();
            list.AddToClassList("docs-mark-list");
            foreach (DocsMark mark in _block.Marks)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-mark-row");
                Label number = new Label(mark.Number.ToString());
                number.AddToClassList("docs-mark-number");
                row.Add(number);
                Label text = new Label(mark.Label);
                text.AddToClassList("docs-text");
                row.Add(text);
                list.Add(row);
            }
            return list;
        }
    }
}
