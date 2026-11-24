using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Turns a parsed page into UI Toolkit elements: title row, headings with pills and anchors, prose, lists, step
    /// lists, tables, callouts, foldouts, code blocks, images and directives (drawn by the host).
    /// </summary>
    public static class DocsRenderer
    {
        public static VisualElement Render(DocsRenderContext _context)
        {
            VisualElement page = new VisualElement();
            page.AddToClassList("docs-page-content");
            page.Add(TitleRow(_context.Page));
            AddBlocks(page, _context.Page.Blocks, _context);
            return page;
        }

        private static VisualElement TitleRow(DocsPage _page)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-title-row");
            Label title = new Label(_page.Title ?? "");
            title.AddToClassList("docs-title");
            title.selection.isSelectable = true;
            row.Add(title);
            if (_page.TitlePill >= 0)
            {
                row.Add(DocsPill.Ladder(_page.TitlePill));
            }
            return row;
        }

        public static void AddBlocks(VisualElement _parent, List<DocsBlock> _blocks, DocsRenderContext _context)
        {
            foreach (DocsBlock block in _blocks)
            {
                VisualElement element = RenderBlock(block, _context);
                if (element != null)
                {
                    _parent.Add(element);
                }
            }
        }

        private static VisualElement RenderBlock(DocsBlock _block, DocsRenderContext _context)
        {
            switch (_block.Kind)
            {
                case DocsBlockKind.Heading: return Heading(_block, _context);
                case DocsBlockKind.Paragraph: return DocsText.Create(_block.Text, "docs-paragraph", _context);
                case DocsBlockKind.List: return List(_block, _context, 0);
                case DocsBlockKind.Table: return Table(_block, _context);
                case DocsBlockKind.Quote: return Quote(_block, _context);
                case DocsBlockKind.Callout: return Callout(_block.Info, _block.Children, _context);
                case DocsBlockKind.Code: return DocsCodeBlock.Create(_block, _context);
                case DocsBlockKind.Details: return Details(_block, _context);
                case DocsBlockKind.Image: return DocsImage.Create(_block, _context);
                case DocsBlockKind.Directive: return _context.Host.RenderDirective(_block.Directive, _context);
                default: return null;
            }
        }

        private static VisualElement Heading(DocsBlock _block, DocsRenderContext _context)
        {
            return HeadingRow(_block.Text, _block.Id, _block.Level, _block.Pill, _context);
        }

        /// <summary>A heading row with its pill and "#" copy-link, registered so links and "On this page" can reach it.</summary>
        public static VisualElement HeadingRow(string _text, string _id, int _level, int _pill, DocsRenderContext _context)
        {
            DocsBlock _block = new DocsBlock(DocsBlockKind.Heading, 0);
            _block.Text = _text;
            _block.Id = _id;
            _block.Level = _level;
            _block.Pill = _pill;
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-heading-row");
            row.AddToClassList("docs-h" + _block.Level);
            row.name = _block.Id;
            Label text = new Label(_block.Text);
            text.AddToClassList("docs-heading");
            text.selection.isSelectable = true;
            row.Add(text);
            if (_block.Pill >= 0)
            {
                row.Add(DocsPill.Ladder(_block.Pill));
            }
            string topic = TopicOf(_context.Page.Path) + "#" + _block.Id;
            Button anchor = new Button(() => EditorGUIUtility.systemCopyBuffer = topic);
            anchor.text = "#";
            anchor.tooltip = "Copies this section's topic id: " + topic;
            anchor.AddToClassList("docs-anchor");
            row.Add(anchor);
            _context.HeadingRows[_block.Id] = row;
            return row;
        }

        // "guide/preload.md" becomes "guide/preload".
        private static string TopicOf(string _pagePath)
        {
            if (_pagePath.EndsWith(".md"))
            {
                return _pagePath.Substring(0, _pagePath.Length - 3).ToLowerInvariant();
            }
            return _pagePath;
        }

        private static VisualElement List(DocsBlock _block, DocsRenderContext _context, int _depth)
        {
            if (_block.Ordered && HasDoneWhen(_block))
            {
                return StepList(_block, _context);
            }
            if (_context.Page.IsNewLayout && DocsFactRows.Applies(_block))
            {
                return DocsFactRows.Create(_block, _context);
            }
            if (_context.Page.IsNewLayout && DocsFlow.Applies(_block))
            {
                return DocsFlow.Create(_block, _context);
            }
            VisualElement list = new VisualElement();
            list.AddToClassList("docs-list");
            if (_depth > 0)
            {
                list.AddToClassList("docs-list-nested");
            }
            int number = 1;
            foreach (DocsListItem item in _block.Items)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-list-row");
                Label marker = new Label(_block.Ordered ? number + "." : "•");
                marker.AddToClassList("docs-list-marker");
                row.Add(marker);
                row.Add(ItemContent(item, _context, _depth));
                list.Add(row);
                number++;
            }
            return list;
        }

        private static VisualElement ItemContent(DocsListItem _item, DocsRenderContext _context, int _depth)
        {
            VisualElement content = new VisualElement();
            content.AddToClassList("docs-list-content");
            content.Add(DocsText.Create(_item.Text, null, _context));
            foreach (DocsDirective directive in _item.Directives)
            {
                VisualElement drawn = _context.Host.RenderDirective(directive, _context);
                if (drawn != null)
                {
                    content.Add(drawn);
                }
            }
            if (_item.NestedList != null)
            {
                content.Add(List(_item.NestedList, _context, _depth + 1));
            }
            return content;
        }

        private static bool HasDoneWhen(DocsBlock _block)
        {
            foreach (DocsListItem item in _block.Items)
            {
                if (!string.IsNullOrEmpty(item.DoneWhen))
                {
                    return true;
                }
            }
            return false;
        }

        private static VisualElement StepList(DocsBlock _block, DocsRenderContext _context)
        {
            DocsStepList steps = new DocsStepList();
            foreach (DocsListItem item in _block.Items)
            {
                steps.AddStep(ItemContent(item, _context, 0), item.DoneWhen);
            }
            _context.StepLists.Add(steps);
            return steps.Root;
        }

        private static VisualElement Table(DocsBlock _block, DocsRenderContext _context)
        {
            VisualElement table = new VisualElement();
            table.AddToClassList("docs-table");
            for (int rowIndex = 0; rowIndex < _block.Rows.Count; rowIndex++)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-table-row");
                bool isHeader = rowIndex == 0;
                row.EnableInClassList("docs-table-header", isHeader);
                row.EnableInClassList("docs-table-alternate", !isHeader && rowIndex % 2 == 0);
                foreach (string cell in _block.Rows[rowIndex])
                {
                    Label cellLabel = DocsText.Create(cell, "docs-table-cell", _context);
                    cellLabel.EnableInClassList("docs-table-first", row.childCount == 0);
                    row.Add(cellLabel);
                }
                table.Add(row);
            }
            return table;
        }

        private static VisualElement Quote(DocsBlock _block, DocsRenderContext _context)
        {
            VisualElement quote = new VisualElement();
            quote.AddToClassList("docs-quote");
            AddBlocks(quote, _block.Children, _context);
            return quote;
        }

        private static VisualElement Callout(string _kind, List<DocsBlock> _children, DocsRenderContext _context)
        {
            VisualElement body;
            VisualElement box = DocsCallout.Create(_kind, out body);
            AddBlocks(body, _children, _context);
            return box;
        }

        // A closed foldout. "Why it works" gets the callout look; the others ("If it goes wrong") stay plain.
        private static VisualElement Details(DocsBlock _block, DocsRenderContext _context)
        {
            if (_context.Page.IsNewLayout)
            {
                Foldout fold = DocsSessionFoldout.Create(_block.Title, _context.Page.Path);
                fold.EnableInClassList("docs-fold-why", _block.Title == "Why it works");
                fold.EnableInClassList("docs-fold-wrong", _block.Title == "If it goes wrong");
                AddBlocks(fold.contentContainer, _block.Children, _context);
                return fold;
            }
            Foldout foldout = new Foldout();
            foldout.text = _block.Title;
            foldout.value = false;
            foldout.AddToClassList("docs-foldout");
            bool isWhy = _block.Title == "Why it works";
            if (isWhy)
            {
                foldout.AddToClassList("docs-foldout-why");
            }
            AddBlocks(foldout.contentContainer, _block.Children, _context);
            return foldout;
        }
    }
}
