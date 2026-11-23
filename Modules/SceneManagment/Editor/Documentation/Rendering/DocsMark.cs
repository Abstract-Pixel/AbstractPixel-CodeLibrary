namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>A numbered mark drawn over an image: a position from 0 to 1 of its width and height, and a label.</summary>
    public class DocsMark
    {
        public int Number { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public string Label { get; private set; }

        public DocsMark(int _number, float _x, float _y, string _label)
        {
            Number = _number;
            X = _x;
            Y = _y;
            Label = _label;
        }
    }
}
