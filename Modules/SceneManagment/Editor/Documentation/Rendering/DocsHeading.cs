namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>A heading of a page: its id (GitHub-style slug), text, level and ladder pill (-1 for none).</summary>
    public class DocsHeading
    {
        public string Id { get; private set; }
        public string Text { get; private set; }
        public int Level { get; private set; }
        public int Pill { get; private set; }

        public DocsHeading(string _id, string _text, int _level, int _pill)
        {
            Id = _id;
            Text = _text;
            Level = _level;
            Pill = _pill;
        }
    }
}
