using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Groups a new-layout page two levels deep so each part's start and end show. A "##" heading starts a numbered
    /// section: a box with a header bar, holding everything up to the next "##". A "###" heading or a paragraph that is
    /// only bold text ("**Or do it yourself**") starts a numbered part: a strip across the box, up to the next strip or
    /// the box's end. A part titled "Try it in the sample" is a foldout strip, closed by default, with its first action
    /// on the strip. A button with fact rows under it becomes one tinted box. (New layout only.)
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
            foreach (DocsBlock block in _blocks)
            {
                if (block.Kind == DocsBlockKind.Heading && block.Level <= 2)
                {
                    _context.SectionCount++;
                    int sectionNumber = _context.SectionCount;
                    partNumber = 0;
                    VisualElement section = new VisualElement();
                    section.AddToClassList("docs-section");
                    section.Add(DocsRenderer.HeadingRow(sectionNumber + ".  " + block.Text, block.Id, block.Level, block.Pill, _context));
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
                    partNumber++;
                    string numbered = _context.SectionCount + "." + partNumber + "  " + partTitle;
                    sample = null;
                    if (partTitle == SampleTitle)
                    {
                        sample = DocsSessionFoldout.Create(numbered, _context.Page.Path + "#sample");
                        sample.RemoveFromClassList("docs-fold");
                        sample.AddToClassList("docs-part-fold");
                        sampleHasAction = false;
                        sectionBody.Add(sample);
                        partBody = sample.contentContainer;
                        continue;
                    }
                    Label strip = new Label(numbered);
                    strip.AddToClassList("docs-part-strip");
                    strip.selection.isSelectable = true;
                    if (block.Kind == DocsBlockKind.Heading)
                    {
                        _context.HeadingRows[block.Id] = strip;
                    }
                    sectionBody.Add(strip);
                    partBody = new VisualElement();
                    partBody.AddToClassList("docs-part-body");
                    sectionBody.Add(partBody);
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
                    AddToStrip(sample, element, block.Directive.Argument(1));
                    continue;
                }
                VisualElement target = partBody ?? sectionBody ?? _page;
                // A button with fact rows right under it: one tinted box, so the rows read as what the button opens.
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

        private static bool IsSampleAction(DocsBlock _block)
        {
            return _block.Kind == DocsBlockKind.Directive && _block.Directive.Name == "action" && _block.Directive.Argument(0) == "open-sample";
        }

        // The Open sample button and the sample folder go on the foldout's strip, after its title.
        private static void AddToStrip(Foldout _sample, VisualElement _button, string _number)
        {
            VisualElement strip = _sample.Q<Toggle>().Q(className: Toggle.inputUssClassName);
            _button.AddToClassList("docs-part-fold-action");
            strip.Add(_button);
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
            strip.Add(path);
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
