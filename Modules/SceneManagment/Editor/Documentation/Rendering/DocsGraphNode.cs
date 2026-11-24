using System;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// One box of a docs graph: an id, a title, a short line, an optional layer (column) hint, and what a click does:
    /// open the node's own graph (<see cref="Inner"/>) or run <see cref="Clicked"/>.
    /// </summary>
    public class DocsGraphNode
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Line { get; private set; }
        public bool IsKey { get; set; }
        public int Layer { get; set; } = -1;
        public Action Clicked { get; set; }
        public DocsGraph Inner { get; set; }
        /// <summary>"api", "interface", "enum", "editor" or "other"; empty for folder boxes. Picks the box's edge colour.</summary>
        public string Category { get; set; } = "";

        public DocsGraphNode(string _id, string _title, string _line)
        {
            Id = _id;
            Title = _title;
            Line = _line;
        }
    }
}
