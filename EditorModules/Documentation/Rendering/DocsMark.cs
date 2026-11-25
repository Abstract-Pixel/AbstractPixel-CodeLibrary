namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>A numbered mark drawn over an image: a position from 0 to 1 of its width and height, and a label.</summary>
    /// <remarks>
    /// - With a width and height (also 0 to 1), it outlines that rectangle and puts its number at the top left.
    /// </remarks>
    public class DocsMark
    {
        public int Number { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public string Label { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public bool IsRectangle
        {
            get { return Width > 0f && Height > 0f; }
        }

        public DocsMark(int _number, float _x, float _y, string _label) : this(_number, _x, _y, _label, 0f, 0f)
        {
        }

        public DocsMark(int _number, float _x, float _y, string _label, float _width, float _height)
        {
            Number = _number;
            X = _x;
            Y = _y;
            Label = _label;
            Width = _width;
            Height = _height;
        }
    }
}
