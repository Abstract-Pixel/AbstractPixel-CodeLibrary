using System.Collections.Generic;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>One list item: its inline text, an optional nested list and an optional "done-when" message.</summary>
    public class DocsListItem
    {
        public string Text { get; set; }
        public DocsBlock NestedList { get; set; }
        public string DoneWhen { get; set; }
        public List<DocsDirective> Directives { get; private set; }

        public DocsListItem(string _text)
        {
            Text = _text;
            Directives = new List<DocsDirective>();
        }
    }
}
