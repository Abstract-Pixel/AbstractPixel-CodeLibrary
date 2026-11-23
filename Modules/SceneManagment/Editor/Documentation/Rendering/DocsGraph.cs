using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>A graph to draw: its title, its boxes and its arrows (from id, to id). Read by <see cref="DocsGraphView"/>.</summary>
    public class DocsGraph
    {
        public string Title { get; private set; }
        public List<DocsGraphNode> Nodes { get; private set; } = new List<DocsGraphNode>();
        public List<KeyValuePair<string, string>> Edges { get; private set; } = new List<KeyValuePair<string, string>>();

        public DocsGraph(string _title)
        {
            Title = _title;
        }

        public DocsGraphNode Add(string _id, string _title, string _line)
        {
            DocsGraphNode node = new DocsGraphNode(_id, _title, _line);
            Nodes.Add(node);
            return node;
        }

        public void Link(string _from, string _to)
        {
            Edges.Add(new KeyValuePair<string, string>(_from, _to));
        }

        public DocsGraphNode Find(string _id)
        {
            return Nodes.Find(_node => _node.Id == _id);
        }
    }
}
