using System.Collections.Generic;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>One row of the contents tree: a title, the topic it opens (empty for a section) and its children.</summary>
    public class DocsContentsEntry
    {
        public string Title { get; private set; }
        public string Topic { get; private set; }
        public List<DocsContentsEntry> Children { get; private set; } = new List<DocsContentsEntry>();

        public DocsContentsEntry(string _title, string _topic)
        {
            Title = _title;
            Topic = _topic;
        }
    }
}
