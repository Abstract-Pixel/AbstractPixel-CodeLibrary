using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Project Settings &gt; Abstract Pixel &gt; Scenes, in UI Toolkit. Five sections in this order: Setup, Scene sets,
    /// New scenes, Checks, Scene groups. It refreshes itself when scene groups, scene sets or the settings change.
    /// </summary>
    public static class SceneSystemSettingsProvider
    {
        public const string PagePath = "Project/Abstract Pixel/Scenes";
        private const string CheckAllTooltip = "Opens a window listing every scene group in the project with its problems and their fixes.";

        private static VisualElement pageRoot;
        private static VisualElement setupCard;
        private static VisualElement setsCard;
        private static VisualElement setsList;
        private static VisualElement setEditor;
        private static SceneSet editedSet;
        private static VisualElement newScenesCard;
        private static VisualElement checksCard;
        private static VisualElement groupsCard;
        private static SceneSet selectedSet;
        private static bool groupsOpen;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.Project);
            provider.label = "Scenes";
            provider.keywords = new HashSet<string> { "scene", "scene set", "scene group", "manager scene", "shared scene", "Addressables", "Build Settings", "Preloaded Assets", "migration" };
            provider.activateHandler = (_search, _root) => Activate(_root);
            provider.deactivateHandler = Deactivate;
            return provider;
        }

        public static void Open()
        {
            SettingsService.OpenProjectSettings(PagePath);
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

            page.Add(SceneSystemUI.Title("Scenes", "How this project opens and switches scenes."));
            setupCard = SceneSystemUI.Section(page, "Setup", "Where this page's settings are saved.");
            setsCard = SceneSystemUI.Section(page, "Scene sets", "Scenes that come with every scene group using the set.");
            setsList = new VisualElement();
            setsCard.Add(setsList);
            setEditor = new VisualElement();
            setsCard.Add(setEditor);
            editedSet = null;
            newScenesCard = SceneSystemUI.Section(page, "New scenes", "Where scenes go so they load in a build.");
            checksCard = SceneSystemUI.Section(page, "Checks", "Catch broken scene groups before they ship.");
            groupsCard = SceneSystemUI.Section(page, "Scene groups", "Every scene group in the project.");

            SceneSystemSettingsAssets.Changed += Refresh;
            EditorBuildSettings.sceneListChanged += Refresh;
            Refresh();
        }

        private static void Deactivate()
        {
            SceneSystemSettingsAssets.Changed -= Refresh;
            EditorBuildSettings.sceneListChanged -= Refresh;
            pageRoot = null;
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
            FillSceneGroups(settings);
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
            setsList.Clear();
            if (_settings == null)
            {
                setsList.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                selectedSet = null;
                FillSetEditor();
                return;
            }
            if (_settings.SceneSets.Count == 0)
            {
                setsList.Add(SceneSystemUI.StatusRow(SceneSystemUI.Yellow, "No scene sets yet.", string.Empty));
            }
            else if (_settings.DefaultSceneSet == null)
            {
                setsList.Add(SceneSystemUI.StatusRow(SceneSystemUI.Yellow, "No default scene set: a new scene group starts without one. Pick a default below.", string.Empty));
            }

            if (selectedSet != null && !_settings.SceneSets.Contains(selectedSet))
            {
                selectedSet = null;
            }
            foreach (SceneSet set in _settings.SceneSets)
            {
                if (set == null)
                {
                    continue;
                }
                setsList.Add(SceneSetRow(_settings, set));
            }

            Button newSet = SceneSystemUI.Button("New scene set", "Creates a scene set next to the settings asset and opens it below for editing.", () => CreateSceneSet(_settings));
            newSet.style.marginTop = 6;
            newSet.style.alignSelf = Align.FlexStart;
            setsList.Add(newSet);
            FillSetEditor();
        }

        private static VisualElement SceneSetRow(SceneSystemSettings _settings, SceneSet _set)
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.marginTop = 2;
            row.style.marginBottom = 2;

            Label name = new Label(_set.name);
            name.style.width = 160;
            name.style.unityFontStyleAndWeight = _set == selectedSet ? FontStyle.Bold : FontStyle.Normal;
            name.tooltip = AssetDatabase.GetAssetPath(_set);
            row.Add(name);

            VisualElement defaultCell = new VisualElement();
            defaultCell.style.width = 110;
            if (_settings.DefaultSceneSet == _set)
            {
                defaultCell.Add(SceneSystemUI.Badge("Default", true, "New scene groups start with this scene set."));
                defaultCell.style.alignItems = Align.FlexStart;
            }
            else
            {
                defaultCell.Add(SceneSystemUI.Button("Make default", "New scene groups will start with this scene set.", () =>
                {
                    _settings.DefaultSceneSet = _set;
                    SceneSystemSettingsAssets.Save(_settings);
                }));
            }
            row.Add(defaultCell);

            int users = SceneSetSync.GroupsUsing(_set).Count;
            Label usage = SceneSystemUI.Hint(users + " groups");
            usage.style.flexGrow = 1;
            usage.style.flexShrink = 1;
            row.Add(usage);

            Button select = SceneSystemUI.Button("Select", "Shows this scene set's scenes below for editing, and selects the asset in the Project window.", () =>
            {
                selectedSet = _set;
                EditorGUIUtility.PingObject(_set);
                Refresh();
            });
            row.Add(select);
            return row;
        }

        // Rebuilt only when another set is selected, so editing a set (which refreshes the page) keeps its focus.
        private static void FillSetEditor()
        {
            if (selectedSet == editedSet)
            {
                return;
            }
            editedSet = selectedSet;
            setEditor.Clear();
            if (selectedSet == null)
            {
                return;
            }
            VisualElement card = SceneSystemUI.Card();
            card.style.marginTop = 8;
            Label title = new Label("Scene set '" + selectedSet.name + "'");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            card.Add(title);
            card.Add(SceneSystemUI.Hint("Changes reach every group using this set, except its overrides."));
            card.Add(SceneSystemUI.SceneSetFields(new SerializedObject(selectedSet)));
            setEditor.Add(card);
        }

        private static void CreateSceneSet(SceneSystemSettings _settings)
        {
            string folder = SceneSystemSettingsAssets.FolderOf(_settings);
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/New Scene Set.asset");
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            AssetDatabase.CreateAsset(set, path);
            _settings.SceneSets.Add(set);
            selectedSet = set;
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
            Button checkAll = SceneSystemUI.Button("Check All Scene Groups", CheckAllTooltip, CheckAllSceneGroupsWindow.Open);
            checkAll.style.alignSelf = Align.FlexStart;
            checksCard.Add(checkAll);
        }

        private static void FillSceneGroups(SceneSystemSettings _settings)
        {
            groupsCard.Clear();
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);
            if (_settings != null && plan.HasWork)
            {
                groupsCard.Add(MigrationBox(_settings, plan));
            }

            Foldout list = new Foldout();
            list.text = "Scene groups (" + groups.Count + ")";
            list.value = groupsOpen;
            list.RegisterValueChangedCallback(_change =>
            {
                if (_change.target == list)
                {
                    groupsOpen = _change.newValue;
                }
            });
            list.Add(GroupHeaderRow());
            foreach (SceneGroup group in groups)
            {
                list.Add(GroupRow(group));
            }
            groupsCard.Add(list);

            Button checkAll = SceneSystemUI.Button("Check All Scene Groups", CheckAllTooltip, CheckAllSceneGroupsWindow.Open);
            checkAll.style.alignSelf = Align.FlexStart;
            checkAll.style.marginTop = 6;
            groupsCard.Add(checkAll);
        }

        private static VisualElement MigrationBox(SceneSystemSettings _settings, SceneGroupMigrationPlan _plan)
        {
            VisualElement box = new VisualElement();
            box.style.marginBottom = 8;
            string message = _plan.GroupsToMigrateCount + " scene groups have no scene set yet. Organize them into: " + string.Join(", ", _plan.SetNames) + ".";
            box.Add(new HelpBox(message, HelpBoxMessageType.Info));
            VisualElement buttons = SceneSystemUI.Row();
            buttons.Add(SceneSystemUI.Button("Preview migration", "Writes the migration report next to the settings asset without changing anything.", () => RunMigration(_settings, true)));
            buttons.Add(SceneSystemUI.Button("Migrate", "Makes the scene sets and points each scene group at its set. Every group keeps every scene. A report lists each group before and after.", () => RunMigration(_settings, false)));
            box.Add(buttons);
            return box;
        }

        private static void RunMigration(SceneSystemSettings _settings, bool _isPreview)
        {
            string reportPath = SceneGroupMigration.Run(_settings, _isPreview, null, null);
            SceneSystemSettingsAssets.RefreshLists(_settings);
            SceneSystemSettingsAssets.NotifyChanged();
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(reportPath));
        }

        private static VisualElement GroupHeaderRow()
        {
            VisualElement row = GroupRowLayout("Scene group", "Main scene", "Scene set", "Problems");
            foreach (Label label in row.Query<Label>().ToList())
            {
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            return row;
        }

        private static VisualElement GroupRow(SceneGroup _group)
        {
            string mainScene = SceneGroupMigration.HasMainScene(_group) ? _group.MainScene.SceneName : "(none)";
            string set = _group.SceneSet != null ? _group.SceneSet.name : "—";
            int problemCount = SceneGroupCheck.Find(_group).Count;
            VisualElement row = GroupRowLayout(_group.name, mainScene, set, string.Empty);
            row.tooltip = AssetDatabase.GetAssetPath(_group);

            VisualElement problems = row.Q<VisualElement>("problems");
            Color colour = problemCount == 0 ? SceneSystemUI.Green : SceneSystemUI.Red;
            string text = problemCount == 0 ? "None" : problemCount.ToString();
            if (problemCount == 0 && SceneGroupCheck.IsBootGroup(_group))
            {
                colour = SceneSystemUI.Yellow;
                text = "Boot group, unused";
            }
            problems.Add(SceneSystemUI.StatusRow(colour, text, string.Empty));

            row.Add(SceneSystemUI.Button("Select", "Selects this scene group and shows it in the Inspector.", () =>
            {
                Selection.activeObject = _group;
                EditorGUIUtility.PingObject(_group);
            }));
            return row;
        }

        private static VisualElement GroupRowLayout(string _group, string _mainScene, string _set, string _problems)
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.marginTop = 1;
            row.style.marginBottom = 1;
            row.Add(Cell(_group, 190));
            row.Add(Cell(_mainScene, 150));
            row.Add(Cell(_set, 110));
            VisualElement problems = new VisualElement();
            problems.name = "problems";
            problems.style.width = 150;
            if (!string.IsNullOrEmpty(_problems))
            {
                problems.Add(new Label(_problems));
            }
            row.Add(problems);
            row.Add(SceneSystemUI.Spacer());
            return row;
        }

        private static Label Cell(string _text, float _width)
        {
            Label cell = new Label(_text);
            cell.style.width = _width;
            cell.style.overflow = Overflow.Hidden;
            cell.style.textOverflow = TextOverflow.Ellipsis;
            return cell;
        }
    }
}
