using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>One row of the contents tree: a title, the topic it opens (empty for a section) and its children.</summary>
    public class SceneDocsContentsEntry
    {
        public string Title { get; private set; }
        public string Topic { get; private set; }
        public List<SceneDocsContentsEntry> Children { get; private set; } = new List<SceneDocsContentsEntry>();

        public SceneDocsContentsEntry(string _title, string _topic)
        {
            Title = _title;
            Topic = _topic;
        }
    }
}
