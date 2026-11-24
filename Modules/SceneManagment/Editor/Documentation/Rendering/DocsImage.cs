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

        // A box with one row per mark: its number, the name of the thing, and one line on what it is or does.
        // A mark label written "Name: what it does" splits at the first ": ".
        private static VisualElement MarkLabels(DocsBlock _block)
        {
            VisualElement list = new VisualElement();
            list.AddToClassList("docs-mark-list");
            list.AddToClassList("docs-mark-table");
            foreach (DocsMark mark in _block.Marks)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-mark-row");
                row.EnableInClassList("docs-mark-row-alt", list.childCount % 2 == 1);
                Label number = new Label(mark.Number.ToString());
                number.AddToClassList("docs-mark-number");
                row.Add(number);

                string name = mark.Label;
                string meaning = string.Empty;
                int split = mark.Label.IndexOf(": ", System.StringComparison.Ordinal);
                if (split > 0)
                {
                    name = mark.Label.Substring(0, split);
                    meaning = mark.Label.Substring(split + 2);
                    // Each meaning reads as a sentence.
                    meaning = char.ToUpperInvariant(meaning[0]) + meaning.Substring(1);
                }
                Label nameLabel = new Label(name);
                nameLabel.AddToClassList("docs-text");
                nameLabel.AddToClassList("docs-mark-name");
                row.Add(nameLabel);
                if (meaning.Length > 0)
                {
                    Label meaningLabel = new Label(meaning);
                    meaningLabel.AddToClassList("docs-text");
                    meaningLabel.AddToClassList("docs-mark-meaning");
                    row.Add(meaningLabel);
                }
                list.Add(row);
            }
            return list;
        }
    }
}
