using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>A parsed page: its title, blocks, headings (for "On this page" and anchors) and problems found.</summary>
    public class DocsPage
    {
        public string Path { get; private set; }
        public string Title { get; set; }
        public int TitlePill { get; set; } = -1;

        /// <summary>The layout a page opts into with a "&lt;!-- layout new --&gt;" line, or null for the old one.</summary>
        public string Layout { get; set; }
        public List<DocsBlock> Blocks { get; private set; } = new List<DocsBlock>();
        public List<DocsHeading> Headings { get; private set; } = new List<DocsHeading>();
        public List<string> Problems { get; private set; } = new List<string>();

        public DocsPage(string _path)
        {
            Path = _path;
        }

        /// <summary>True when a heading on this page has this id.</summary>
        public bool HasHeading(string _id)
        {
            foreach (DocsHeading heading in Headings)
            {
                if (heading.Id == _id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
