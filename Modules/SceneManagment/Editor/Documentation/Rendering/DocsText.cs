using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Prose: a wrapping, selectable rich-text label whose links go through the host.</summary>
    public static class DocsText
    {
        public static Label Create(string _markdown, string _className, DocsRenderContext _context)
        {
            string pagePath = _context.Page.Path;
            IDocsHost host = _context.Host;
            System.Func<string, string> codeLink = null;
            if (_context.Page.IsNewLayout)
            {
                codeLink = _code => host.CodeLinkTarget(_code, pagePath);
            }
            Label label = new Label(DocsInline.ToRichText(_markdown, codeLink));
            label.enableRichText = true;
            label.selection.isSelectable = true;
            label.focusable = true;
            label.AddToClassList("docs-text");
            if (!string.IsNullOrEmpty(_className))
            {
                label.AddToClassList(_className);
            }
            DocsLinkEvents.Register(label, _target => host.OpenLink(_target, pagePath), host.TooltipFor);
            return label;
        }
    }
}
