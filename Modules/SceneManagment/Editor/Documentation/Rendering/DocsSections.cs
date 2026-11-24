using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Groups a new-layout page two levels deep so each part's start and end show. A "##" heading starts a numbered
    /// section: a box with a header bar, holding everything up to the next "##". A "###" heading or a paragraph that is
    /// only bold text ("**Or do it yourself**") starts a numbered part: a foldout strip across the box, closed by
    /// default, up to the next strip or the box's end. On a "Try it in the sample" strip the first action sits on the
    /// strip. A button with fact rows under it becomes a foldout too: the button on its strip, the rows inside.
    /// Boxes have no inner padding: tables and rows meet the box's edges, text keeps its own margin. (New layout only.)
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsSections
    {
        public const string SampleTitle = "Try it in the sample";
        private static readonly Regex BoldLine = new Regex(@"^\*\*([^*]+)\*\*$");

        public static void AddBlocks(VisualElement _page, List<DocsBlock> _blocks, DocsRenderContext _context)
        {
            VisualElement sectionBody = null;
            VisualElement partBody = null;
            Foldout sample = null;
            bool sampleHasAction = false;
            VisualElement lastAction = null;
            int partNumber = 0;
            List<VisualElement> bodies = new List<VisualElement>();
            foreach (DocsBlock block in _blocks)
            {
                if (block.Kind == DocsBlockKind.Heading && block.Level <= 2)
                {
                    _context.SectionCount++;
                    partNumber = 0;
                    VisualElement section = new VisualElement();
                    section.AddToClassList("docs-section");
                    section.Add(DocsRenderer.HeadingRow(_context.SectionCount + ".  " + block.Text, block.Id, block.Level, block.Pill, _context));
                    sectionBody = new VisualElement();
                    sectionBody.AddToClassList("docs-section-body");
                    bodies.Add(sectionBody);
                    section.Add(sectionBody);
                    _page.Add(section);
                    partBody = null;
                    sample = null;
                    continue;
                }
                string partTitle = PartTitle(block);
                if (partTitle != null && sectionBody != null)
                {
                    partNumber++;
                    string numbered = _context.SectionCount + "." + partNumber + "  " + partTitle;
                    Foldout part = DocsSessionFoldout.Create(numbered, _context.Page.Path + "#part");
                    part.RemoveFromClassList("docs-fold");
                    part.AddToClassList("docs-part-fold");
                    if (block.Kind == DocsBlockKind.Heading)
                    {
                        _context.HeadingRows[block.Id] = part;
                    }
                    sectionBody.Add(part);
                    partBody = part.contentContainer;
                    bodies.Add(partBody);
                    sample = partTitle == SampleTitle ? part : null;
                    sampleHasAction = false;
                    continue;
                }
                // Member tables are numbered sections of their own, so the section before them ends here.
                if (block.Kind == DocsBlockKind.Directive && block.Directive.Name == "members")
                {
                    sectionBody = null;
                    partBody = null;
                    sample = null;
                }
                VisualElement element = DocsRenderer.RenderBlock(block, _context);
                if (element == null)
                {
                    continue;
                }
                if (sample != null && !sampleHasAction && IsSampleAction(block))
                {
                    sampleHasAction = true;
                    AddToStrip(sample, element);
                    AddSampleFolder(sample, block.Directive.Argument(1));
                    continue;
                }
                VisualElement target = partBody ?? sectionBody ?? _page;
                // A button with fact rows right under it: one foldout, the button on its strip and the rows inside.
                if (lastAction != null && lastAction.parent == target && block.Kind == DocsBlockKind.List && DocsFactRows.Applies(block))
                {
                    string label = SceneDocsActions.LabelOf((DocsDirective)lastAction.userData);
                    Foldout group = DocsSessionFoldout.Create("", _context.Page.Path + "#button-" + label);
                    group.RemoveFromClassList("docs-fold");
                    group.AddToClassList("docs-action-fold");
                    group.AddToClassList("docs-inner-box");
                    target.Insert(target.IndexOf(lastAction), group);
                    AddToStrip(group, lastAction);
                    element.AddToClassList("docs-action-group-facts");
                    group.Add(element);
                    lastAction = null;
                    continue;
                }
                target.Add(element);
                bool isAction = block.Kind == DocsBlockKind.Directive && block.Directive.Name == "action";
                lastAction = isAction ? element : null;
            }
            // USS has no :first-child or :last-child, so each box's first and last blocks are marked here.
            foreach (VisualElement body in bodies)
            {
                if (body.childCount > 0)
                {
                    body[0].AddToClassList("docs-first");
                    body[body.childCount - 1].AddToClassList("docs-last");
                }
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

        private static bool IsSampleAction(DocsBlock _block)
        {
            return _block.Kind == DocsBlockKind.Directive && _block.Directive.Name == "action" && _block.Directive.Argument(0) == "open-sample";
        }

        // Puts a button on a foldout's strip, after its title, where a click runs the button and not the foldout.
        private static void AddToStrip(Foldout _fold, VisualElement _button)
        {
            VisualElement strip = _fold.Q<Toggle>().Q(className: Toggle.inputUssClassName);
            _button.AddToClassList("docs-strip-action");
            _button.RegisterCallback<ClickEvent>(_click => _click.StopPropagation());
            strip.Add(_button);
        }

        private static void AddSampleFolder(Foldout _sample, string _number)
        {
            string folder = SceneDocsTopics.SampleFolder(_number);
            if (folder == null)
            {
                return;
            }
            string shortPath = folder.Contains("/Samples/") ? folder.Substring(folder.IndexOf("/Samples/") + 1) : folder;
            Button path = new Button(() => SelectFolder(folder));
            path.text = shortPath;
            path.tooltip = "Selects this sample's folder in the Project window";
            path.AddToClassList("docs-sample-path");
            AddToStrip(_sample, path);
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
