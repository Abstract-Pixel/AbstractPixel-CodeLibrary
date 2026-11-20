using System.Collections.Generic;
using System.Text;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Scene System page (Project Settings &gt; Abstract Pixel &gt; Scene System), in UI Toolkit. Five sections in this
    /// order: Setup, Scene sets, New scenes, Checks, Scene Groups Debugger. Explanations are in tooltips; names, values,
    /// status and problems are always visible. It refreshes itself when scene groups, scene sets or the settings change,
    /// and after Undo or Redo.
    /// </summary>
    public static class SceneSystemSettingsProvider
    {
        public const string PagePath = "Project/Abstract Pixel/Scene System";
        public const string PageTitle = "Scene System";
        public const string SetupSection = "Setup";
        public const string SceneSetsSection = "Scene sets";
        public const string NewScenesSection = "New scenes";
        public const string ChecksSection = "Checks";
        public const string DebuggerSection = SceneGroupsDebugger.Title;

        private static VisualElement pageRoot;
        private static ScrollView pageScroll;
        private static readonly Dictionary<string, VisualElement> sections = new Dictionary<string, VisualElement>();
        private static VisualElement setupCard;
        private static VisualElement setsCard;
        private static VisualElement newScenesCard;
        private static VisualElement checksCard;
        private static VisualElement debuggerCard;
        private static VisualElement debuggerSummary;
        private static bool refreshQueued;
        private static string pendingSection;

        // Scene sets section state. The cards are rebuilt only when the sets, their names, paths, default or users change,
        // so editing a set's scenes keeps the focus in its field.
        private static string setsSignature;
        private static string selectedSetGuid;
        private static readonly HashSet<string> openSetGuids = new HashSet<string>();
        private static readonly Dictionary<string, VisualElement> setCards = new Dictionary<string, VisualElement>();
        private static Button removeSetButton;
        private static Label removeSetBlocked;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.Project);
            provider.label = PageTitle;
            provider.keywords = new HashSet<string> { "scene", "scene system", "scene set", "scene group", "manager scene", "shared scene", "Addressables", "Build Settings", "Preloaded Assets", "migration", "debugger", "check" };
            provider.activateHandler = (_search, _root) => Activate(_root);
            provider.deactivateHandler = Deactivate;
            return provider;
        }

        /// <summary>Opens Project Settings at the Scene System page.</summary>
        public static void Open()
        {
            OpenAt(null);
        }

        /// <summary>Opens the Scene System page scrolled to the Scene Groups Debugger.</summary>
        public static void OpenAtDebugger()
        {
            OpenAt(DebuggerSection);
        }

        /// <summary>Opens the Scene System page scrolled to the section with this title (the top when null).</summary>
        public static void OpenAt(string _section)
        {
            pendingSection = _section;
            SettingsService.OpenProjectSettings(PagePath);
            if (pageRoot != null)
            {
                ScrollToPendingSectionSoon();
            }
        }

        /// <summary>True while the page is open in Project Settings.</summary>
        public static bool IsOpen
        {
            get { return pageRoot != null; }
        }

        private static void Activate(VisualElement _root)
        {
            ScrollView scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            VisualElement page = scroll.contentContainer;
            page.style.paddingLeft = 10;
            page.style.paddingRight = 12;
            page.style.paddingTop = 2;
            page.style.paddingBottom = 16;
            _root.Add(scroll);
            pageRoot = page;
            pageScroll = scroll;
            sections.Clear();
            setsSignature = null;
            refreshQueued = false;

            page.Add(SceneSystemUI.PageTitle(PageTitle, "How this project opens and switches scenes."));
            setupCard = AddSection(page, SetupSection, "Where this page's settings are saved.");
            setsCard = AddSection(page, SceneSetsSection, "Scenes that come with every scene group using the set. Click a scene set to select it; the + and − under the list add and delete scene sets.");
            newScenesCard = AddSection(page, NewScenesSection, "Where scenes go so they load in a build.");
            checksCard = AddSection(page, ChecksSection, "Catch broken scene groups before they ship.");
            debuggerCard = SceneSystemUI.Section(page, DebuggerSection, SceneGroupsDebugger.Tooltip, out VisualElement debuggerHeader);
            sections[DebuggerSection] = debuggerCard.parent;
            debuggerSummary = new VisualElement();
            debuggerHeader.Add(debuggerSummary);
            debuggerHeader.Add(SceneSystemUI.Button("Check again", "Checks every scene group in the project again.", Refresh));

            SceneSystemSettingsAssets.Changed += RequestRefresh;
            EditorBuildSettings.sceneListChanged += RequestRefresh;
            Undo.undoRedoPerformed += RequestRefresh;
            Refresh();
            if (!string.IsNullOrEmpty(pendingSection))
            {
                ScrollToPendingSectionSoon();
            }
        }

        private static VisualElement AddSection(VisualElement _page, string _title, string _tooltip)
        {
            VisualElement card = SceneSystemUI.Section(_page, _title, _tooltip);
            sections[_title] = card.parent;
            return card;
        }

        private static void Deactivate()
        {
            SceneSystemSettingsAssets.Changed -= RequestRefresh;
            EditorBuildSettings.sceneListChanged -= RequestRefresh;
            Undo.undoRedoPerformed -= RequestRefresh;
            pageRoot = null;
            pageScroll = null;
            sections.Clear();
            setCards.Clear();
            removeSetButton = null;
            removeSetBlocked = null;
        }

        // Many changes arrive together (a save, an import, Undo); they rebuild the page once, after the current event.
        private static void RequestRefresh()
        {
            if (pageRoot == null || refreshQueued)
            {
                return;
            }
            refreshQueued = true;
            pageRoot.schedule.Execute(() =>
            {
                refreshQueued = false;
                Refresh();
            });
        }

        private static void Refresh()
        {
            if (pageRoot == null)
            {
                return;
            }
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            FillSetup(settings);
            FillSceneSets(settings);
            FillNewScenes(settings);
            FillChecks(settings);
            SceneGroupsDebugger.Fill(debuggerCard, debuggerSummary, settings, RequestRefresh);
        }

        private static void ScrollToPendingSectionSoon()
        {
            if (pageRoot == null)
            {
                return;
            }
            // The sections need a layout pass before their positions are known.
            pageRoot.schedule.Execute(ScrollToPendingSection).ExecuteLater(50);
        }

        private static void ScrollToPendingSection()
        {
            string section = pendingSection;
            pendingSection = null;
            if (pageScroll == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(section) || !sections.TryGetValue(section, out VisualElement target))
            {
                pageScroll.scrollOffset = Vector2.zero;
                return;
            }
            float top = target.worldBound.yMin - pageScroll.contentContainer.worldBound.yMin;
            pageScroll.scrollOffset = new Vector2(0, Mathf.Max(0, top - 6));
        }

        private static void FillSetup(SceneSystemSettings _settings)
        {
            setupCard.Clear();
            List<string> allPaths = SceneSystemSettingsAssets.FindAllPaths();
            if (allPaths.Count > 1)
            {
                setupCard.Add(TwoSettingsWarning(allPaths));
            }
            if (_settings == null)
            {
                setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Yellow, "Not set up yet", string.Empty));
                setupCard.Add(new SceneSystemSetupView(_created => Refresh()));
                if (SceneSystemOnboardingWindow.IsTurnedOff)
                {
                    HelpBox turnedOff = SceneSystemUI.HelpBoxWithButton("The setup window is turned off for this project.", HelpBoxMessageType.Info, "Turn it back on", "The setup window opens by itself again while the project has no scene system settings asset.", () =>
                    {
                        SceneSystemOnboardingWindow.IsTurnedOff = false;
                        Refresh();
                    });
                    turnedOff.style.marginTop = 8;
                    setupCard.Add(turnedOff);
                }
                return;
            }

            string path = AssetDatabase.GetAssetPath(_settings);
            setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "Set up", string.Empty));
            ObjectField asset = new ObjectField("Settings asset");
            asset.objectType = typeof(SceneSystemSettings);
            asset.value = _settings;
            asset.tooltip = "The scene system settings asset. Move it to any folder: the scene system finds it by type.";
            asset.SetEnabled(false);
            asset.AddToClassList(ObjectField.alignedFieldUssClassName);
            setupCard.Add(asset);
            setupCard.Add(SceneSystemUI.Hint(path));

            if (SceneSystemSettingsAssets.IsPreloaded(_settings))
            {
                setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "Included in builds (Preloaded Assets)", string.Empty));
                return;
            }
            setupCard.Add(SceneSystemUI.HelpBoxWithButton("The settings asset is not in Player Settings > Preloaded Assets, so a build can't read it. The next build puts it back, or fix it now.", HelpBoxMessageType.Warning, "Add to Preloaded Assets", "Adds the settings asset to Preloaded Assets and drops empty entries.", () =>
            {
                SceneSystemSettingsAssets.EnsurePreloaded(_settings);
                Refresh();
            }));
        }

        private static VisualElement TwoSettingsWarning(List<string> _paths)
        {
            VisualElement box = new VisualElement();
            box.style.marginBottom = 8;
            box.Add(new HelpBox("This project has " + _paths.Count + " scene system settings assets; the scene system uses one. Keep the right one: the others go to your system's trash.", HelpBoxMessageType.Warning));
            foreach (string path in _paths)
            {
                VisualElement row = SceneSystemUI.Row();
                Label label = new Label(path);
                label.style.flexGrow = 1;
                label.style.flexShrink = 1;
                row.Add(label);
                string keptPath = path;
                row.Add(SceneSystemUI.Button("Keep this one", "Moves every other scene system settings asset to the trash.", () => KeepOnly(keptPath, _paths)));
                box.Add(row);
            }
            return box;
        }

        private static void KeepOnly(string _keptPath, List<string> _allPaths)
        {
            List<string> others = new List<string>();
            foreach (string path in _allPaths)
            {
                if (path != _keptPath)
                {
                    others.Add(path);
                }
            }
            bool confirmed = EditorUtility.DisplayDialog("Keep one scene system settings asset", "Keep " + _keptPath + " and move these to the trash?\n\n" + string.Join("\n", others), "Keep it", "Cancel");
            if (!confirmed)
            {
                return;
            }
            foreach (string path in others)
            {
                AssetDatabase.MoveAssetToTrash(path);
            }
            SceneSystemSettings kept = AssetDatabase.LoadAssetAtPath<SceneSystemSettings>(_keptPath);
            SceneSystemSettingsAssets.Remember(kept);
            SceneSystemSettingsAssets.EnsurePreloaded(kept);
            SceneSystemSettingsAssets.NotifyChanged();
        }

        private static void FillSceneSets(SceneSystemSettings _settings)
        {
            string signature = SceneSetsSignature(_settings);
            if (signature == setsSignature)
            {
                RefreshSetSelection();
                return;
            }
            setsSignature = signature;
            setsCard.Clear();
            setCards.Clear();
            removeSetButton = null;
            removeSetBlocked = null;
            if (_settings == null)
            {
                setsCard.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                return;
            }

            List<SceneSet> sets = ListedSets(_settings);
            if (sets.Count == 0)
            {
                setsCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Yellow, "No scene sets yet. Click + to make one.", string.Empty));
            }
            else if (_settings.DefaultSceneSet == null)
            {
                setsCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Yellow, "No default scene set: a new scene group starts without one. Click Make default on a scene set.", string.Empty));
            }

            bool selectionStillListed = false;
            int number = 1;
            foreach (SceneSet set in sets)
            {
                string guid = GuidOf(set);
                if (guid == selectedSetGuid)
                {
                    selectionStillListed = true;
                }
                setsCard.Add(SceneSetCard(_settings, set, number, guid));
                number++;
            }
            if (!selectionStillListed)
            {
                selectedSetGuid = null;
            }
            setsCard.Add(SetsFooter(_settings));
            RefreshSetSelection();
        }

        private static List<SceneSet> ListedSets(SceneSystemSettings _settings)
        {
            List<SceneSet> sets = new List<SceneSet>();
            foreach (SceneSet set in _settings.SceneSets)
            {
                if (set != null)
                {
                    sets.Add(set);
                }
            }
            return sets;
        }

        // Everything the cards show apart from the scenes (which are bound): sets, names, paths, default and users.
        private static string SceneSetsSignature(SceneSystemSettings _settings)
        {
            if (_settings == null)
            {
                return "no settings";
            }
            StringBuilder signature = new StringBuilder();
            foreach (SceneSet set in ListedSets(_settings))
            {
                signature.Append(GuidOf(set)).Append('|').Append(set.name).Append('|').Append(AssetDatabase.GetAssetPath(set));
                if (_settings.DefaultSceneSet == set)
                {
                    signature.Append("|default");
                }
                foreach (SceneGroup group in SceneSetSync.GroupsUsing(set))
                {
                    signature.Append('|').Append(group.name);
                }
                signature.Append('\n');
            }
            return signature.ToString();
        }

        private static string GuidOf(Object _asset)
        {
            string path = AssetDatabase.GetAssetPath(_asset);
            return AssetDatabase.AssetPathToGUID(path);
        }

        // Collapsed: number, name, path, Make default (or "Chosen default scene set") and "N groups".
        // Expanded: manager scenes, shared scenes, Reset shared scenes, and the scene groups using the set.
        private static VisualElement SceneSetCard(SceneSystemSettings _settings, SceneSet _set, int _number, string _guid)
        {
            bool isDefault = _settings.DefaultSceneSet == _set;
            List<SceneGroup> users = SceneSetSync.GroupsUsing(_set);
            VisualElement card = SceneSystemUI.SetCard(isDefault);
            card.RegisterCallback<PointerDownEvent>(_event => SelectSet(_guid), TrickleDown.TrickleDown);
            setCards[_guid] = card;

            bool isOpen = openSetGuids.Contains(_guid);
            VisualElement body = new VisualElement();
            body.style.marginLeft = 16;
            body.style.marginTop = 2;
            body.style.marginBottom = 4;
            body.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            if (isOpen)
            {
                FillSetBody(body, _set, users);
            }

            VisualElement header = SceneSystemUI.Row();
            Foldout foldout = new Foldout();
            foldout.text = _number + "   " + _set.name;
            foldout.value = isOpen;
            foldout.tooltip = "Changes to a scene set reach every scene group using it, except the parts a group overrides. Click to show its scenes.";
            foldout.style.width = 190;
            foldout.style.flexShrink = 0;
            foldout.contentContainer.style.display = DisplayStyle.None;
            Label foldoutText = foldout.Q<Label>(className: Toggle.textUssClassName);
            if (foldoutText != null)
            {
                foldoutText.style.unityFontStyleAndWeight = FontStyle.Bold;
                foldoutText.style.overflow = Overflow.Hidden;
                foldoutText.style.textOverflow = TextOverflow.Ellipsis;
                foldoutText.style.flexShrink = 1;
            }
            SceneSystemUI.AddHoverTint(foldout.Q<Toggle>());
            foldout.RegisterValueChangedCallback(_change =>
            {
                if (_change.target != foldout)
                {
                    return;
                }
                OnSetCardToggled(_guid, _change.newValue, body, _set, users);
            });
            header.Add(foldout);

            Label path = SceneSystemUI.AssetPathField(_set);
            path.style.flexGrow = 1;
            path.style.flexShrink = 1;
            path.style.minWidth = 60;
            path.style.marginLeft = 4;
            path.style.marginRight = 8;
            header.Add(path);

            header.Add(DefaultCell(_settings, _set, isDefault));

            Label usage = SceneSystemUI.Hint(users.Count + " groups");
            usage.tooltip = "How many scene groups use this scene set. Open the card to list them.";
            usage.style.width = 62;
            usage.style.flexShrink = 0;
            usage.style.unityTextAlign = TextAnchor.MiddleRight;
            header.Add(usage);

            card.Add(header);
            card.Add(body);
            return card;
        }

        private static VisualElement DefaultCell(SceneSystemSettings _settings, SceneSet _set, bool _isDefault)
        {
            VisualElement cell = new VisualElement();
            cell.style.width = 160;
            cell.style.flexShrink = 0;
            cell.style.alignItems = Align.FlexStart;
            if (_isDefault)
            {
                Label chosen = SceneSystemUI.Badge("Chosen default scene set", false, "New scene groups start with this scene set.");
                chosen.style.marginLeft = 3;
                cell.Add(chosen);
                return cell;
            }
            Button makeDefault = SceneSystemUI.Button("Make default", "New scene groups will start with this scene set.", () =>
            {
                _settings.DefaultSceneSet = _set;
                SceneSystemSettingsAssets.Save(_settings);
            });
            cell.Add(makeDefault);
            return cell;
        }

        private static void OnSetCardToggled(string _guid, bool _isOpen, VisualElement _body, SceneSet _set, List<SceneGroup> _users)
        {
            if (_isOpen)
            {
                openSetGuids.Add(_guid);
                if (_body.childCount == 0)
                {
                    FillSetBody(_body, _set, _users);
                }
                _body.style.display = DisplayStyle.Flex;
                return;
            }
            openSetGuids.Remove(_guid);
            _body.style.display = DisplayStyle.None;
        }

        private static void FillSetBody(VisualElement _body, SceneSet _set, List<SceneGroup> _users)
        {
            _body.Add(SceneSystemUI.SceneSetFields(new SerializedObject(_set)));

            Foldout usersFoldout = new Foldout();
            usersFoldout.text = "Scene groups using this set (" + _users.Count + ")";
            usersFoldout.tooltip = "Every scene group whose scene set is this one. Click a name to select it.";
            usersFoldout.value = false;
            usersFoldout.style.marginTop = 6;
            if (_users.Count == 0)
            {
                usersFoldout.Add(SceneSystemUI.Hint("No scene group uses this set."));
            }
            foreach (SceneGroup group in _users)
            {
                Label link = SceneSystemUI.AssetLink(group, group.name);
                link.style.marginTop = 1;
                link.style.marginBottom = 1;
                link.style.alignSelf = Align.FlexStart;
                usersFoldout.Add(link);
            }
            _body.Add(usersFoldout);
        }

        private static VisualElement SetsFooter(SceneSystemSettings _settings)
        {
            VisualElement footer = SceneSystemUI.Row();
            footer.style.marginTop = 2;
            removeSetBlocked = SceneSystemUI.Hint(string.Empty);
            removeSetBlocked.style.flexGrow = 1;
            removeSetBlocked.style.flexShrink = 1;
            removeSetBlocked.style.marginRight = 8;
            footer.Add(removeSetBlocked);
            VisualElement buttons = SceneSystemUI.AddRemoveFooter(
                "Makes a new scene set next to the settings asset.", () => CreateSceneSet(_settings),
                "Deletes the selected scene set, after asking. A scene set that scene groups use can't be deleted.", () => DeleteSelectedSet(_settings),
                out Button remove);
            removeSetButton = remove;
            footer.Add(buttons);
            return footer;
        }

        private static void SelectSet(string _guid)
        {
            if (selectedSetGuid == _guid)
            {
                return;
            }
            selectedSetGuid = _guid;
            RefreshSetSelection();
        }

        // The selected card's background, and the − button: enabled only for a selected set that no scene group uses.
        private static void RefreshSetSelection()
        {
            foreach (KeyValuePair<string, VisualElement> card in setCards)
            {
                SceneSystemUI.ShowSelected(card.Value, card.Key == selectedSetGuid);
            }
            if (removeSetButton == null || removeSetBlocked == null)
            {
                return;
            }
            SceneSet selected = SelectedSet();
            if (selected == null)
            {
                removeSetButton.SetEnabled(false);
                removeSetBlocked.text = string.Empty;
                return;
            }
            bool canDelete = SceneSetDeletion.CanDelete(selected, out string reason);
            removeSetButton.SetEnabled(canDelete);
            removeSetBlocked.text = reason;
        }

        private static SceneSet SelectedSet()
        {
            if (string.IsNullOrEmpty(selectedSetGuid))
            {
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(selectedSetGuid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneSet>(path);
        }

        private static void DeleteSelectedSet(SceneSystemSettings _settings)
        {
            SceneSet set = SelectedSet();
            if (set == null)
            {
                return;
            }
            if (!SceneSetDeletion.CanDelete(set, out string _))
            {
                RefreshSetSelection();
                return;
            }
            string path = AssetDatabase.GetAssetPath(set);
            string message = "Delete scene set '" + set.name + "'? It moves to your system's trash.\n\n" + path;
            if (_settings.DefaultSceneSet == set)
            {
                message += "\n\nIt is the default scene set, so new scene groups will start without one until you pick another default.";
            }
            bool confirmed = EditorUtility.DisplayDialog("Delete scene set", message, "Delete", "Cancel");
            if (!confirmed)
            {
                return;
            }
            SceneSetDeletion.Delete(_settings, set);
            openSetGuids.Remove(selectedSetGuid);
            selectedSetGuid = null;
            SceneSystemSettingsAssets.NotifyChanged();
        }

        private static void CreateSceneSet(SceneSystemSettings _settings)
        {
            string folder = SceneSystemSettingsAssets.FolderOf(_settings);
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/New Scene Set.asset");
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            AssetDatabase.CreateAsset(set, path);
            _settings.SceneSets.Add(set);
            string guid = AssetDatabase.AssetPathToGUID(path);
            selectedSetGuid = guid;
            openSetGuids.Add(guid);
            SceneSystemSettingsAssets.Save(_settings);
            Debug.Log("[Scene system] Created scene set '" + set.name + "' at " + path + ".", set);
        }

        private static void FillNewScenes(SceneSystemSettings _settings)
        {
            newScenesCard.Clear();
            if (_settings == null)
            {
                newScenesCard.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                return;
            }
            bool addressablesInstalled = AddressableScenes.IsInstalled;
            RadioButtonGroup place = new RadioButtonGroup("New scenes go to", new List<string> { "Build Settings", "Addressables" });
            place.value = _settings.NewScenesAreAddressable ? 1 : 0;
            place.tooltip = "Where a scene that is Not in build goes when its fix button is clicked.";
            place.RegisterValueChangedCallback(_change =>
            {
                _settings.NewScenesAreAddressable = _change.newValue == 1;
                SceneSystemSettingsAssets.Save(_settings);
            });
            place.schedule.Execute(() =>
            {
                List<RadioButton> buttons = place.Query<RadioButton>().ToList();
                if (buttons.Count < 2)
                {
                    return;
                }
                buttons[1].tooltip = addressablesInstalled ? "Scenes load through Addressables." : SceneMoves.NeedsAddressablesTooltip;
                buttons[1].SetEnabled(addressablesInstalled);
            });
            newScenesCard.Add(place);

            string fixName = _settings.NewScenesAreAddressable && addressablesInstalled ? "Make Addressable" : "Add to build";
            place.tooltip = "A scene that is Not in build gets a '" + fixName + "' button. Right-click a scene field to move one scene.";

            Button moveAll = SceneSystemUI.Button("Move all scenes of all groups to Addressables…", addressablesInstalled ? "Lists the scenes first and asks before it moves them. Each scene leaves Build Settings." : SceneMoves.NeedsAddressablesTooltip, SceneMoves.MoveAllGroupsToAddressablesAfterConfirmation);
            moveAll.SetEnabled(addressablesInstalled);
            moveAll.style.marginTop = 6;
            moveAll.style.alignSelf = Align.FlexStart;
            newScenesCard.Add(moveAll);
        }

        private static void FillChecks(SceneSystemSettings _settings)
        {
            checksCard.Clear();
            if (_settings != null)
            {
                Toggle beforeBuild = new Toggle("Check scene groups before a build");
                beforeBuild.value = _settings.CheckSceneGroupsBeforeBuild;
                beforeBuild.tooltip = "A build stops when any scene group has a problem, a scene listed twice included, with a message naming each group and scene. Saved in the settings asset, so it is shared through version control.";
                beforeBuild.AddToClassList(Toggle.alignedFieldUssClassName);
                beforeBuild.RegisterValueChangedCallback(_change =>
                {
                    _settings.CheckSceneGroupsBeforeBuild = _change.newValue;
                    SceneSystemSettingsAssets.Save(_settings);
                });
                checksCard.Add(beforeBuild);
            }

            Dictionary<string, List<SceneGroupProblem>> problemsByGroup = SceneGroupAssets.ProblemsByGroupPath();
            int problemCount = 0;
            foreach (List<SceneGroupProblem> problems in problemsByGroup.Values)
            {
                problemCount += problems.Count;
            }
            int groupCount = SceneGroupAssets.FindAll().Count;
            if (problemCount == 0)
            {
                checksCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "No problems in " + groupCount + " scene group(s).", string.Empty));
            }
            else
            {
                checksCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Red, problemCount + " problem(s) in " + problemsByGroup.Count + " of " + groupCount + " scene group(s).", string.Empty));
            }
        }
    }
}
