using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// What the renderer asks of the module that shows the pages: where links go, what a term means, how to draw a
    /// directive, and where an image or a script file is. Keeps the renderer free of any scene system type.
    /// </summary>
    public interface IDocsHost
    {
        /// <summary>Follows a link written on a page (a relative .md path with an optional #anchor, term:slug or http).</summary>
        void OpenLink(string _target, string _fromPagePath);

        /// <summary>The tooltip for a link: a term's definition, or where a page link goes. Null for none.</summary>
        string TooltipFor(string _target);

        /// <summary>The element for a directive line, or null to show nothing.</summary>
        VisualElement RenderDirective(DocsDirective _directive, DocsRenderContext _context);

        /// <summary>The image a page names, or null when it is missing.</summary>
        Texture2D LoadImage(string _imagePath, string _fromPagePath);

        /// <summary>The asset path of the script a code block names (source=...), or null.</summary>
        string ResolveSourcePath(string _source, string _fromPagePath);
    }
}
