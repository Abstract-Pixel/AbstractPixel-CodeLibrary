using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The Project Settings &gt; Abstract Pixel &gt; Game Management page: five numbered cards.
    /// It rebuilds after every change, Undo and project change.</summary>
    internal static class GameManagementSettingsProvider
    {
        public const string PagePath = "Project/Abstract Pixel/Game Management";
        private const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/GameManagement/Editor/Settings/GameManagementSettingsPage.uss";

        private static ScrollView page;
        private static bool rebuildQueued;
        private static readonly StateListView stateList = new StateListView(QueueRebuild);

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.Project);
            provider.label = "Game Management";
            provider.keywords = new HashSet<string> { "game management", "state", "sub-state", "catalog", "pause", "input", "time scale", "debugger", "Preloaded Assets" };
            provider.activateHandler = (_search, _root) => Activate(_root);
            provider.deactivateHandler = Deactivate;
            return provider;
        }

        private static void Activate(VisualElement _root)
        {
            page = new ScrollView(ScrollViewMode.Vertical);
            page.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            page.AddToClassList("gm-page");
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null)
            {
                page.styleSheets.Add(sheet);
            }
            else
            {
                Debug.LogError("[Game management] The settings page style sheet is missing at " + StyleSheetPath + ". Reimport the library.");
            }
            _root.Add(page);
            Undo.undoRedoPerformed += QueueRebuild;
            EditorApplication.projectChanged += QueueRebuild;
            Rebuild();
        }

        private static void Deactivate()
        {
            Undo.undoRedoPerformed -= QueueRebuild;
            EditorApplication.projectChanged -= QueueRebuild;
            page = null;
            rebuildQueued = false;
        }

        // Delayed, so an element is never destroyed while it is still handling its own click.
        private static void QueueRebuild()
        {
            if (rebuildQueued || page == null)
            {
                return;
            }
            rebuildQueued = true;
            EditorApplication.delayCall += Rebuild;
        }

        private static void Rebuild()
        {
            rebuildQueued = false;
            if (page == null)
            {
                return;
            }
            Vector2 scroll = page.scrollOffset;
            page.Clear();

            VisualElement title = EditorUI.Row();
            title.AddToClassList("gm-page-title");
            title.Add(new Label("Game Management"));
            title.Add(HelpButton());
            page.Add(title);

            GameManagementSettings settings = GameManagementSettingsAssets.Find();
            BuildSetup(settings);
            BuildStates(settings);
            BuildBetweenScenes(settings);
            BuildInput(settings);
            BuildDebugger(settings);
            page.schedule.Execute(() => page.scrollOffset = scroll);
        }

        private static Button HelpButton()
        {
            Button help = EditorUI.Button("?", "Opens the Game Management read-me", OpenReadMe);
            help.AddToClassList("gm-help-button");
            return help;
        }

        internal static void OpenReadMe()
        {
            Object readMe = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.abstractpixel.library/Modules/GameManagement/README.md");
            if (readMe == null)
            {
                Debug.LogWarning("[Game management] The read-me is missing from Modules/GameManagement. Reimport the library.");
                return;
            }
            AssetDatabase.OpenAsset(readMe);
        }

        private static VisualElement Card(string _title, out VisualElement _header)
        {
            _header = EditorUI.Row();
            _header.AddToClassList("gm-section-header");
            Label label = new Label(_title);
            label.AddToClassList("gm-section-title");
            _header.Add(label);
            _header.Add(HelpButton());
            page.Add(_header);
            VisualElement card = new VisualElement();
            card.AddToClassList("gm-card");
            page.Add(card);
            return card;
        }

        // Without settings, a card shows why it is off and its fields are greyed.
        private static VisualElement Body(VisualElement _card, GameManagementSettings _settings)
        {
            VisualElement body = new VisualElement();
            if (_settings == null)
            {
                _card.Add(Hint("Create settings first"));
                body.SetEnabled(false);
            }
            _card.Add(body);
            return body;
        }

        private static Label Hint(string _text)
        {
            Label hint = new Label(_text);
            hint.AddToClassList("gm-hint");
            return hint;
        }

        private static void BuildSetup(GameManagementSettings _settings)
        {
            VisualElement card = Card("1. Setup", out VisualElement _);
            VisualElement dotRow = new VisualElement();
            ObjectField field = new ObjectField("Set up");
            field.objectType = typeof(GameManagementSettings);
            field.allowSceneObjects = false;
            field.value = _settings;
            field.tooltip = "The project's game management settings. Drop another asset here to use it.";
            field.AddToClassList("gm-dot-field");
            field.RegisterValueChangedCallback(_change =>
            {
                if (_change.newValue is GameManagementSettings picked)
                {
                    GameManagementSettingsAssets.Use(picked);
                }
                QueueRebuild();
            });
            VisualElement dot = EditorUI.Dot(_settings != null ? EditorUI.Green : EditorUI.Yellow);
            dot.AddToClassList("gm-label-dot");
            dotRow.Add(field);
            dotRow.Add(dot);
            card.Add(dotRow);

            if (_settings == null)
            {
                card.Add(new HelpBox("No game management settings asset: states and checks are off.", HelpBoxMessageType.Warning));
                Button create = EditorUI.PrimaryButton("Create settings", "Makes the settings and an empty state list in " + GameManagementSettings.DefaultFolder, () =>
                {
                    GameManagementSettingsAssets.Create(GameManagementSettings.DefaultFolder);
                    QueueRebuild();
                });
                create.AddToClassList("gm-left-button");
                card.Add(create);
                return;
            }

            VisualElement pathRow = EditorUI.Row();
            TextField path = new TextField("Asset path");
            path.isReadOnly = true;
            path.value = AssetDatabase.GetAssetPath(_settings);
            path.tooltip = path.value;
            path.AddToClassList("gm-grow");
            pathRow.Add(path);
            pathRow.Add(EditorUI.Button("Move...", "Moves the settings asset to another folder inside Assets", () => Move(_settings)));
            card.Add(pathRow);

            if (!GameManagementSettingsAssets.IsPreloaded(_settings))
            {
                card.Add(EditorUI.HelpBoxWithButton("Builds can't read these settings yet: they aren't in Player Settings > Preloaded Assets.",
                    HelpBoxMessageType.Warning, "Fix now", "Adds the settings to Preloaded Assets", () =>
                    {
                        GameManagementSettingsAssets.Use(_settings);
                        Debug.Log("[Game management] Added " + _settings.name + " to Preloaded Assets.", _settings);
                        QueueRebuild();
                    }));
            }
        }

        private static void Move(GameManagementSettings _settings)
        {
            string chosen = EditorUtility.OpenFolderPanel("Move the game management settings", Application.dataPath, "");
            if (string.IsNullOrEmpty(chosen))
            {
                return;
            }
            string relative = FileUtil.GetProjectRelativePath(chosen.Replace('\\', '/'));
            if (relative != "Assets" && !relative.StartsWith("Assets/"))
            {
                EditorUtility.DisplayDialog("Move settings", "Pick a folder inside this project's Assets folder.", "OK");
                return;
            }
            string from = AssetDatabase.GetAssetPath(_settings);
            string to = relative + "/" + Path.GetFileName(from);
            if (to == from)
            {
                return;
            }
            string error = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Move settings", "The settings could not move to " + to + ": " + error, "OK");
            }
            QueueRebuild();
        }

        private static void BuildStates(GameManagementSettings _settings)
        {
            VisualElement card = Card("2. States", out VisualElement header);
            GameStateCatalog catalog = _settings != null ? _settings.Catalog : null;
            List<StateProblem> problems = StateListValidator.Find(catalog);

            VisualElement status = EditorUI.Row();
            status.AddToClassList("gm-status");
            if (catalog != null)
            {
                status.Add(EditorUI.Dot(StatusColour(problems)));
            }
            Label summary = new Label(catalog != null ? Summary(catalog, problems) : "Not set up");
            summary.tooltip = summary.text;
            status.Add(summary);
            header.Insert(1, status);
            Button check = EditorUI.Button("Check again", "Checks the state list again", QueueRebuild);
            check.SetEnabled(catalog != null);
            header.Insert(2, check);

            VisualElement body = Body(card, _settings);
            if (_settings == null)
            {
                return;
            }
            SerializedObject data = new SerializedObject(_settings);
            body.Add(Property(data, nameof(GameManagementSettings.CheckStatesBeforeBuild), "Check states before a build", false));
            body.Add(Property(data, nameof(GameManagementSettings.Catalog), "Catalog", true));
            if (catalog == null)
            {
                body.Add(new HelpBox("The settings have no state list. Pick a State Catalog above.", HelpBoxMessageType.Error));
                return;
            }
            body.Add(stateList.Build(_settings, problems));
            body.Add(Hint("NOTE: To change what a state does, select it and edit it in the Inspector."));
        }

        private static Color StatusColour(List<StateProblem> _problems)
        {
            if (_problems.Exists(_problem => _problem.IsError))
            {
                return EditorUI.Red;
            }
            return _problems.Count > 0 ? EditorUI.Yellow : EditorUI.Green;
        }

        private static string Summary(GameStateCatalog _catalog, List<StateProblem> _problems)
        {
            int states = new HashSet<StateSO>(_catalog.States.FindAll(_state => _state != null)).Count;
            string text = Count(states, "state");
            int errors = _problems.FindAll(_problem => _problem.IsError).Count;
            int warnings = _problems.Count - errors;
            if (_problems.Count == 0)
            {
                return text + ", no problems";
            }
            if (errors > 0)
            {
                text += ", " + Count(errors, "error");
            }
            if (warnings > 0)
            {
                text += ", " + Count(warnings, "warning");
            }
            return text;
        }

        private static string Count(int _count, string _word)
        {
            return _count + " " + _word + (_count == 1 ? "" : "s");
        }

        private static PropertyField Property(SerializedObject _data, string _name, string _label, bool _rebuildOnChange)
        {
            SerializedProperty property = _data.FindProperty(_name);
            PropertyField field = new PropertyField(property, _label);
            field.tooltip = property.tooltip;
            field.Bind(_data);
            if (_rebuildOnChange)
            {
                // Tracks the value itself: a PropertyField also reports a change while it binds, which would rebuild forever.
                field.TrackPropertyValue(property, _changed => QueueRebuild());
            }
            return field;
        }

        private static void BuildBetweenScenes(GameManagementSettings _settings)
        {
            VisualElement body = Body(Card("3. Between scenes", out VisualElement _), _settings);
            DropdownField time = new DropdownField("When no state is active", new List<string> { "Keep last time scale", "Set time to 1" }, 0);
            time.tooltip = "What happens to time while no state is active, such as between scenes.";
            if (_settings != null)
            {
                time.SetValueWithoutNotify(time.choices[(int)_settings.NoStateTimeScale]);
                time.RegisterValueChangedCallback(_change =>
                {
                    Undo.RecordObject(_settings, "Change time between scenes");
                    _settings.NoStateTimeScale = (NoStateTimeScale)time.index;
                    EditorUtility.SetDirty(_settings);
                });
            }
            body.Add(time);
            body.Add(Hint("Keep last stops loaded shared scenes running before the new level is ready."));
        }

        private static void BuildInput(GameManagementSettings _settings)
        {
            VisualElement body = Body(Card("4. Input", out VisualElement _), _settings);
            DropdownField source = new DropdownField("Input source", new List<string> { "None", "Project-wide actions", "Chosen asset" }, 0);
            source.tooltip = "Which input actions states switch on and off. None leaves input alone.";
            body.Add(source);
            if (_settings == null)
            {
                return;
            }
            InputSourceKind[] kinds = { InputSourceKind.None, InputSourceKind.ProjectWideActions, InputSourceKind.ChosenAsset };
            source.SetValueWithoutNotify(source.choices[System.Array.IndexOf(kinds, _settings.InputSource)]);
            source.RegisterValueChangedCallback(_change =>
            {
                Undo.RecordObject(_settings, "Change input source");
                _settings.InputSource = kinds[source.index];
                EditorUtility.SetDirty(_settings);
                QueueRebuild();
            });
            if (_settings.InputSource == InputSourceKind.ChosenAsset)
            {
                body.Add(Property(new SerializedObject(_settings), nameof(GameManagementSettings.InputActions), "Input actions", true));
                if (_settings.InputActions == null)
                {
                    body.Add(new HelpBox("Chosen asset is selected but no asset is picked, so states leave input alone. Pick one in Input actions.", HelpBoxMessageType.Warning));
                }
            }
            if (_settings.InputSource == InputSourceKind.ProjectWideActions && InputSystem.actions == null)
            {
                body.Add(new HelpBox("The project has no project-wide actions, so states leave input alone. Set them in Project Settings > Input System Package.", HelpBoxMessageType.Warning));
            }
        }

        private static void BuildDebugger(GameManagementSettings _settings)
        {
            VisualElement body = Body(Card("5. Debugger", out VisualElement _), _settings);
            if (_settings != null)
            {
                body.Add(Property(new SerializedObject(_settings), nameof(GameManagementSettings.JournalSize), "Journal size", false));
            }
            else
            {
                body.Add(new IntegerField("Journal size"));
            }
            // These two are saved and read from the debugger session on; until then they show what is coming.
            ObjectField toggleAction = new ObjectField("Show or hide debugger");
            toggleAction.objectType = typeof(InputActionReference);
            toggleAction.tooltip = "Not available yet: it arrives with the in-game debugger.";
            toggleAction.SetEnabled(false);
            body.Add(toggleAction);
            Toggle release = new Toggle("Include the debugger in release builds");
            release.tooltip = toggleAction.tooltip;
            release.SetEnabled(false);
            body.Add(release);
        }
    }
}
