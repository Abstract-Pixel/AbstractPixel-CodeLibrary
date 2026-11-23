using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// One parsed block. Which fields are set depends on <see cref="Kind"/>: Text for headings, paragraphs and code;
    /// Items for lists; Rows for tables; Children for quotes, callouts and foldouts; Directive for a directive line.
    /// </summary>
    public class DocsBlock
    {
        public DocsBlockKind Kind { get; private set; }
        public int Line { get; private set; }
        public string Text { get; set; }
        public int Level { get; set; }
        public int Pill { get; set; } = -1;
        public string Id { get; set; }
        public bool Ordered { get; set; }
        public string Info { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public DocsDirective Directive { get; set; }
        public List<DocsListItem> Items { get; private set; } = new List<DocsListItem>();
        public List<List<string>> Rows { get; private set; } = new List<List<string>>();
        public List<DocsBlock> Children { get; private set; } = new List<DocsBlock>();
        public List<DocsMark> Marks { get; private set; } = new List<DocsMark>();

        public DocsBlock(DocsBlockKind _kind, int _line)
        {
            Kind = _kind;
            Line = _line;
        }
    }
}
