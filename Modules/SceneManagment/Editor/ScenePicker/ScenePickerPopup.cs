using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene picker's dropdown: Scenes and Scene Groups tabs, one search bar under them that both share, the rows,
    /// and a footer with "Open the group with its main scene" and the keys. The search bar has focus when it opens;
    /// arrow keys move, Enter opens, Shift+Enter selects, Esc closes (decision 10). A click on a row opens it; its
    /// Select button selects it. Either closes the picker. It reads the project again whenever the project changes.
    /// </summary>
    public class ScenePickerPopup : PopupWindowContent
    {
        public const string ScenesTabName = "Scenes";
        public const string GroupsTabName = "Scene Groups";
        public const string OpenWithGroupLabel = "Open the group with its main scene";
        public const string KeysHint = "Enter open · Shift+Enter select";
        private const string StyleSheetGuid = "e090a93f571d828532ed313f826f10ed";
        private const float Width = 400f;
        private const float RowHeight = 20f;

        /// <summary>
        /// The list shows at most this many rows and scrolls past them, so the picker stays a small dropdown under the
        /// toolbar and never runs down the Editor. Fewer matches make it shorter.
        /// </summary>
        public const int MostVisibleRows = 12;
        private const int FewestVisibleRows = 6;

        // When the list scrolls, part of the next row shows, so it reads as a list that goes on.
        private const float NextRowPeek = 6f;

        // Tabs, search bar and footer, measured in the Editor at 100 % scale.
        private const float ChromeHeight = 24f + 24f + 23f + 2f;

        private static string lastQuery = string.Empty;
        private static int lastTabIndex;

        private VisualElement rootElement;
        private TabView tabView;
        private ToolbarSearchField searchField;
        private ListView listView;
        private Label emptyLabel;
        private Label messageLabel;
        private List<ScenePickerRow> allRows = new List<ScenePickerRow>();
        private List<ScenePickerRow> shownRows = new List<ScenePickerRow>();
        private bool hasPicked;
        private bool isForgotten;
        private string[] searchWords = new string[0];

        /// <summary>True while a scene picker popup is open, so the button never opens a second one.</summary>
        public static bool IsOpen { get; private set; }

        /// <summary>The open popup, for probes and tests; null when none is open.</summary>
        public static ScenePickerPopup Current { get; private set; }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(Width, ChromeHeight + MostVisibleRows * RowHeight + NextRowPeek);
        }

#if UNITY_7000_0_OR_NEWER
        // Unity 7 extras for the popup go here (session 5A, stage 9). The 6.3 popup is complete without them.
#endif

        public override void OnOpen()
        {
            IsOpen = true;
            Current = this;
            ScenePickerProject.Changed += OnProjectChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }

        public override void OnClose()
        {
            Forget();
        }

        public override VisualElement CreateGUI()
        {
            VisualElement root = new VisualElement();
            rootElement = root;
            root.AddToClassList("scene-picker");
            root.style.width = Width;
            AddStyleSheet(root);

            tabView = new TabView();
            tabView.AddToClassList("scene-picker__tabs");
            tabView.Add(NewTab(ScenesTabName, "Every scene in the project. Tab switches to Scene Groups."));
            tabView.Add(NewTab(GroupsTabName, "Every scene group in the project. Tab switches to Scenes."));
            tabView.selectedTabIndex = lastTabIndex;
            tabView.activeTabChanged += (_previous, _next) => OnTabChanged();
            root.Add(tabView);

            searchField = new ToolbarSearchField();
            searchField.AddToClassList("scene-picker__search");
            // Each open starts with an empty search, so the Recent section and the open group are what the eye meets first.
            searchField.SetValueWithoutNotify(string.Empty);
            searchField.RegisterValueChangedCallback(_change => OnQueryChanged(_change.newValue));
            root.Add(searchField);

            listView = new ListView();
            listView.AddToClassList("scene-picker__list");
            listView.fixedItemHeight = RowHeight;
            listView.style.flexGrow = 0;
            listView.selectionType = SelectionType.Single;
            listView.makeItem = () => new ScenePickerRowView(OpenRow, SelectRow);
            listView.bindItem = (_element, _index) => BindRow(_element, _index);
            listView.itemsSource = shownRows;
            root.Add(listView);

            emptyLabel = new Label();
            emptyLabel.AddToClassList("scene-picker__empty");
            root.Add(emptyLabel);

            messageLabel = new Label();
            messageLabel.AddToClassList("scene-picker__message");
            root.Add(messageLabel);

            root.Add(Footer());
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            ReadRows();
            HighlightOpenAsset();
            FocusSearchWhenShown();
            return root;
        }

        private static Tab NewTab(string _label, string _tooltip)
        {
            Tab tab = new Tab(_label);
            tab.tooltip = _tooltip;
            return tab;
        }

        // Before any search the open group (or the active scene) is highlighted, so Enter reopens it and arrows start there.
        private void HighlightOpenAsset()
        {
            string openGuid = ScenePickerProject.OpenAssetGuid(ShowsGroups);
            int index = -1;
            if (string.IsNullOrWhiteSpace(searchField.value))
            {
                index = IndexOfGuid(openGuid);
            }
            if (index < 0)
            {
                index = FirstOpenableIndex(0, 1);
            }
            Highlight(index);
        }

        /// <summary>The rows shown now, for probes and tests.</summary>
        public List<ScenePickerRow> ShownRows
        {
            get { return shownRows; }
        }

        /// <summary>Types into the search bar, as a probe or test would.</summary>
        public void SetQuery(string _query)
        {
            if (searchField != null)
            {
                searchField.value = _query ?? string.Empty;
            }
        }

        /// <summary>Shows the Scenes (0) or Scene Groups (1) tab.</summary>
        public void ShowTab(int _index)
        {
            if (tabView != null)
            {
                tabView.selectedTabIndex = Mathf.Clamp(_index, 0, 1);
            }
        }

        /// <summary>Opens the row, as a click on it does.</summary>
        public void OpenRow(ScenePickerRow _row)
        {
            if (_row == null || _row.IsHeading || hasPicked)
            {
                return;
            }
            if (!_row.CanOpen)
            {
                ShowMessage(_row.Title + " has no main scene, so it cannot open. Use the Select button to set it in the Inspector.");
                return;
            }
            string reason = ScenePickerActions.WhyNothingCanOpen();
            if (!string.IsNullOrEmpty(reason))
            {
                ShowMessage(reason);
                return;
            }
            hasPicked = true;
            Close();
            ScenePickerActions.Open(_row);
        }

        /// <summary>Selects the row's asset in the Project window and opens nothing, as its Select button does.</summary>
        public void SelectRow(ScenePickerRow _row)
        {
            if (_row == null || _row.IsHeading || hasPicked)
            {
                return;
            }
            hasPicked = true;
            Close();
            ScenePickerActions.Select(_row);
        }

        public void Close()
        {
            if (editorWindow == null)
            {
                Forget();
                return;
            }
            try
            {
                editorWindow.Close();
            }
            catch (System.Exception)
            {
                // Unity's window was only half made (a show that failed); forget it so the next click opens a new one.
                Forget();
            }
        }

        /// <summary>Clears the open-picker state and its event subscriptions, once, even when Unity never called OnClose.</summary>
        internal void Forget()
        {
            if (isForgotten)
            {
                return;
            }
            isForgotten = true;
            IsOpen = false;
            if (Current == this)
            {
                Current = null;
            }
            ScenePickerProject.Changed -= OnProjectChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        private VisualElement Footer()
        {
            VisualElement footer = new VisualElement();
            footer.AddToClassList("scene-picker__footer");
            Toggle openWithGroup = new Toggle(OpenWithGroupLabel);
            openWithGroup.tooltip = "Opening a scene also opens the scenes Play loads with it. The same choice as Preferences > Abstract Pixel > Scenes.";
            openWithGroup.SetValueWithoutNotify(OpenGroupWithMainScenePreference.IsOn);
            openWithGroup.RegisterValueChangedCallback(_change => OpenGroupWithMainScenePreference.IsOn = _change.newValue);
            footer.Add(openWithGroup);
            Label hint = new Label(KeysHint);
            hint.AddToClassList("scene-picker__footer-hint");
            footer.Add(hint);
            return footer;
        }

        private static void AddStyleSheet(VisualElement _root)
        {
            string path = AssetDatabase.GUIDToAssetPath(StyleSheetGuid);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (sheet != null)
            {
                _root.styleSheets.Add(sheet);
            }
        }

        private bool ShowsGroups
        {
            get { return tabView != null && tabView.selectedTabIndex == 1; }
        }

        private void ReadRows()
        {
            if (ShowsGroups)
            {
                allRows = ScenePickerProject.GroupRows();
            }
            else
            {
                allRows = ScenePickerProject.SceneRows();
            }
            ApplySearch();
        }

        private void ApplySearch()
        {
            string previousKey = HighlightedKey();
            string query = searchField.value;
            searchWords = ScenePickerSearch.Words(query);
            if (string.IsNullOrWhiteSpace(query))
            {
                shownRows = ScenePickerRecent.WithRecentFirst(allRows, ScenePickerRecent.Guids());
            }
            else
            {
                shownRows = ScenePickerSearch.Filter(allRows, query);
            }
            UpdatePlaceholder();
            listView.itemsSource = shownRows;
            listView.Rebuild();
            Highlight(IndexOfKey(previousKey));
            UpdateEmptyText();
            FitListHeight();
        }

        // The list is as tall as its rows, between FewestVisibleRows and MostVisibleRows; the popup follows its content.
        private void FitListHeight()
        {
            int visibleRows = Mathf.Clamp(shownRows.Count, FewestVisibleRows, MostVisibleRows);
            float height = visibleRows * RowHeight;
            if (shownRows.Count > MostVisibleRows)
            {
                height += NextRowPeek;
            }
            listView.style.height = height;
            emptyLabel.style.height = height;
            // An explicit height: Unity sizes the popup from its root, and a root not laid out yet measured as NaN.
            rootElement.style.height = ChromeHeight + height + MessageHeight();
        }

        private float MessageHeight()
        {
            bool hasMessage = !string.IsNullOrEmpty(messageLabel.text);
            return hasMessage ? 32f : 0f;
        }

        private void UpdatePlaceholder()
        {
            TextField input = searchField.Q<TextField>();
            if (input == null)
            {
                return;
            }
            input.textEdition.placeholder = ShowsGroups ? "Search scene groups by name or folder" : "Search scenes by name, folder or group";
            input.textEdition.hidePlaceholderOnFocus = false;
        }

        private void UpdateEmptyText()
        {
            bool isEmpty = shownRows.Count == 0;
            listView.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            emptyLabel.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            emptyLabel.text = EmptyText();
            bool hasMessage = !string.IsNullOrEmpty(messageLabel.text);
            messageLabel.style.display = hasMessage ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private string EmptyText()
        {
            string query = searchField.value;
            if (!string.IsNullOrWhiteSpace(query))
            {
                string what = ShowsGroups ? "scene groups" : "scenes";
                return "No " + what + " match \"" + query.Trim() + "\"";
            }
            if (ShowsGroups)
            {
                return "No scene groups yet. To make one, right-click a scene in the Project window, then Create > Scene Group.";
            }
            return "No scenes in Assets yet.";
        }

        private void ShowMessage(string _text)
        {
            messageLabel.text = _text;
            messageLabel.style.display = DisplayStyle.Flex;
            FitListHeight();
        }

        private void BindRow(VisualElement _element, int _index)
        {
            ScenePickerRowView view = _element as ScenePickerRowView;
            if (view == null || _index < 0 || _index >= shownRows.Count)
            {
                return;
            }
            view.Bind(shownRows[_index], searchWords);
        }

        private void OnTabChanged()
        {
            lastTabIndex = tabView.selectedTabIndex;
            messageLabel.text = string.Empty;
            ReadRows();
            HighlightOpenAsset();
            FocusSearchWhenShown();
        }

        private void OnQueryChanged(string _query)
        {
            lastQuery = _query ?? string.Empty;
            messageLabel.text = string.Empty;
            ApplySearch();
            Highlight(FirstOpenableIndex(0, 1));
        }

        private void OnProjectChanged()
        {
            if (listView == null || hasPicked)
            {
                return;
            }
            ReadRows();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange _change)
        {
            if (_change == PlayModeStateChange.ExitingEditMode)
            {
                Close();
            }
        }

        private void OnKeyDown(KeyDownEvent _key)
        {
            switch (_key.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveHighlight(1);
                    break;
                case KeyCode.UpArrow:
                    MoveHighlight(-1);
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    PickHighlighted(_key.shiftKey);
                    break;
                case KeyCode.Escape:
                    Close();
                    break;
                case KeyCode.Tab:
                    ShowTab(ShowsGroups ? 0 : 1);
                    break;
                default:
                    return;
            }
            _key.StopImmediatePropagation();
        }

        private void PickHighlighted(bool _selectOnly)
        {
            int index = listView.selectedIndex;
            if (index < 0 || index >= shownRows.Count)
            {
                index = FirstOpenableIndex(0, 1);
            }
            if (index < 0)
            {
                return;
            }
            ScenePickerRow row = shownRows[index];
            if (_selectOnly)
            {
                SelectRow(row);
                return;
            }
            OpenRow(row);
        }

        private void MoveHighlight(int _step)
        {
            int start = listView.selectedIndex + _step;
            if (listView.selectedIndex < 0)
            {
                start = _step > 0 ? 0 : shownRows.Count - 1;
            }
            int next = FirstOpenableIndex(start, _step);
            if (next >= 0)
            {
                Highlight(next);
            }
        }

        // The first row from _start, stepping by _step, that is not a heading; -1 when there is none.
        private int FirstOpenableIndex(int _start, int _step)
        {
            for (int index = _start; index >= 0 && index < shownRows.Count; index += _step)
            {
                if (!shownRows[index].IsHeading)
                {
                    return index;
                }
            }
            return -1;
        }

        private void Highlight(int _index)
        {
            if (_index < 0 || _index >= shownRows.Count)
            {
                listView.ClearSelection();
                return;
            }
            listView.SetSelectionWithoutNotify(new[] { _index });
            listView.ScrollToItem(_index);
        }

        private string HighlightedKey()
        {
            int index = listView.selectedIndex;
            if (index < 0 || index >= shownRows.Count)
            {
                return null;
            }
            return shownRows[index].AssetGuid;
        }

        private int IndexOfGuid(string _guid)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return -1;
            }
            for (int index = 0; index < shownRows.Count; index++)
            {
                if (!shownRows[index].IsHeading && shownRows[index].AssetGuid == _guid)
                {
                    return index;
                }
            }
            return -1;
        }

        private int IndexOfKey(string _key)
        {
            if (!string.IsNullOrEmpty(_key))
            {
                for (int index = 0; index < shownRows.Count; index++)
                {
                    if (shownRows[index].AssetGuid == _key)
                    {
                        return index;
                    }
                }
            }
            return FirstOpenableIndex(0, 1);
        }

        // The popup's panel takes focus after CreateGUI returns, so focus moves to the search bar one tick later.
        private void FocusSearchWhenShown()
        {
            searchField.schedule.Execute(() =>
            {
                TextField input = searchField.Q<TextField>();
                if (input != null)
                {
                    input.Focus();
                    return;
                }
                searchField.Focus();
            });
        }
    }
}
