using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The scene documentation window: a toolbar (Back, Forward, Home, sidebar toggle, crumbs, "On this page", search),
    /// a sidebar with the contents tree and the page column. Dockable. The current topic, the history and the scroll
    /// position are serialized, so they survive a domain reload.
    /// </summary>
    public class SceneDocumentationWindow : EditorWindow
    {
        public const string WindowTitle = "Scene documentation";
        private const float MinWidth = 275f;
        private const float MinHeight = 300f;
        private const float SidebarWidth = 220f;
        private const float NarrowWidth = 500f;

        [SerializeField] private DocsHistory history = new DocsHistory();
        [SerializeField] private float scrollY;
        [SerializeField] private bool sidebarHidden;

        private ToolbarButton backButton;
        private ToolbarButton forwardButton;
        private Label crumbs;
        private ToolbarMenu onThisPage;
        private ToolbarSearchField searchField;
        private TwoPaneSplitView split;
        private VisualElement sidebar;
        private ScrollView pageScroll;
        private VisualElement pageColumn;

        /// <summary>The topic shown now (null on a new window).</summary>
        public string CurrentTopic
        {
            get { return history.Current; }
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
            VisualElement root = rootVisualElement;
            root.Clear();
            DocsStyles.Apply(root);
            root.AddToClassList("docs-window");
            root.Add(BuildToolbar());
            root.Add(BuildBody());
            root.RegisterCallback<GeometryChangedEvent>(OnWindowResized);
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            ShowTopic(history.Current, false);
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
            backButton = IconButton(DocsIcons.Back, "Go back to the page you read before", GoBack);
            forwardButton = IconButton(DocsIcons.Forward, "Go forward again", GoForward);
            ToolbarButton homeButton = new ToolbarButton(() => Navigate(SceneSystemDocTopics.Home));
            homeButton.text = "Home";
            homeButton.tooltip = "Open the Home page";
            ToolbarButton sidebarButton = IconButton(DocsIcons.Sidebar, "Show or hide the contents", ToggleSidebar);
            sidebarButton.AddToClassList("docs-sidebar-toggle");
            toolbar.Add(backButton);
            toolbar.Add(forwardButton);
            toolbar.Add(homeButton);
            toolbar.Add(sidebarButton);

            crumbs = new Label();
            crumbs.AddToClassList("docs-crumbs");
            toolbar.Add(crumbs);

            onThisPage = new ToolbarMenu();
            onThisPage.text = "On this page";
            onThisPage.tooltip = "Jump to a section of this page";
            onThisPage.AddToClassList("docs-on-this-page");
            toolbar.Add(onThisPage);

            searchField = new ToolbarSearchField();
            searchField.tooltip = "Search titles, headings, glossary terms and type names";
            searchField.AddToClassList("docs-search");
            toolbar.Add(searchField);
            return toolbar;
        }

        private static ToolbarButton IconButton(string _icon, string _tooltip, System.Action _clicked)
        {
            ToolbarButton button = new ToolbarButton(_clicked);
            button.tooltip = _tooltip;
            button.AddToClassList("docs-toolbar-icon");
            Image icon = new Image();
            icon.image = DocsIcons.Load(_icon);
            icon.AddToClassList("docs-icon");
            button.Add(icon);
            return button;
        }

        private VisualElement BuildBody()
        {
            split = new TwoPaneSplitView(0, SidebarWidth, TwoPaneSplitViewOrientation.Horizontal);
            split.viewDataKey = "scene-docs-split";
            split.AddToClassList("docs-split");

            sidebar = new ScrollView();
            sidebar.AddToClassList("docs-sidebar");
            split.Add(sidebar);

            pageScroll = new ScrollView(ScrollViewMode.Vertical);
            pageScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            pageScroll.AddToClassList("docs-page-scroll");
            pageScroll.verticalScroller.valueChanged += _value => scrollY = _value;
            pageColumn = new VisualElement();
            pageColumn.AddToClassList("docs-page");
            pageScroll.Add(pageColumn);
            split.Add(pageScroll);
            return split;
        }

        private void ShowTopic(string _topicId, bool _resetScroll)
        {
            if (pageColumn == null)
            {
                return;
            }
            pageColumn.Clear();
            RefreshHistoryButtons();
            pageColumn.Add(EmptyState());
            crumbs.text = "";
        }

        // Shown until a page is picked, and for a topic that has no page yet.
        private static VisualElement EmptyState()
        {
            Label empty = new Label("Pick a page on the left");
            empty.AddToClassList("docs-empty");
            return empty;
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
