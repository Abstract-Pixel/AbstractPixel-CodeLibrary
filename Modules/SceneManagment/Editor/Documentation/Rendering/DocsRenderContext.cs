using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>One page being drawn: its parsed form, the host, the step lists that tick and each heading's row.</summary>
    public class DocsRenderContext
    {
        public DocsPage Page { get; private set; }
        public IDocsHost Host { get; private set; }
        public List<DocsStepList> StepLists { get; private set; } = new List<DocsStepList>();
        public Dictionary<string, VisualElement> HeadingRows { get; private set; } = new Dictionary<string, VisualElement>();

        public DocsRenderContext(DocsPage _page, IDocsHost _host)
        {
            Page = _page;
            Host = _host;
        }
    }
}
