using System.Collections.Generic;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The scene documentation window: a toolbar (Back, Forward, Home, sidebar toggle, crumbs, "On this page", search),
    /// the contents tree (or search results) and the page column. Dockable. The current topic, the history and the
    /// scroll position are serialized, so they survive a domain reload.
    /// </summary>
    public class SceneDocumentationWindow : EditorWindow
    {
        public const string WindowTitle = "Scene documentation";
        private const float MinWidth = 275f;
        private const float MinHeight = 300f;
        private const float SidebarWidth = 220f;
        private const float NarrowWidth = 500f;
        private const long FlashMilliseconds = 1000;

        [SerializeField] private DocsHistory history = new DocsHistory();
        [SerializeField] private float scrollY;
        [SerializeField] private bool sidebarHidden;

        private ToolbarButton backButton;
        private ToolbarButton forwardButton;
        private Label crumbs;
        private DocsTrail trail;
        private ToolbarMenu onThisPage;
        private ToolbarSearchField searchField;
        private TwoPaneSplitView split;
        private VisualElement sidebar;
        private TreeView tree;
        private ListView results;
        private ScrollView pageScroll;
        private DocsPageRail rail;
        private VisualElement pageColumn;
        private SceneDocsHost host;
        private DocsRenderContext renderContext;
        private string shownPage;
        private System.DateTime contentsWriteTime;
        private List<SceneDocsSearch.Result> shownResults = new List<SceneDocsSearch.Result>();

        /// <summary>The topic shown now (null on a new window).</summary>
        public string CurrentTopic
        {
            get { return history.Current; }
        }

        /// <summary>The page drawn now, for probes.</summary>
        public DocsRenderContext RenderContext
        {
            get { return renderContext; }
        }

        public ScrollView PageScroll
        {
            get { return pageScroll; }
        }

        /// <summary>True while the search field has text (sample following waits then).</summary>
        public bool IsSearching
        {
            get { return searchField != null && !string.IsNullOrEmpty(searchField.value); }
        }

        [MenuItem(SceneSystemDocs.MenuPath, false, 1001)]
        public static void OpenHome()
        {
            OpenAt(SceneSystemDocTopics.Home);
        }

        /// <summary>Opens (or focuses) the window at a topic id.</summary>
        public static SceneDocumentationWindow OpenAt(string _topicId)
        {
            SceneDocumentationWindow window = GetWindow<SceneDocumentationWindow>();
            window.titleContent = new GUIContent(WindowTitle, DocsIcons.Load(DocsIcons.Help));
            window.minSize = new Vector2(MinWidth, MinHeight);
            window.Show();
            window.Navigate(_topicId);
            return window;
        }

        public void CreateGUI()
        {
            titleContent = new GUIContent(WindowTitle, DocsIcons.Load(DocsIcons.Help));
            host = new SceneDocsHost(Navigate);
            VisualElement root = rootVisualElement;
            root.Clear();
            DocsStyles.Apply(root);
            root.AddToClassList("docs-window");
            root.Add(BuildToolbar());
            root.Add(BuildBody());
            root.RegisterCallback<GeometryChangedEvent>(OnWindowResized);
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            SceneDocsTimeline.RowAdded -= OnTimelineRow;
            SceneDocsTimeline.RowAdded += OnTimelineRow;
            SceneDocsTimeline.Cleared -= OnTimelineCleared;
            SceneDocsTimeline.Cleared += OnTimelineCleared;
            float savedScroll = scrollY;
            ShowTopic(history.Current, false);
            RestoreScroll(savedScroll);
        }

        private void OnDestroy()
        {
            SceneDocsTimeline.RowAdded -= OnTimelineRow;
            SceneDocsTimeline.Cleared -= OnTimelineCleared;
        }

        // The "?" in the tab bar. Unity calls this by reflection, as for the Inspector's lock.
        private void ShowButton(Rect _position)
        {
            GUIContent helpContent = new GUIContent(DocsIcons.Load(DocsIcons.Help), "Open the scene documentation Home page");
            if (GUI.Button(_position, helpContent, GUIStyle.none))
            {
                Navigate(SceneSystemDocTopics.Home);
            }
        }

        /// <summary>Shows a topic and adds it to the history.</summary>
        public void Navigate(string _topicId)
        {
            if (string.IsNullOrEmpty(_topicId))
            {
                return;
            }
            history.Visit(_topicId);
            scrollY = 0f;
            ShowTopic(_topicId, true);
        }

        public void GoBack()
        {
            ShowTopic(history.Back(), true);
        }

        public void GoForward()
        {
            ShowTopic(history.Forward(), true);
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new Toolbar();
            toolbar.AddToClassList("docs-toolbar");
            backButton = IconButton(DocsIcons.GlyphBack, "Go back to the page you read before", GoBack);
            forwardButton = IconButton(DocsIcons.GlyphForward, "Go forward again", GoForward);
            ToolbarButton homeButton = new ToolbarButton(() => Navigate(SceneSystemDocTopics.Home));
            homeButton.text = "Home";
            homeButton.tooltip = "Open the Home page";
            ToolbarButton sidebarButton = IconButton(DocsIcons.GlyphSidebar, "Show or hide the contents", ToggleSidebar);
            sidebarButton.AddToClassList("docs-sidebar-toggle");
            toolbar.Add(backButton);
            toolbar.Add(forwardButton);
            toolbar.Add(homeButton);
            toolbar.Add(sidebarButton);

            // Left: the page and where it sits in the contents. Right: the history trail.
            crumbs = new Label();
            crumbs.AddToClassList("docs-crumbs");
            toolbar.Add(crumbs);
            trail = new DocsTrail(Navigate);
            toolbar.Add(trail.Root);

            onThisPage = new ToolbarMenu();
            onThisPage.text = "On this page";
            onThisPage.tooltip = "Jump to a section of this page";
            onThisPage.AddToClassList("docs-on-this-page");
            toolbar.Add(onThisPage);

            searchField = new ToolbarSearchField();
            searchField.tooltip = "Search titles, headings, glossary terms and type names";
            searchField.AddToClassList("docs-search");
            searchField.RegisterValueChangedCallback(_change => ShowSearch(_change.newValue));
            searchField.RegisterCallback<KeyDownEvent>(OnSearchKey, TrickleDown.TrickleDown);
            toolbar.Add(searchField);
            return toolbar;
        }

        private static ToolbarButton IconButton(string _glyph, string _tooltip, System.Action _clicked)
        {
            ToolbarButton button = new ToolbarButton(_clicked);
            button.tooltip = _tooltip;
            button.text = _glyph;
            button.AddToClassList("docs-toolbar-icon");
            return button;
        }

        private VisualElement BuildBody()
        {
            split = new TwoPaneSplitView(0, SidebarWidth, TwoPaneSplitViewOrientation.Horizontal);
            split.viewDataKey = "scene-docs-split";
            split.AddToClassList("docs-split");

            sidebar = new VisualElement();
            sidebar.AddToClassList("docs-sidebar");
            tree = BuildTree();
            sidebar.Add(tree);
            results = BuildResults();
            sidebar.Add(results);
            split.Add(sidebar);

            pageScroll = new ScrollView(ScrollViewMode.Vertical);
            pageScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            pageScroll.AddToClassList("docs-page-scroll");
            pageScroll.verticalScroller.valueChanged += _value => scrollY = _value;
            pageColumn = new VisualElement();
            pageColumn.AddToClassList("docs-page");
            pageScroll.Add(pageColumn);
            // The page and, beside it, the "On this page" rail that stays put while the page scrolls.
            VisualElement pageArea = new VisualElement();
            pageArea.AddToClassList("docs-page-area");
            pageArea.Add(pageScroll);
            rail = new DocsPageRail(pageScroll);
            pageArea.Add(rail.Root);
            split.Add(pageArea);
            return split;
        }

        // The contents file is read again when it changes on disk, so a new page shows up without a reload.
        private void RebuildTreeIfContentsChanged()
        {
            System.DateTime writeTime = System.IO.File.GetLastWriteTimeUtc(SceneDocsPaths.Physical(SceneDocsPaths.Contents));
            if (writeTime == contentsWriteTime)
            {
                return;
            }
            contentsWriteTime = writeTime;
            int index = sidebar.IndexOf(tree);
            sidebar.Remove(tree);
            tree = BuildTree();
            sidebar.Insert(index, tree);
        }

        // Each page's tree id and each id's parent, so the tree can follow the page you open.
        private readonly Dictionary<string, int> treeIdOfTopic = new Dictionary<string, int>();
        private readonly Dictionary<int, int> treeParentOf = new Dictionary<int, int>();

        private TreeView BuildTree()
        {
            List<TreeViewItemData<SceneDocsContentsEntry>> items = new List<TreeViewItemData<SceneDocsContentsEntry>>();
            int nextId = 1;
            treeIdOfTopic.Clear();
            treeParentOf.Clear();
            foreach (SceneDocsContentsEntry entry in SceneDocsContents.Tree())
            {
                items.Add(TreeItem(entry, 0, ref nextId));
            }
            TreeView view = new TreeView();
            view.AddToClassList("docs-tree");
            view.viewDataKey = "scene-docs-tree";
            view.fixedItemHeight = 20;
            view.SetRootItems(items);
            view.makeItem = () =>
            {
                Label label = new Label();
                label.AddToClassList("docs-tree-label");
                return label;
            };
            view.bindItem = (_element, _index) =>
            {
                SceneDocsContentsEntry entry = view.GetItemDataForIndex<SceneDocsContentsEntry>(_index);
                Label label = (Label)_element;
                label.text = entry.Title;
                label.tooltip = string.IsNullOrEmpty(entry.Topic) ? "" : "Opens " + entry.Title;
            };
            view.selectionChanged += _selection => OnTreeSelection(_selection);
            view.Rebuild();
            return view;
        }

        private TreeViewItemData<SceneDocsContentsEntry> TreeItem(SceneDocsContentsEntry _entry, int _parentId, ref int _nextId)
        {
            int id = _nextId++;
            treeParentOf[id] = _parentId;
            string page = PageOfTopic(_entry.Topic);
            if (page.Length > 0 && !treeIdOfTopic.ContainsKey(page))
            {
                treeIdOfTopic[page] = id;
            }
            List<TreeViewItemData<SceneDocsContentsEntry>> children = new List<TreeViewItemData<SceneDocsContentsEntry>>();
            foreach (SceneDocsContentsEntry child in _entry.Children)
            {
                children.Add(TreeItem(child, id, ref _nextId));
            }
            return new TreeViewItemData<SceneDocsContentsEntry>(id, _entry, children);
        }

        private static string PageOfTopic(string _topic)
        {
            string topic = (_topic ?? "").ToLowerInvariant();
            int hash = topic.IndexOf('#');
            return hash < 0 ? topic : topic.Substring(0, hash);
        }

        // Selects the open page in the tree, opening the folders above it, without opening the page again.
        private void FollowInTree(string _topicId)
        {
            int id;
            if (tree == null || !treeIdOfTopic.TryGetValue(PageOfTopic(_topicId), out id))
            {
                return;
            }
            for (int parent = treeParentOf[id]; parent > 0; parent = treeParentOf[parent])
            {
                tree.ExpandItem(parent);
            }
            tree.SetSelectionByIdWithoutNotify(new[] { id });
            tree.ScrollToItemById(id);
        }

        private void OnTreeSelection(IEnumerable<object> _selection)
        {
            foreach (object item in _selection)
            {
                SceneDocsContentsEntry entry = item as SceneDocsContentsEntry;
                bool opensAPage = entry != null && !string.IsNullOrEmpty(entry.Topic) && entry.Topic != history.Current;
                if (opensAPage)
                {
                    Navigate(entry.Topic);
                }
                return;
            }
        }

        private ListView BuildResults()
        {
            ListView list = new ListView();
            list.AddToClassList("docs-results");
            list.AddToClassList("docs-hidden");
            list.fixedItemHeight = 34;
            list.itemsSource = shownResults;
            list.makeItem = () =>
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-result");
                Label text = new Label();
                text.AddToClassList("docs-result-text");
                row.Add(text);
                Label category = new Label();
                category.AddToClassList("docs-result-category");
                row.Add(category);
                return row;
            };
            list.bindItem = (_element, _index) =>
            {
                SceneDocsSearch.Result result = shownResults[_index];
                _element.Q<Label>(className: "docs-result-text").text = result.Text;
                _element.Q<Label>(className: "docs-result-category").text = result.Category;
                _element.tooltip = "Opens " + result.Text;
            };
            list.itemsChosen += _chosen => OpenFirst(_chosen);
            list.selectionChanged += _chosen => OpenFirst(_chosen);
            return list;
        }

        private void OpenFirst(IEnumerable<object> _items)
        {
            foreach (object item in _items)
            {
                SceneDocsSearch.Result result = item as SceneDocsSearch.Result;
                if (result != null)
                {
                    Navigate(result.Topic);
                }
                return;
            }
        }

        private void ShowSearch(string _query)
        {
            bool searching = !string.IsNullOrEmpty(_query);
            tree.EnableInClassList("docs-hidden", searching);
            results.EnableInClassList("docs-hidden", !searching);
            shownResults.Clear();
            if (searching)
            {
                shownResults.AddRange(SceneDocsSearch.Find(_query));
                if (sidebarHidden)
                {
                    sidebarHidden = false;
                    ApplySidebar();
                }
            }
            results.RefreshItems();
        }

        private void OnSearchKey(KeyDownEvent _event)
        {
            bool isEnter = _event.keyCode == KeyCode.Return || _event.keyCode == KeyCode.KeypadEnter;
            if (isEnter && shownResults.Count > 0)
            {
                Navigate(shownResults[0].Topic);
                _event.StopPropagation();
            }
        }

        private void ShowTopic(string _topicId, bool _resetScroll)
        {
            if (pageColumn == null)
            {
                return;
            }
            RebuildTreeIfContentsChanged();
            RefreshHistoryButtons();
            if (string.IsNullOrEmpty(_topicId))
            {
                ShowEmpty();
                return;
            }
            string page;
            string anchor;
            SceneDocsTopics.Split(_topicId, out page, out anchor);
            bool samePage = page == shownPage && renderContext != null;
            if (!samePage)
            {
                DocsPage parsed = SceneDocsPageCache.Get(page);
                if (parsed == null)
                {
                    ShowEmpty();
                    crumbs.text = "No page for " + _topicId;
                    return;
                }
                RenderPage(page, parsed);
            }
            trail.Refresh(history.RecentPages(DocsTrail.MaxPages), crumbs.text);
            FollowInTree(_topicId);
            if (!string.IsNullOrEmpty(anchor))
            {
                ScrollToHeading(anchor);
                return;
            }
            if (_resetScroll)
            {
                pageScroll.scrollOffset = Vector2.zero;
            }
        }

        private void RenderPage(string _page, DocsPage _parsed)
        {
            pageColumn.Clear();
            shownPage = _page;
            renderContext = new DocsRenderContext(_parsed, host);
            pageColumn.AddToClassList("docs-layout-new");
            pageColumn.Add(DocsRenderer.Render(renderContext));
            pageColumn.Add(PagerCards(_page));
            rail.Build(pageColumn);
            rootVisualElement.AddToClassList("docs-has-rail");
            crumbs.text = CrumbsFor(_page, _parsed.Title);
            trail.Refresh(history.RecentPages(DocsTrail.MaxPages), crumbs.text);
            BuildOnThisPage(_parsed);
            ReplayTimelineIntoSteps();
        }

        private void ShowEmpty()
        {
            pageColumn.Clear();
            shownPage = null;
            renderContext = null;
            Label empty = new Label("Pick a page on the left");
            empty.AddToClassList("docs-empty");
            pageColumn.Add(empty);
            crumbs.text = "";
            onThisPage.menu.ClearItems();
        }

        private static string CrumbsFor(string _page, string _title)
        {
            int slash = _page.IndexOf('/');
            if (slash < 0)
            {
                return _title;
            }
            string section = _page.Substring(0, slash);
            string sectionTitle = char.ToUpperInvariant(section[0]) + section.Substring(1);
            return sectionTitle + " / " + _title;
        }

        private void BuildOnThisPage(DocsPage _parsed)
        {
            onThisPage.menu.ClearItems();
            foreach (DocsHeading heading in _parsed.Headings)
            {
                if (heading.Level > 2)
                {
                    continue;
                }
                string id = heading.Id;
                onThisPage.menu.AppendAction(heading.Text, _action => ScrollToHeading(id));
            }
            onThisPage.SetEnabled(_parsed.Headings.Count > 0);
        }

        // The new layout's pager: two equal halves, a small "Previous" or "Next" over the page title, Next on the right.
        private VisualElement PagerCards(string _page)
        {
            VisualElement pager = new VisualElement();
            pager.AddToClassList("docs-pager-cards");
            List<SceneDocsContentsEntry> all = SceneDocsContents.Flatten();
            int index = all.FindIndex(_entry => _entry.Topic == _page);
            bool hasPrevious = index > 0;
            bool hasNext = index >= 0 && index < all.Count - 1;
            pager.Add(hasPrevious ? PagerCard("Previous", all[index - 1], false) : Spacer());
            pager.Add(hasNext ? PagerCard("Next", all[index + 1], true) : Spacer());
            return pager;
        }

        private static VisualElement Spacer()
        {
            VisualElement spacer = new VisualElement();
            spacer.AddToClassList("docs-pager-card-space");
            return spacer;
        }

        private Button PagerCard(string _caption, SceneDocsContentsEntry _entry, bool _isNext)
        {
            Button card = new Button(() => Navigate(_entry.Topic));
            card.tooltip = "Opens " + _entry.Title;
            card.AddToClassList("docs-pager-card");
            card.EnableInClassList("docs-pager-card-next", _isNext);
            Label caption = new Label(_isNext ? _caption + " →" : "← " + _caption);
            caption.AddToClassList("docs-pager-card-caption");
            card.Add(caption);
            Label title = new Label(_entry.Title);
            title.AddToClassList("docs-pager-card-title");
            card.Add(title);
            return card;
        }

        private Button PagerButton(string _text, string _topic)
        {
            Button button = new Button(() => Navigate(_topic));
            button.text = _text;
            button.tooltip = "Opens " + _text.Substring(_text.IndexOf(':') + 2);
            button.AddToClassList("docs-pager-button");
            return button;
        }

        /// <summary>Scrolls a heading into view and flashes its row. Returns false when the page has no such heading.</summary>
        public bool ScrollToHeading(string _anchor)
        {
            VisualElement row;
            if (renderContext == null || !renderContext.HeadingRows.TryGetValue(_anchor, out row))
            {
                return false;
            }
            // A heading inside closed parts opens them first, so the link lands on something you can see.
            for (VisualElement parent = row; parent != null; parent = parent.parent)
            {
                Foldout fold = parent as Foldout;
                if (fold != null)
                {
                    fold.value = true;
                }
                // A member row sits in a member list that starts closed.
                if (parent.ClassListContains("docs-member-table"))
                {
                    SceneDocsMemberTables.SetOpen(parent, true);
                }
            }
            row.AddToClassList("docs-flash");
            pageScroll.schedule.Execute(() => ScrollToIfStillShown(row)).StartingIn(30);
            row.schedule.Execute(() => row.RemoveFromClassList("docs-flash")).StartingIn(FlashMilliseconds);
            return true;
        }

        // The page may have changed in the 30 ms before the scroll runs; a row from the old page is skipped.
        private void ScrollToIfStillShown(VisualElement _row)
        {
            bool isOnThePage = pageScroll.contentContainer.Contains(_row);
            if (!isOnThePage)
            {
                return;
            }
            pageScroll.ScrollTo(_row);
        }

        private void RestoreScroll(float _value)
        {
            if (_value <= 0f)
            {
                return;
            }
            pageScroll.schedule.Execute(() => pageScroll.scrollOffset = new Vector2(0f, _value)).StartingIn(50);
        }

        private void OnTimelineRow(SceneDocsTimelineRow _row)
        {
            if (renderContext == null)
            {
                return;
            }
            foreach (DocsStepList steps in renderContext.StepLists)
            {
                steps.Report(_row.Message);
            }
        }

        private void OnTimelineCleared()
        {
            if (renderContext == null)
            {
                return;
            }
            foreach (DocsStepList steps in renderContext.StepLists)
            {
                steps.Reset();
            }
        }

        // A page opened during Play ticks the steps the rows so far already prove.
        private void ReplayTimelineIntoSteps()
        {
            foreach (SceneDocsTimelineRow row in SceneDocsTimeline.Rows)
            {
                OnTimelineRow(row);
            }
        }

        private void RefreshHistoryButtons()
        {
            backButton.SetEnabled(history.CanGoBack);
            forwardButton.SetEnabled(history.CanGoForward);
        }

        private void ToggleSidebar()
        {
            sidebarHidden = !sidebarHidden;
            ApplySidebar();
        }

        private void ApplySidebar()
        {
            if (sidebarHidden)
            {
                split.CollapseChild(0);
                return;
            }
            split.UnCollapse();
        }

        // Under about 500 px the sidebar collapses behind the toolbar toggle.
        private void OnWindowResized(GeometryChangedEvent _event)
        {
            bool isNarrow = _event.newRect.width < NarrowWidth;
            rootVisualElement.EnableInClassList("docs-narrow", isNarrow);
            rootVisualElement.EnableInClassList("docs-wide", _event.newRect.width >= DocsPageRail.MinWindowWidth);
            bool wasNarrow = _event.oldRect.width < NarrowWidth && _event.oldRect.width > 0f;
            if (isNarrow == wasNarrow)
            {
                return;
            }
            sidebarHidden = isNarrow;
            ApplySidebar();
        }

        private void OnKeyDown(KeyDownEvent _event)
        {
            bool isBack = _event.altKey && _event.keyCode == KeyCode.LeftArrow;
            bool isForward = _event.altKey && _event.keyCode == KeyCode.RightArrow;
            bool isFind = _event.actionKey && _event.keyCode == KeyCode.F;
            bool isEscape = _event.keyCode == KeyCode.Escape;
            if (isBack)
            {
                GoBack();
                _event.StopPropagation();
                return;
            }
            if (isForward)
            {
                GoForward();
                _event.StopPropagation();
                return;
            }
            if (isFind)
            {
                searchField.Focus();
                _event.StopPropagation();
                return;
            }
            if (isEscape && !string.IsNullOrEmpty(searchField.value))
            {
                searchField.value = "";
                _event.StopPropagation();
            }
        }
    }
}
