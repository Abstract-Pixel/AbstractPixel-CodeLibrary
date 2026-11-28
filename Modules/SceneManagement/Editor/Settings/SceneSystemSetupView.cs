using AbstractPixel.Core.Editor.UI;
using System;
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
    /// The setup wizard, shared by the setup window and the page's Setup section. The Abstract Pixel logo sits on top;
    /// under it, one question per page (session 5A2): 1 Scene System, 2 Settings Folder, 3 New Scenes, 4 Launch Scene,
    /// 5 Loading Scene, 6 Scene Sets, 7 Opening Scenes, then Ready and All set. A step that brings in a new word explains
    /// it in a small Terminology block; later steps carry the word's line as a tooltip. Next stays disabled while the
    /// page's answer can't work. Once the project is set up, the setup menu opens only the Opening Scenes step.
    /// </summary>
    public class SceneSystemSetupView : VisualElement
    {
        private enum Page
        {
            Welcome,
            Folder,
            NewScenes,
            LaunchScene,
            LoadingScene,
            SceneSets,
            OpeningScenes,
            Ready,
            Done
        }

        private const float LogoHeight = 88;
        private const int NumberedSteps = 7;
        public const string ChangeLaterText = "Change them later: run setup again (Tools > External Project Tools > Scene System > Set Up Scene System), Edit > Preferences > Abstract Pixel > Scenes, or the scene picker's footer.";

        private readonly Action<SceneSystemSettings> onDone;
        private readonly bool openingScenesOnly;
        private readonly SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
        private readonly SceneGroupMigrationPlan plan;
        private readonly List<Page> pages = new List<Page>();
        private readonly VisualElement content;
        private readonly Label stepLabel;
        private readonly Button backButton;
        private readonly Button nextButton;
        private int pageIndex;
        private string setupError = string.Empty;
        private VisualElement folderStatus;
        private SceneSystemSettings createdSettings;

        public SceneSystemSetupView(Action<SceneSystemSettings> _onDone) : this(_onDone, false)
        {
        }

        /// <param name="_openingScenesOnly">True once the project is set up: only the Opening Scenes step, with Done.</param>
        public SceneSystemSetupView(Action<SceneSystemSettings> _onDone, bool _openingScenesOnly)
        {
            onDone = _onDone;
            openingScenesOnly = _openingScenesOnly;
            plan = SceneSystemSetup.MigrationPlanFor(SceneGroupAssets.FindAll());
            AddPages();

            style.flexGrow = 1;
            style.minHeight = 340;
            style.paddingTop = 18;
            VisualElement logo = EditorUI.Logo(LogoHeight);
            logo.style.marginBottom = 14;
            Add(logo);
            VisualElement separator = EditorUI.Separator();
            separator.style.marginBottom = 18;
            Add(separator);
            content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.flexShrink = 1;
            content.style.justifyContent = Justify.FlexStart;
            content.style.paddingTop = 6;
            Add(content);

            VisualElement footer = new VisualElement();
            footer.style.marginTop = 14;
            footer.style.flexShrink = 0;
            stepLabel = new Label();
            stepLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            stepLabel.style.fontSize = 10;
            stepLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            stepLabel.style.marginBottom = 6;
            footer.Add(stepLabel);
            VisualElement buttons = EditorUI.Row();
            backButton = EditorUI.Button("Back", string.Empty, () => Go(-1));
            backButton.style.width = 100;
            buttons.Add(backButton);
            buttons.Add(EditorUI.Spacer());
            nextButton = EditorUI.Button("Next", string.Empty, OnNext);
            nextButton.style.width = 120;
            nextButton.style.height = 24;
            nextButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            buttons.Add(nextButton);
            footer.Add(buttons);
            Add(footer);

            Show();
        }

        private void AddPages()
        {
            if (openingScenesOnly)
            {
                pages.Add(Page.OpeningScenes);
                return;
            }
            pages.Add(Page.Welcome);
            pages.Add(Page.Folder);
            pages.Add(Page.NewScenes);
            pages.Add(Page.LaunchScene);
            pages.Add(Page.LoadingScene);
            pages.Add(Page.SceneSets);
            pages.Add(Page.OpeningScenes);
            pages.Add(Page.Ready);
            pages.Add(Page.Done);
        }

        private Page Current
        {
            get { return pages[pageIndex]; }
        }

        private void Go(int _step)
        {
            pageIndex = Mathf.Clamp(pageIndex + _step, 0, pages.Count - 1);
            Show();
        }

        private void OnNext()
        {
            if (openingScenesOnly || Current == Page.Done)
            {
                Finish();
                return;
            }
            if (Current == Page.Ready)
            {
                RunSetup();
                return;
            }
            Go(1);
        }

        private void Show()
        {
            content.Clear();
            switch (Current)
            {
                case Page.Welcome:
                    ShowWelcome();
                    break;
                case Page.Folder:
                    ShowFolder();
                    break;
                case Page.NewScenes:
                    ShowNewScenes();
                    break;
                case Page.LaunchScene:
                    ShowLaunchScene();
                    break;
                case Page.LoadingScene:
                    ShowLoadingScene();
                    break;
                case Page.SceneSets:
                    ShowSceneSets();
                    break;
                case Page.OpeningScenes:
                    ShowOpeningScenes();
                    break;
                case Page.Ready:
                    ShowReady();
                    break;
                default:
                    ShowDone();
                    break;
            }
            RefreshFooter();
        }

        // "Step N of 7" on the seven questions; Ready and All set have no number.
        private string StepText()
        {
            if (openingScenesOnly || Current == Page.Done)
            {
                return string.Empty;
            }
            if (Current == Page.Ready)
            {
                return "Review";
            }
            return "Step " + (pageIndex + 1) + " of " + NumberedSteps;
        }

        private void RefreshFooter()
        {
            bool isDone = Current == Page.Done;
            stepLabel.text = StepText();
            bool canGoBack = pageIndex > 0 && !isDone && !openingScenesOnly;
            backButton.style.visibility = canGoBack ? Visibility.Visible : Visibility.Hidden;

            nextButton.text = "Next";
            nextButton.tooltip = string.Empty;
            nextButton.SetEnabled(true);
            if (openingScenesOnly)
            {
                nextButton.text = "Done";
                return;
            }
            if (Current == Page.Welcome)
            {
                nextButton.text = "Get started";
            }
            if (Current == Page.Folder)
            {
                bool folderIsFine = string.IsNullOrEmpty(SceneSystemSetup.FolderProblemInProject(choices.Folder));
                nextButton.SetEnabled(folderIsFine);
            }
            if (Current == Page.LaunchScene)
            {
                string launchProblem = SceneSystemSetup.LaunchSceneProblem(choices);
                nextButton.text = choices.LaunchScene == LaunchSceneChoice.None ? "Skip" : "Next";
                nextButton.tooltip = launchProblem;
                nextButton.SetEnabled(string.IsNullOrEmpty(launchProblem));
            }
            if (Current == Page.LoadingScene)
            {
                string loadingProblem = SceneSystemSetup.LoadingSceneProblem(choices);
                nextButton.text = choices.LoadingScene == LoadingSceneChoice.None ? "Skip" : "Next";
                nextButton.tooltip = loadingProblem;
                nextButton.SetEnabled(string.IsNullOrEmpty(loadingProblem));
            }
            if (Current == Page.SceneSets)
            {
                string setsProblem = SceneSystemSetup.SceneSetsProblem(choices);
                nextButton.tooltip = setsProblem;
                nextButton.SetEnabled(string.IsNullOrEmpty(setsProblem));
            }
            if (Current == Page.Ready)
            {
                string blocking = SceneSystemSetup.BlockingProblem(choices);
                nextButton.text = "Set up";
                nextButton.tooltip = blocking;
                nextButton.SetEnabled(string.IsNullOrEmpty(blocking));
            }
            if (isDone)
            {
                nextButton.text = "Open Scene System";
                nextButton.tooltip = "Opens Project Settings > Abstract Pixel > Scene System.";
            }
        }

        // The page title, its line, then the Terminology lines for the step's new words.
        private void AddHeader(string _title, string _line)
        {
            content.Add(EditorUI.PageHeader(_title, _line));
            int step = (int)Current;
            bool hasWords = step < SceneSystemTerms.WordsPerStep.Length && SceneSystemTerms.WordsPerStep[step].Length > 0;
            if (hasWords)
            {
                content.Add(SceneSystemUI.Terminology(SceneSystemTerms.WordsPerStep[step]));
            }
            string topic = TopicFor(Current);
            if (topic != null)
            {
                Button learnMore = SceneSystemHelp.LearnMore(topic);
                learnMore.style.alignSelf = Align.Center;
                content.Add(learnMore);
            }
        }

        // The guide section each step's "Learn more" opens (All set has its own button instead).
        private static string TopicFor(Page _page)
        {
            switch (_page)
            {
                case Page.Welcome: return SceneSystemDocTopics.SetupWelcome;
                case Page.Folder: return SceneSystemDocTopics.SetupFolder;
                case Page.NewScenes: return SceneSystemDocTopics.SetupNewScenes;
                case Page.LaunchScene: return SceneSystemDocTopics.SetupLaunchScene;
                case Page.LoadingScene: return SceneSystemDocTopics.SetupLoadingScene;
                case Page.SceneSets: return SceneSystemDocTopics.SetupSceneSets;
                case Page.OpeningScenes: return SceneSystemDocTopics.SetupOpeningScenes;
                case Page.Ready: return SceneSystemDocTopics.SetupReady;
                default: return null;
            }
        }

        private void ShowWelcome()
        {
            AddHeader("Scene System", "Let's set up scenes for this project.<br>It takes a minute.");
            Label steps = EditorUI.Hint("You can change everything later in Project Settings.");
            steps.style.unityTextAlign = TextAnchor.MiddleCenter;
            content.Add(steps);
        }

        private void ShowFolder()
        {
            AddHeader("Settings Folder", "Where to keep the scene system's settings.");
            VisualElement row = EditorUI.Row();
            TextField folder = new TextField();
            folder.value = choices.Folder;
            folder.tooltip = "Any folder inside Assets/. You can move the settings later; the scene system finds them by type.";
            folder.style.flexGrow = 1;
            folder.style.marginLeft = 0;
            folder.RegisterValueChangedCallback(_change =>
            {
                choices.Folder = _change.newValue;
                RefreshFolderStatus();
                RefreshFooter();
            });
            row.Add(folder);
            row.Add(EditorUI.Button("Browse…", "Pick a folder inside this project's Assets folder.", () =>
            {
                string picked = EditorUtility.OpenFolderPanel("Settings folder", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(picked))
                {
                    folder.value = ToProjectPath(picked);
                }
            }));
            content.Add(row);
            folderStatus = new VisualElement();
            folderStatus.style.marginTop = 4;
            content.Add(folderStatus);
            RefreshFolderStatus();
        }

        private void RefreshFolderStatus()
        {
            if (folderStatus == null)
            {
                return;
            }
            folderStatus.Clear();
            string problem = SceneSystemSetup.FolderProblemInProject(choices.Folder);
            if (string.IsNullOrEmpty(problem))
            {
                folderStatus.Add(EditorUI.StatusRow(EditorUI.Green, "Looks good.", string.Empty));
                return;
            }
            folderStatus.Add(EditorUI.StatusRow(EditorUI.Red, problem, string.Empty));
        }

        private void ShowNewScenes()
        {
            AddHeader("New Scenes", "Where scenes go so they load in a build.");
            bool addressablesInstalled = AddressableScenes.IsInstalled;
            content.Add(EditorUI.ChoiceCard("Build Settings", "New scenes are added to the build's scene list.", !choices.NewScenesAreAddressable, true,
                "Scenes are added to File > Build Profiles > Scene List. Simple, and right for most games.", () => PickPlace(false)));
            string addressablesLine = addressablesInstalled ? "New scenes get an Addressables entry and load on demand." : "Needs the Addressables package.";
            content.Add(EditorUI.ChoiceCard("Addressables", addressablesLine, choices.NewScenesAreAddressable, addressablesInstalled,
                addressablesInstalled ? "Scenes get an entry in Addressables' default group instead of a place in the scene list." : SceneMoves.NeedsAddressablesTooltip, () => PickPlace(true)));
        }

        private void PickPlace(bool _addressables)
        {
            choices.NewScenesAreAddressable = _addressables;
            Show();
        }

        // Optional: a scene, such as a splash, that the game opens first. It is put at build index 0.
        private void ShowLaunchScene()
        {
            AddHeader("Launch Scene", "Optional: a splash or intro the game opens first.");
            content.Add(EditorUI.ChoiceCard("No launch scene", "The game opens the start scene group right away.", choices.LaunchScene == LaunchSceneChoice.None, true,
                "You can add one later in Project Settings > Abstract Pixel > Scene System > Start-up.", () => PickLaunchScene(LaunchSceneChoice.None)));
            content.Add(EditorUI.ChoiceCard("Use a scene I have", "The scene you pick opens first, then the start scene group.", choices.LaunchScene == LaunchSceneChoice.Existing, true,
                "It becomes the launch scene and moves to build index 0, the scene a build opens first.", () => PickLaunchScene(LaunchSceneChoice.Existing)));
            content.Add(EditorUI.ChoiceCard("Create one", "Setup makes a new scene with a camera and a light, and opens it first.", choices.LaunchScene == LaunchSceneChoice.CreateNew, true,
                "Setup saves a new scene where you choose, makes it the launch scene and puts it at build index 0.", () => PickLaunchScene(LaunchSceneChoice.CreateNew)));

            if (choices.LaunchScene == LaunchSceneChoice.Existing)
            {
                ObjectField scenePicker = new ObjectField("Scene");
                scenePicker.objectType = typeof(SceneAsset);
                scenePicker.allowSceneObjects = false;
                scenePicker.SetValueWithoutNotify(choices.ExistingLaunchScene);
                scenePicker.tooltip = "The scene the game opens first.";
                scenePicker.style.marginTop = 6;
                scenePicker.RegisterValueChangedCallback(_change =>
                {
                    choices.ExistingLaunchScene = _change.newValue as SceneAsset;
                    RefreshFooter();
                });
                content.Add(scenePicker);
            }
            if (choices.LaunchScene == LaunchSceneChoice.CreateNew)
            {
                VisualElement row = EditorUI.Row();
                row.style.marginTop = 6;
                TextField path = new TextField("Save as");
                path.value = choices.NewLaunchScenePath;
                path.tooltip = "Where to save the new launch scene, inside Assets/.";
                path.style.flexGrow = 1;
                path.RegisterValueChangedCallback(_change =>
                {
                    choices.NewLaunchScenePath = _change.newValue;
                    RefreshFooter();
                });
                row.Add(path);
                row.Add(EditorUI.Button("Choose…", "Pick where to save the launch scene.", () =>
                {
                    string picked = LaunchSceneCreation.AskForPath(Path.GetDirectoryName(choices.NewLaunchScenePath));
                    if (!string.IsNullOrEmpty(picked))
                    {
                        path.value = picked;
                    }
                }));
                content.Add(row);
            }
        }

        private void PickLaunchScene(LaunchSceneChoice _choice)
        {
            choices.LaunchScene = _choice;
            Show();
        }

        // Optional: the scene shown while one place closes and the next opens. Scene groups still choose when to show it.
        private void ShowLoadingScene()
        {
            AddHeader("Loading Scene", "Optional: a scene shown while the next place loads.");
            bool readyMadeExists = SceneSystemSetup.LibraryLoadingSceneExists();
            string readyMadeLine = readyMadeExists ? "Setup copies a loading scene with a progress bar for you to restyle." : "Not in this project.";
            string groupTerm = SceneSystemTerms.Tooltip(SceneSystemTerms.SceneGroup);
            content.Add(EditorUI.ChoiceCard("Ready-made loading scene", readyMadeLine, choices.LoadingScene == LoadingSceneChoice.ReadyMade, readyMadeExists,
                SceneSystemTerms.WithTerms("Setup copies the library's loading scene next to the settings and makes it the default loading scene. The library's original stays untouched.", SceneSystemTerms.SceneGroup), () => PickLoadingScene(LoadingSceneChoice.ReadyMade)));
            content.Add(EditorUI.ChoiceCard("Use a scene I have", "The scene you pick shows while the next place loads.", choices.LoadingScene == LoadingSceneChoice.Existing, true,
                "It becomes the default loading scene and goes where new scenes go, so it loads in a build.", () => PickLoadingScene(LoadingSceneChoice.Existing)));
            content.Add(EditorUI.ChoiceCard("No loading scene", "Scene changes show only the screen effect, such as a fade.", choices.LoadingScene == LoadingSceneChoice.None, true,
                "You can add one later in Project Settings > Abstract Pixel > Scene System > Loading scenes.", () => PickLoadingScene(LoadingSceneChoice.None)));

            if (choices.LoadingScene == LoadingSceneChoice.Existing)
            {
                ObjectField scenePicker = new ObjectField("Scene");
                scenePicker.objectType = typeof(SceneAsset);
                scenePicker.allowSceneObjects = false;
                scenePicker.SetValueWithoutNotify(choices.ExistingLoadingScene);
                scenePicker.tooltip = "The scene shown while the next place loads.";
                scenePicker.style.marginTop = 6;
                scenePicker.RegisterValueChangedCallback(_change =>
                {
                    choices.ExistingLoadingScene = _change.newValue as SceneAsset;
                    RefreshFooter();
                });
                content.Add(scenePicker);
            }
        }

        private void PickLoadingScene(LoadingSceneChoice _choice)
        {
            choices.LoadingScene = _choice;
            Show();
        }

        // "Set up your scene sets now?" Yes lists the sets to make; Not now makes the Default Scene Set as before.
        private void ShowSceneSets()
        {
            AddHeader("Scene Sets", "Set up your scene sets now?");
            string fade = SceneSystemSetup.LibraryFadeExists() ? ", holding a copy of the fade" : string.Empty;
            content.Add(EditorUI.ChoiceCard("Yes", "Pick the manager and shared scenes for each scene set now.", choices.SetUpSceneSetsNow, true,
                SceneSystemTerms.WithTerms("The first scene set is the default one. You can change every scene set later on the Scene System page.", SceneSystemTerms.SceneGroup), () => PickSetUpSets(true)));
            content.Add(EditorUI.ChoiceCard("Not now", "Setup makes the Default Scene Set with a new Managers scene" + fade + ".", !choices.SetUpSceneSetsNow, true,
                "Add scenes to it later on the Scene System page, in Scene sets.", () => PickSetUpSets(false)));
            if (!choices.SetUpSceneSetsNow)
            {
                return;
            }

            ScrollView scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.marginTop = 4;
            for (int index = 0; index < choices.SceneSets.Count; index++)
            {
                scroll.Add(DraftCard(index));
            }
            Button addSet = EditorUI.Button("Add another scene set", "Adds an empty scene set. Scene groups pick their scene set on the Scene System page.", () =>
            {
                choices.SceneSets.Add(SceneSetDraft.Another(choices.SceneSets));
                Show();
            });
            addSet.style.alignSelf = Align.FlexStart;
            addSet.style.marginTop = 2;
            scroll.Add(addSet);
            content.Add(scroll);
        }

        private void PickSetUpSets(bool _now)
        {
            choices.SetUpSceneSetsNow = _now;
            Show();
        }

        // One scene set to make: its name, manager scenes, shared scenes and Reset shared scenes.
        private VisualElement DraftCard(int _index)
        {
            SceneSetDraft draft = choices.SceneSets[_index];
            VisualElement card = EditorUI.Card();
            card.style.marginBottom = 6;

            VisualElement nameRow = EditorUI.Row();
            TextField name = new TextField("Name");
            name.value = draft.Name;
            name.tooltip = "The scene set's asset name.";
            name.style.flexGrow = 1;
            name.style.marginLeft = 0;
            nameRow.Add(name);
            if (_index == 0)
            {
                nameRow.Add(EditorUI.Badge("Default", false, "The first scene set is the default: scene groups use it unless they pick another."));
            }
            else
            {
                nameRow.Add(EditorUI.Button("Remove", "Removes this scene set from the list. Nothing is made for it.", () =>
                {
                    choices.SceneSets.RemoveAt(_index);
                    Show();
                }));
            }
            card.Add(nameRow);
            Label nameProblem = new Label();
            nameProblem.style.color = EditorUI.Red;
            nameProblem.style.fontSize = 11;
            nameProblem.style.whiteSpace = WhiteSpace.Normal;
            card.Add(nameProblem);
            RefreshNameProblem(nameProblem, _index);
            name.RegisterValueChangedCallback(_change =>
            {
                draft.Name = _change.newValue;
                RefreshNameProblem(nameProblem, _index);
                RefreshFooter();
            });

            card.Add(SceneList("Manager scenes", SceneSystemTerms.Tooltip(SceneSystemTerms.ManagerScene), draft.ManagerScenes, draft.IncludeStarterManagers, () =>
            {
                draft.IncludeStarterManagers = false;
                Show();
            }));
            card.Add(SceneList("Shared scenes", SceneSystemTerms.Tooltip(SceneSystemTerms.SharedScene), draft.SharedScenes, false, null));

            Toggle reset = new Toggle("Reset shared scenes");
            reset.tooltip = "Closes and reopens every shared scene on each scene change, so state kept in them starts fresh.";
            reset.SetValueWithoutNotify(draft.ResetSharedScenes);
            reset.RegisterValueChangedCallback(_change => draft.ResetSharedScenes = _change.newValue);
            card.Add(reset);
            return card;
        }

        private void RefreshNameProblem(Label _label, int _index)
        {
            List<string> others = new List<string>();
            for (int index = 0; index < choices.SceneSets.Count; index++)
            {
                if (index != _index)
                {
                    others.Add(choices.SceneSets[index].Name);
                }
            }
            string problem = SceneSetNames.Problem(choices.SceneSets[_index].Name, others);
            _label.text = problem;
            _label.style.display = string.IsNullOrEmpty(problem) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // A titled list of scene fields with Remove buttons and "Add scene"; the starter Managers scene comes first.
        private VisualElement SceneList(string _title, string _tooltip, List<SceneAsset> _scenes, bool _withStarter, Action _removeStarter)
        {
            VisualElement list = new VisualElement();
            list.style.marginTop = 6;
            Label title = new Label(_title);
            title.tooltip = _tooltip;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 2;
            list.Add(title);
            if (_withStarter)
            {
                VisualElement starterRow = EditorUI.Row();
                Label starter = new Label("Managers (new, made by setup)");
                starter.tooltip = "An empty manager scene setup makes in the settings folder, holding a copy of the library's basic fade. Remove it if your own manager scenes are enough.";
                starter.style.flexGrow = 1;
                starter.style.paddingLeft = 3;
                starterRow.Add(starter);
                starterRow.Add(EditorUI.Button("Remove", "Setup will not make the starter Managers scene for this scene set.", _removeStarter));
                list.Add(starterRow);
            }
            for (int index = 0; index < _scenes.Count; index++)
            {
                list.Add(SceneRow(_scenes, index));
            }
            Button add = EditorUI.Button("Add scene", "Adds a row to pick a scene from this project.", () =>
            {
                _scenes.Add(null);
                Show();
            });
            add.style.alignSelf = Align.FlexStart;
            list.Add(add);
            return list;
        }

        private VisualElement SceneRow(List<SceneAsset> _scenes, int _index)
        {
            VisualElement row = EditorUI.Row();
            ObjectField field = new ObjectField();
            field.objectType = typeof(SceneAsset);
            field.allowSceneObjects = false;
            field.SetValueWithoutNotify(_scenes[_index]);
            field.style.flexGrow = 1;
            field.style.marginLeft = 0;
            field.RegisterValueChangedCallback(_change =>
            {
                _scenes[_index] = _change.newValue as SceneAsset;
                RefreshFooter();
            });
            row.Add(field);
            row.Add(EditorUI.Button("Remove", "Removes this scene from the list.", () =>
            {
                _scenes.RemoveAt(_index);
                Show();
            }));
            return row;
        }

        // The two scene opening preferences, written as soon as they change; they are per user, not project settings.
        private void ShowOpeningScenes()
        {
            AddHeader("Opening Scenes", "What else opens when you open a scene in the Editor.");
            content.Add(OpeningToggle(SceneOpeningPreferences.MainSceneWithGroupLabel,
                SceneSystemTerms.WithTerms(SceneOpeningPreferences.MainSceneWithGroupTooltip, SceneSystemTerms.SceneGroup, SceneSystemTerms.StartSceneGroup),
                SceneOpeningPreferences.MainSceneWithGroup, _value => SceneOpeningPreferences.MainSceneWithGroup = _value));
            content.Add(OpeningToggle(SceneOpeningPreferences.NoGroupSceneWithDefaultSetLabel,
                SceneSystemTerms.WithTerms(SceneOpeningPreferences.NoGroupSceneWithDefaultSetTooltip, SceneSystemTerms.SceneSet),
                SceneOpeningPreferences.NoGroupSceneWithDefaultSet, _value => SceneOpeningPreferences.NoGroupSceneWithDefaultSet = _value));
            Label play = EditorUI.Hint(SceneOpeningPreferencesProvider.PlayNote);
            play.style.marginTop = 8;
            content.Add(play);
            Label later = EditorUI.Hint(ChangeLaterText);
            later.style.marginTop = 6;
            content.Add(later);
        }

        private static VisualElement OpeningToggle(string _label, string _tooltip, bool _value, Action<bool> _changed)
        {
            Toggle toggle = new Toggle();
            toggle.text = _label;
            toggle.tooltip = _tooltip;
            toggle.style.marginBottom = 4;
            toggle.SetValueWithoutNotify(_value);
            toggle.RegisterValueChangedCallback(_change => _changed(_change.newValue));
            return toggle;
        }

        private void ShowReady()
        {
            AddHeader("Ready", "This is what setup will make.");
            string folder = SceneSystemSetup.NormalizeFolder(choices.Folder);
            string place = choices.NewScenesAreAddressable && AddressableScenes.IsInstalled ? "Addressables" : "Build Settings";
            bool hasFade = SceneSystemSetup.LibraryFadeExists();
            VisualElement list = EditorUI.Card();
            list.Add(Check("Settings in " + folder));
            foreach (string setLine in SceneSystemSetup.SceneSetLines(choices, hasFade))
            {
                list.Add(Check(setLine));
            }
            list.Add(Check("New scenes go to " + place));
            string launchLine = SceneSystemSetup.LaunchSceneLine(choices);
            if (!string.IsNullOrEmpty(launchLine))
            {
                list.Add(Check(launchLine));
            }
            string loadingLine = SceneSystemSetup.LoadingSceneLine(choices);
            if (!string.IsNullOrEmpty(loadingLine))
            {
                list.Add(Check(loadingLine));
            }
            list.tooltip = string.Join("\n", SceneSystemSetup.WillCreate(choices, hasFade, AddressableScenes.IsInstalled));
            content.Add(list);

            string migrationHint = SceneSystemSetup.MigrationHint(plan);
            if (!string.IsNullOrEmpty(migrationHint))
            {
                Label hint = EditorUI.Hint(migrationHint);
                hint.style.marginTop = 8;
                content.Add(hint);
            }

            string blocking = string.IsNullOrEmpty(setupError) ? SceneSystemSetup.BlockingProblem(choices) : setupError;
            if (!string.IsNullOrEmpty(blocking))
            {
                HelpBox problem = new HelpBox(blocking, HelpBoxMessageType.Warning);
                problem.style.marginTop = 8;
                content.Add(problem);
            }
        }

        private static VisualElement Check(string _text)
        {
            VisualElement row = EditorUI.StatusRow(EditorUI.Green, _text, string.Empty);
            row.style.marginTop = 4;
            row.style.marginBottom = 4;
            return row;
        }

        private void ShowDone()
        {
            AddHeader("All set", "The scene system is ready.");
            VisualElement dot = EditorUI.Dot(EditorUI.Green);
            dot.style.width = 36;
            dot.style.height = 36;
            dot.style.alignSelf = Align.Center;
            dot.style.borderTopLeftRadius = 18;
            dot.style.borderTopRightRadius = 18;
            dot.style.borderBottomLeftRadius = 18;
            dot.style.borderBottomRightRadius = 18;
            content.Add(dot);
            VisualElement pickerRow = ScenePickerToolbarHint.Centered();
            pickerRow.style.marginTop = 12;
            content.Add(pickerRow);
            Button docs = EditorUI.Button("Open Scene Documentation", "Opens the scene documentation at its Home page", () => SceneSystemDocs.Open(SceneSystemDocTopics.Home));
            docs.AddToClassList(SceneSystemHelp.ButtonClass);
            docs.userData = SceneSystemDocTopics.Home;
            docs.style.alignSelf = Align.Center;
            docs.style.marginTop = 10;
            content.Add(docs);
        }

        private void RunSetup()
        {
            SceneSystemSettings settings = SceneSystemSetup.Run(choices, out string error);
            if (settings == null)
            {
                setupError = error;
                Show();
                return;
            }
            setupError = string.Empty;
            createdSettings = settings;
            Go(1);
        }

        private void Finish()
        {
            if (onDone != null)
            {
                onDone(createdSettings);
            }
        }

        // "C:/Project/Assets/SceneSystem" becomes "Assets/SceneSystem"; a folder outside the project stays as it is, so
        // the check under the field says why it can't be used.
        private static string ToProjectPath(string _absolutePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            string normalized = _absolutePath.Replace('\\', '/');
            if (normalized.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(projectRoot.Length + 1);
            }
            return normalized;
        }
    }
}
