using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// "On this page" as a tree beside the page that stays put while the page scrolls, drawn like Unity's tree views:
    /// a row per numbered section with a fold arrow, its numbered parts indented under a guide line. A click scrolls
    /// there (opening a closed part); the section you are reading shows as the selected row. Shown for new-layout
    /// pages when the window has room; the toolbar's "On this page" menu covers the rest.
    /// </summary>
    public class DocsPageRail
    {
        public const float MinWindowWidth = 1250f;
        public readonly VisualElement Root = new VisualElement();
        private readonly ScrollView pageScroll;
        private readonly ScrollView list = new ScrollView(ScrollViewMode.Vertical);
        private readonly List<VisualElement> sections = new List<VisualElement>();
        private readonly List<VisualElement> sectionRows = new List<VisualElement>();
        private readonly List<Foldout> parts = new List<Foldout>();
        private readonly List<VisualElement> partRows = new List<VisualElement>();
        private readonly List<int> partSection = new List<int>();
        private const long FlashMilliseconds = 1200;
        private readonly DocsRailResizer resizer;

        public DocsPageRail(ScrollView _pageScroll)
        {
            pageScroll = _pageScroll;
            Root.AddToClassList("docs-rail");
            Label title = new Label("On this page");
            title.AddToClassList("docs-rail-title");
            Root.Add(title);
            list.AddToClassList("docs-rail-list");
            Root.Add(list);
            pageScroll.verticalScroller.valueChanged += _value => MarkCurrent();
            // The left edge drags to resize the rail.
            resizer = new DocsRailResizer(Root);
        }

        /// <summary>Rebuilds the tree from the drawn page: its section boxes and the part strips inside them.</summary>
        public void Build(VisualElement _page)
        {
            list.Clear();
            sections.Clear();
            sectionRows.Clear();
            parts.Clear();
            partRows.Clear();
            partSection.Clear();
            _page.Query<VisualElement>(null, "docs-section").ForEach(_section =>
            {
                Label heading = _section.Q<Label>(null, "docs-heading");
                if (heading == null)
                {
                    return;
                }
                VisualElement target = _section;
                VisualElement children = new VisualElement();
                children.AddToClassList("docs-rail-children");
                List<Foldout> sectionParts = _section.Query<Foldout>(null, "docs-part-fold").ToList();
                int sectionIndex = sections.Count;
                VisualElement headingRow = heading.parent;
                VisualElement row = Row(heading.text, false, sectionParts.Count > 0, children, () =>
                {
                    pageScroll.ScrollTo(target);
                    // The page shows where the click landed, and the rail marks it at once.
                    Flash(headingRow);
                    MarkClicked(sectionIndex, -1);
                });
                list.Add(row);
                sections.Add(_section);
                sectionRows.Add(row);
                foreach (Foldout part in sectionParts)
                {
                    Foldout open = part;
                    int partIndex = parts.Count;
                    VisualElement partRow = Row(part.text, true, false, null, () =>
                    {
                        open.value = true;
                        pageScroll.schedule.Execute(() => pageScroll.ScrollTo(open)).StartingIn(20);
                        Flash(open.Q<Toggle>());
                        MarkClicked(sectionIndex, partIndex);
                    });
                    children.Add(partRow);
                    parts.Add(part);
                    partRows.Add(partRow);
                    partSection.Add(sectionIndex);
                }
                if (sectionParts.Count > 0)
                {
                    list.Add(children);
                }
            });
            Root.schedule.Execute(MarkCurrent).StartingIn(50);
        }

        // "3.  Show the progress" becomes a muted "3" and the title, so the titles line up.
        private static VisualElement Row(string _text, bool _isPart, bool _hasChildren, VisualElement _children, System.Action _clicked)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-rail-row");
            row.AddToClassList(_isPart ? "docs-rail-part" : "docs-rail-section");
            Label arrow = new Label(_hasChildren ? "▾" : "");
            arrow.AddToClassList("docs-rail-arrow");
            if (_hasChildren)
            {
                arrow.RegisterCallback<ClickEvent>(_click =>
                {
                    bool isOpen = _children.style.display != DisplayStyle.None;
                    _children.style.display = isOpen ? DisplayStyle.None : DisplayStyle.Flex;
                    arrow.text = isOpen ? "▸" : "▾";
                    _click.StopPropagation();
                });
            }
            row.Add(arrow);
            string number;
            string title;
            SplitNumber(_text, out number, out title);
            Label numberLabel = new Label(number);
            numberLabel.AddToClassList("docs-rail-number");
            row.Add(numberLabel);
            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("docs-rail-text");
            row.Add(titleLabel);
            row.tooltip = title;
            row.RegisterCallback<ClickEvent>(_click => _clicked());
            return row;
        }

        private static void SplitNumber(string _text, out string _number, out string _title)
        {
            string text = (_text ?? "").Trim();
            int space = text.IndexOf(' ');
            bool startsWithNumber = space > 0 && char.IsDigit(text[0]);
            _number = startsWithNumber ? text.Substring(0, space).TrimEnd('.') : "";
            _title = startsWithNumber ? text.Substring(space).Trim() : text;
        }

        // The last section whose top has scrolled past the top of the page is the one being read, and inside it the
        // last part whose strip has. A click marks its target at once, since the page may not scroll far enough.
        private bool skipNextScrollMark;

        private void MarkCurrent()
        {
            if (skipNextScrollMark)
            {
                skipNextScrollMark = false;
                return;
            }
            int current = 0;
            for (int index = 0; index < sections.Count; index++)
            {
                float top = TopOf(sections[index]);
                if (top <= 40f)
                {
                    current = index;
                }
            }
            int currentPart = -1;
            for (int index = 0; index < parts.Count; index++)
            {
                bool inCurrentSection = partSection[index] == current;
                if (inCurrentSection && TopOf(parts[index]) <= 40f)
                {
                    currentPart = index;
                }
            }
            Apply(current, currentPart);
        }

        private float TopOf(VisualElement _element)
        {
            return _element.worldBound.yMin - pageScroll.contentViewport.worldBound.yMin;
        }

        private void MarkClicked(int _section, int _part)
        {
            Apply(_section, _part);
            // The scroll the click causes would move the mark again; the click's target wins once.
            skipNextScrollMark = true;
        }

        private void Apply(int _section, int _part)
        {
            for (int index = 0; index < sectionRows.Count; index++)
            {
                sectionRows[index].EnableInClassList("docs-rail-current", index == _section && _part < 0);
                sectionRows[index].EnableInClassList("docs-rail-current-parent", index == _section && _part >= 0);
            }
            for (int index = 0; index < partRows.Count; index++)
            {
                partRows[index].EnableInClassList("docs-rail-current", index == _part);
            }
        }

        // Lights up the heading or part strip the reader jumped to, then lets it fade.
        private static void Flash(VisualElement _target)
        {
            if (_target == null)
            {
                return;
            }
            _target.AddToClassList("docs-rail-target");
            _target.schedule.Execute(() => _target.RemoveFromClassList("docs-rail-target")).StartingIn(FlashMilliseconds);
        }
    }
}
