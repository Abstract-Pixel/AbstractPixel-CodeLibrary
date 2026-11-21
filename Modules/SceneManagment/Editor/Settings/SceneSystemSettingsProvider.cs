using System.Collections.Generic;
using System.IO;
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
    /// order: Setup, Start-up, Scene sets, New scenes, Scene Groups Debugger (which also holds the pre-build check). Explanations are in tooltips; names, values,
    /// status and problems are always visible. It refreshes itself when scene groups, scene sets or the settings change,
    /// and after Undo or Redo.
    /// </summary>
    public static class SceneSystemSettingsProvider
    {
        public const string PagePath = "Project/Abstract Pixel/Scene System";
        public const string PageTitle = "Scene System";
        public const string SetupSection = "Setup";
        public const string StartUpSection = SceneSystemStartUpSection.Title;
        public const string SceneSetsSection = "Scene sets";
        public const string NewScenesSection = "New scenes";
        public const string DebuggerSection = SceneGroupsDebugger.Title;

        private static VisualElement pageRoot;
        private static ScrollView pageScroll;
        private static readonly Dictionary<string, VisualElement> sections = new Dictionary<string, VisualElement>();
        private static VisualElement setupCard;
        private static VisualElement startUpCard;
        private static VisualElement setsCard;
        private static VisualElement loadingScenesCard;
        private static VisualElement newScenesCard;
        private static VisualElement debuggerCard;
        private static VisualElement debuggerSummary;
        private static VisualElement relocationCard;
        private static VisualElement samplesCard;
        private static bool refreshQueued;
        private static string pendingSection;
        private static int scrollAttempts;
        private static string moveError;
        private static string settingsError;
        private const string PreloadedTooltip = "The settings asset is in Player Settings > Preloaded Assets, the list of assets a build loads before its first scene, so the scene system can read it in a build. The scene system keeps it there.";
        private const int MaxScrollAttempts = 10;

        // Scene sets section state. The cards are rebuilt only when the sets, their names, paths, default or users change,
        // so editing a set's scenes keeps the focus in its field.
        private static string setsSignature;
        private static string selectedSetGuid;
        private static readonly HashSet<string> openSetGuids = new HashSet<string>();
        private static readonly Dictionary<string, VisualElement> setCards = new Dictionary<string, VisualElement>();
        private static Button removeSetButton;
        private const string DeleteNoteName = "delete-note";
        private const string NoteNameColour = "#F2B27A";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.Project);
            provider.label = PageTitle;
            provider.keywords = new HashSet<string> { "scene", "scene system", "scene set", "scene group", "manager scene", "shared scene", "Addressables", "Build Settings", "Preloaded Assets", "start-up", "start scene group", "launch scene", "loading scene", "fallback scene group", "migration", "debugger", "check" };
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
            scrollAttempts = 0;
            bool wasOpen = pageRoot != null;
            SettingsService.OpenProjectSettings(PagePath);
            // A page built just now scrolls from Activate; one that was already open scrolls from here. Asking twice
            // would let the second request find nothing pending.
            if (wasOpen)
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
            startUpCard = AddSection(page, StartUpSection, SceneSystemStartUpSection.Tooltip);
            setsCard = AddSection(page, SceneSetsSection, "Scenes that come with every scene group using the set. Click a scene set to select it; the + and − under the list add and delete scene sets.");
            loadingScenesCard = AddSection(page, SceneSystemLoadingScenesSection.Title, SceneSystemLoadingScenesSection.Tooltip);
            newScenesCard = AddSection(page, NewScenesSection, "Where scenes go so they load in a build.");
            debuggerCard = SceneSystemUI.Section(page, NumberedTitle(DebuggerSection), SceneGroupsDebugger.Tooltip, out VisualElement debuggerHeader);
            sections[DebuggerSection] = debuggerCard.parent;
            debuggerSummary = new VisualElement();
            debuggerHeader.Add(debuggerSummary);
            debuggerHeader.Add(SceneSystemUI.Button("Check again", "Checks every scene group in the project again.", Refresh));
            samplesCard = AddSection(page, SceneSystemSampleGroupsSection.Title, SceneSystemSampleGroupsSection.Tooltip);
            relocationCard = AddSection(page, SceneSystemRelocationSection.Title, SceneSystemRelocationSection.Tooltip);

            // Never subscribed twice, even if Unity activates the page again without deactivating it first.
            UnsubscribeFromProjectChanges();
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
            VisualElement card = SceneSystemUI.Section(_page, NumberedTitle(_title), _tooltip);
            sections[_title] = card.parent;
            return card;
        }

        // Sections are numbered in page order ("1. Setup"), so a page this long reads like a checklist. The plain title
        // stays the section's key for OpenAt.
        private static string NumberedTitle(string _title)
        {
            int number = sections.Count + 1;
            return number + ". " + _title;
        }

        private static void Deactivate()
        {
            UnsubscribeFromProjectChanges();
            pageRoot = null;
            pageScroll = null;
            sections.Clear();
            setCards.Clear();
            removeSetButton = null;
        }

        private static void UnsubscribeFromProjectChanges()
        {
            SceneSystemSettingsAssets.Changed -= RequestRefresh;
            EditorBuildSettings.sceneListChanged -= RequestRefresh;
            Undo.undoRedoPerformed -= RequestRefresh;
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
            SceneSystemStartUpSection.Fill(startUpCard, settings);
            FillSceneSets(settings);
            SceneSystemLoadingScenesSection.Fill(loadingScenesCard, settings);
            FillNewScenes(settings);
            SceneGroupsDebugger.Fill(debuggerCard, debuggerSummary, settings, RequestRefresh);
            SceneSystemSampleGroupsSection.Fill(samplesCard);
            SceneSystemRelocationSection.Fill(relocationCard, settings);
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
            if (string.IsNullOrEmpty(section) || pageScroll == null)
            {
                return;
            }
            if (!sections.TryGetValue(section, out VisualElement target))
            {
                pendingSection = null;
                return;
            }
            // Not laid out yet: try again shortly, a few times at most.
            bool laidOut = target.layout.height > 0 && !float.IsNaN(target.worldBound.yMin);
            if (!laidOut && scrollAttempts < MaxScrollAttempts)
            {
                scrollAttempts++;
                ScrollToPendingSectionSoon();
                return;
            }
            pendingSection = null;
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
                HelpBox missing = new HelpBox("Settings asset missing: this project has no scene system settings asset, so scene sets and checks are off. Click Get started below to create one.", HelpBoxMessageType.Warning);
                missing.style.marginTop = 4;
                missing.style.marginBottom = 8;
                setupCard.Add(missing);
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

            setupCard.Add(SetUpRow(_settings));
            setupCard.Add(SettingsAssetRow(_settings));
            if (!string.IsNullOrEmpty(settingsError))
            {
                setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Red, settingsError, string.Empty));
            }
            if (!string.IsNullOrEmpty(moveError))
            {
                setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Red, moveError, string.Empty));
            }

            if (SceneSystemSettingsAssets.IsPreloaded(_settings))
            {
                setupCard.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "Builds can read these settings", PreloadedTooltip));
                return;
            }
            setupCard.Add(SceneSystemUI.HelpBoxWithButton("Builds can't read these settings yet: the settings asset is missing from Player Settings > Preloaded Assets, the list a build loads before its first scene. The next build adds it back, or fix it now.", HelpBoxMessageType.Warning, "Fix now", "Adds the settings asset to Player Settings > Preloaded Assets and drops empty entries there.", () =>
            {
                SceneSystemSettingsAssets.EnsurePreloaded(_settings);
                Refresh();
            }));
        }

        // "● Set up   [SceneSystemSettings (Scene System Settings)]"
        private static VisualElement SetUpRow(SceneSystemSettings _settings)
        {
            VisualElement row = SceneSystemUI.StatusRow(SceneSystemUI.Green, "Set up", string.Empty);
            row.Q<Label>().style.width = 106;
            row.Q<Label>().style.flexShrink = 0;
            ObjectField reference = new ObjectField();
            reference.objectType = typeof(SceneSystemSettings);
            reference.allowSceneObjects = false;
            reference.SetValueWithoutNotify(_settings);
            reference.tooltip = "The scene system settings asset this project uses. Pick or drop another one to use that instead. The choice is kept in Preloaded Assets and follows the asset when it moves.";
            reference.style.marginLeft = 0;
            reference.RegisterValueChangedCallback(_change =>
            {
                SceneSystemSettings picked = _change.newValue as SceneSystemSettings;
                if (picked == _settings)
                {
                    return;
                }
                string error = SceneSystemSettingsAssets.Use(picked);
                if (!string.IsNullOrEmpty(error))
                {
                    reference.SetValueWithoutNotify(_settings);
                }
                settingsError = error;
                RequestRefresh();
            });
            reference.style.flexGrow = 1;
            reference.style.flexShrink = 1;
            reference.style.minWidth = 60;
            reference.style.marginRight = 0;
            row.Add(reference);
            return row;
        }

        // "Asset path  [Assets/…/SceneSystemSettings.asset]  [Move…]"
        private static VisualElement SettingsAssetRow(SceneSystemSettings _settings)
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.marginTop = 2;
            row.style.marginBottom = 2;
            Label label = new Label("Asset path");
            label.tooltip = "Where the settings asset is. Move… puts it in another folder inside Assets/.";
            label.style.width = 120;
            label.style.flexShrink = 0;
            row.Add(label);
            Label path = SceneSystemUI.AssetPathField(_settings);
            path.style.flexGrow = 1;
            path.style.flexShrink = 1;
            path.style.minWidth = 60;
            path.style.marginRight = 4;
            row.Add(path);
            row.Add(SceneSystemUI.Button("Move…", "Moves the settings asset to another folder inside Assets/. New scene sets are made next to it.", () => PickFolderAndMove(_settings)));
            return row;
        }

        private static void PickFolderAndMove(SceneSystemSettings _settings)
        {
            string current = SceneSystemSettingsAssets.FolderOf(_settings);
            string picked = EditorUtility.OpenFolderPanel("Move the scene system settings", current, string.Empty);
            if (string.IsNullOrEmpty(picked))
            {
                return;
            }
            string projectRoot = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            string folder = picked.Replace('\\', '/');
            if (folder.StartsWith(projectRoot + "/", System.StringComparison.OrdinalIgnoreCase))
            {
                folder = folder.Substring(projectRoot.Length + 1);
            }
            moveError = SceneSystemSettingsAssets.MoveTo(_settings, folder);
            SceneSystemSettingsAssets.NotifyChanged();
            Refresh();
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
            foldout.text = _number + ".  " + _set.name;
            foldout.value = isOpen;
            foldout.tooltip = "Changes to a scene set reach every scene group using it, except the parts a group overrides. Click to show its scenes.";
            foldout.style.width = 190;
            foldout.style.flexShrink = 1;
            foldout.style.minWidth = 100;
            foldout.contentContainer.style.display = DisplayStyle.None;
            Label foldoutText = foldout.Q<Label>(className: Toggle.textUssClassName);
            if (foldoutText != null)
            {
                foldoutText.style.unityFontStyleAndWeight = FontStyle.Bold;
                if (isDefault)
                {
                    foldoutText.style.color = SceneSystemUI.DefaultAccent;
                }
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

            ObjectField reference = SceneSystemUI.ReadOnlyObjectField(_set, typeof(SceneSet), AssetDatabase.GetAssetPath(_set) + "\nClick to show it in the Project window.");
            reference.style.flexGrow = 1;
            reference.style.flexShrink = 1;
            reference.style.minWidth = 60;
            reference.style.marginLeft = 4;
            reference.style.marginRight = 8;
            header.Add(reference);

            header.Add(DefaultCell(_settings, _set, isDefault));

            Label usage = SceneSystemUI.Hint(users.Count + " groups");
            usage.tooltip = "How many scene groups use this scene set. Open the card to list them.";
            usage.style.width = 62;
            usage.style.flexShrink = 0;
            usage.style.unityTextAlign = TextAnchor.MiddleRight;
            header.Add(usage);

            if (isDefault)
            {
                card.Add(DefaultTag());
            }
            card.Add(header);
            card.Add(DeleteNote());
            card.Add(body);
            return card;
        }

        // Under a selected card's header, says what − will ask when scene groups use the set. Hidden until then.
        private static Label DeleteNote()
        {
            Label note = new Label();
            note.name = DeleteNoteName;
            note.enableRichText = true;
            note.style.whiteSpace = WhiteSpace.Normal;
            note.style.fontSize = 11;
            note.style.marginLeft = 16;
            note.style.marginTop = 3;
            note.style.marginBottom = 3;
            note.style.display = DisplayStyle.None;
            return note;
        }

        // "NOTE: Scene set 'Level' is used by 12 scene groups. Removing it asks …"
        private static string DeleteNoteText(SceneSet _set)
        {
            int users = SceneSetSync.GroupsUsing(_set).Count;
            string groupWord = "scene groups";
            if (users == 1)
            {
                groupWord = "scene group";
            }
            string name = "<color=" + NoteNameColour + ">'" + _set.name + "'</color>";
            return "<b>NOTE:</b> Scene set " + name + " is used by " + users + " " + groupWord + ". Removing it asks whether they move to another scene set or keep their scenes as overrides.";
        }

        // "Chosen default scene set" in the default accent, above the default card's name.
        private static VisualElement DefaultTag()
        {
            Label chosen = SceneSystemUI.Badge("Chosen default scene set", false, "New scene groups start with this scene set.");
            Color accent = SceneSystemUI.DefaultAccent;
            chosen.style.color = accent;
            chosen.style.borderTopColor = accent;
            chosen.style.borderBottomColor = accent;
            chosen.style.borderLeftColor = accent;
            chosen.style.borderRightColor = accent;
            chosen.style.unityFontStyleAndWeight = FontStyle.Bold;
            chosen.style.alignSelf = Align.FlexStart;
            chosen.style.marginLeft = 16;
            chosen.style.marginTop = 4;
            chosen.style.marginBottom = 4;
            return chosen;
        }

        private static VisualElement DefaultCell(SceneSystemSettings _settings, SceneSet _set, bool _isDefault)
        {
            VisualElement cell = new VisualElement();
            cell.style.width = 160;
            cell.style.flexShrink = 1;
            cell.style.minWidth = 100;
            cell.style.overflow = Overflow.Hidden;
            cell.style.alignItems = Align.FlexStart;
            if (_isDefault)
            {
                // The default set's tag sits above its name instead; the empty cell keeps the columns lined up.
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
            footer.style.justifyContent = Justify.FlexEnd;
            VisualElement buttons = SceneSystemUI.AddRemoveFooter(
                "Makes a new scene set next to the settings asset.", () => CreateSceneSet(_settings),
                "Removes the selected scene set, after asking. When scene groups use it, you choose whether they move to another set or keep their scenes as overrides.", () => DeleteSelectedSet(_settings),
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

        // The selected card's background and its note, and the − button: enabled while a scene set is selected. The note
        // sits in the selected card, so it stays in view however many scene sets there are.
        private static void RefreshSetSelection()
        {
            SceneSet selected = SelectedSet();
            bool isUsed = selected != null && SceneSetSync.GroupsUsing(selected).Count > 0;
            foreach (KeyValuePair<string, VisualElement> card in setCards)
            {
                bool isSelected = card.Key == selectedSetGuid;
                SceneSystemUI.ShowSelected(card.Value, isSelected);
                Label note = card.Value.Q<Label>(DeleteNoteName);
                if (note == null)
                {
                    continue;
                }
                bool showNote = isSelected && isUsed;
                note.style.display = showNote ? DisplayStyle.Flex : DisplayStyle.None;
                if (showNote)
                {
                    note.text = DeleteNoteText(selected);
                }
            }
            if (removeSetButton == null)
            {
                return;
            }
            removeSetButton.SetEnabled(selected != null);
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

        // An unused set that is not the default: a plain confirmation. Otherwise the removal window asks what happens to
        // the scene groups using it and which set becomes the default.
        private static void DeleteSelectedSet(SceneSystemSettings _settings)
        {
            SceneSet set = SelectedSet();
            if (set == null)
            {
                return;
            }
            bool isUsed = SceneSetSync.GroupsUsing(set).Count > 0;
            bool isDefault = _settings.DefaultSceneSet == set;
            if (isUsed || isDefault)
            {
                SceneSetRemovalWindow.Open(_settings, set, AfterSetRemoved);
                return;
            }
            string path = AssetDatabase.GetAssetPath(set);
            string message = "Remove scene set '" + set.name + "'? No scene group uses it. It moves to your system's trash; this can't be undone.\n\n" + path;
            bool confirmed = EditorUtility.DisplayDialog("Remove scene set", message, "Remove", "Cancel");
            if (!confirmed)
            {
                return;
            }
            SceneSetRemoval.Remove(_settings, set, SceneSetRemovalMode.ConvertToOverride, null, null);
            AfterSetRemoved();
        }

        private static void AfterSetRemoved()
        {
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
    }
}
