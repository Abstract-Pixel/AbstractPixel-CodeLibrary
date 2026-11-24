using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Groups a new-layout page into sections and parts so each one's start and end show. A "##" heading starts a
    /// section: its heading bar, then everything up to the next "##" behind one line on the left. A "###" heading or a
    /// paragraph that is only bold text ("**Or do it yourself**") starts a part inside the section, behind its own
    /// line. A part titled "Try it in the sample" becomes one box, closed by default: its first action sits on the
    /// title line, and the steps show when it opens. (New layout only.)
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsSections
    {
        public const string SampleTitle = "Try it in the sample";
        private static readonly Regex BoldLine = new Regex(@"^\*\*([^*]+)\*\*$");

        public static void AddBlocks(VisualElement _page, System.Collections.Generic.List<DocsBlock> _blocks, DocsRenderContext _context)
        {
            VisualElement sectionBody = null;
            VisualElement partBody = null;
            DocsSampleBox sample = null;
            VisualElement lastAction = null;
            foreach (DocsBlock block in _blocks)
            {
                if (block.Kind == DocsBlockKind.Heading && block.Level <= 2)
                {
                    VisualElement section = new VisualElement();
                    section.AddToClassList("docs-section");
                    section.Add(DocsRenderer.RenderBlock(block, _context));
                    sectionBody = new VisualElement();
                    sectionBody.AddToClassList("docs-section-body");
                    section.Add(sectionBody);
                    _page.Add(section);
                    partBody = null;
                    sample = null;
                    continue;
                }
                string partTitle = PartTitle(block);
                if (partTitle != null && sectionBody != null)
                {
                    sample = null;
                    if (partTitle == SampleTitle)
                    {
                        sample = new DocsSampleBox(partTitle, _context.Page.Path);
                        sectionBody.Add(sample.Root);
                        partBody = sample.Body;
                        continue;
                    }
                    VisualElement part = new VisualElement();
                    part.AddToClassList("docs-part");
                    Label title = new Label(partTitle);
                    title.AddToClassList("docs-part-title");
                    title.selection.isSelectable = true;
                    part.Add(title);
                    if (block.Kind == DocsBlockKind.Heading)
                    {
                        _context.HeadingRows[block.Id] = part;
                    }
                    partBody = new VisualElement();
                    partBody.AddToClassList("docs-part-body");
                    part.Add(partBody);
                    sectionBody.Add(part);
                    continue;
                }
                VisualElement element = DocsRenderer.RenderBlock(block, _context);
                if (element == null)
                {
                    continue;
                }
                if (sample != null && sample.TakesIntoTitle(block, element))
                {
                    continue;
                }
                VisualElement target = partBody ?? sectionBody ?? _page;
                // A button with fact rows right under it: one group, so the rows read as what the button opens.
                if (lastAction != null && lastAction.parent == target && block.Kind == DocsBlockKind.List && DocsFactRows.Applies(block))
                {
                    VisualElement group = new VisualElement();
                    group.AddToClassList("docs-action-group");
                    target.Insert(target.IndexOf(lastAction), group);
                    group.Add(lastAction);
                    element.AddToClassList("docs-action-group-facts");
                    group.Add(element);
                    lastAction = null;
                    continue;
                }
                target.Add(element);
                bool isAction = block.Kind == DocsBlockKind.Directive && block.Directive.Name == "action";
                lastAction = isAction ? element : null;
            }
        }

        // The title of a part this block starts, or null.
        private static string PartTitle(DocsBlock _block)
        {
            if (_block.Kind == DocsBlockKind.Heading && _block.Level >= 3)
            {
                return _block.Text;
            }
            if (_block.Kind == DocsBlockKind.Paragraph)
            {
                Match match = BoldLine.Match((_block.Text ?? "").Trim());
                return match.Success ? match.Groups[1].Value : null;
            }
            return null;
        }
    }

    /// <summary>
    /// The "Try it in the sample" box. Closed: one line with the title, the Open sample button and the sample's folder
    /// (a click selects it in the Project window). Open: the steps. Stays open for the editor session once opened.
    /// </summary>
    public class DocsSampleBox
    {
        public readonly VisualElement Root = new VisualElement();
        public readonly VisualElement Body = new VisualElement();
        private readonly VisualElement titleLine = new VisualElement();
        private readonly Label arrow = new Label();
        private readonly string key;
        private bool hasAction;

        public DocsSampleBox(string _title, string _pagePath)
        {
            key = "scene-docs-sample:" + _pagePath;
            Root.AddToClassList("docs-sample-box");
            titleLine.AddToClassList("docs-sample-line");
            Button toggle = new Button(() => SetOpen(!IsOpen));
            toggle.AddToClassList("docs-sample-toggle");
            toggle.tooltip = "Shows or hides the steps";
            arrow.AddToClassList("docs-sample-arrow");
            toggle.Add(arrow);
            Label title = new Label(_title);
            title.AddToClassList("docs-sample-title");
            toggle.Add(title);
            titleLine.Add(toggle);
            Root.Add(titleLine);
            Body.AddToClassList("docs-sample-body");
            Root.Add(Body);
            SetOpen(SessionState.GetBool(key, false));
        }

        public bool IsOpen { get; private set; }

        public void SetOpen(bool _open)
        {
            IsOpen = _open;
            SessionState.SetBool(key, _open);
            arrow.text = _open ? "▾" : "▸";
            Body.style.display = _open ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList("docs-sample-open", _open);
        }

        /// <summary>Puts the first "open-sample" action on the title line, with the sample folder next to it.</summary>
        public bool TakesIntoTitle(DocsBlock _block, VisualElement _element)
        {
            if (hasAction || _block.Kind != DocsBlockKind.Directive || _block.Directive.Name != "action" || _block.Directive.Argument(0) != "open-sample")
            {
                return false;
            }
            hasAction = true;
            titleLine.Add(_element);
            string folder = SceneDocsTopics.SampleFolder(_block.Directive.Argument(1));
            if (folder != null)
            {
                string shortPath = folder.Contains("/Samples/") ? folder.Substring(folder.IndexOf("/Samples/") + 1) : folder;
                Button path = new Button(() => SelectFolder(folder));
                path.text = shortPath;
                path.tooltip = "Selects this sample's folder in the Project window";
                path.AddToClassList("docs-sample-path");
                titleLine.Add(path);
            }
            return true;
        }

        private static void SelectFolder(string _folder)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_folder);
            if (asset == null)
            {
                return;
            }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
