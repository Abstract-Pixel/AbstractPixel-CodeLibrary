using System;
using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The setup wizard, shared by the setup window and the page's Setup section. The Abstract Pixel logo sits on top;
    /// under it, one question per page, few words:
    /// Welcome, Settings folder, New scenes, Launch scene (optional), Scene groups (only when the project has some), Ready, Done. Details are in
    /// tooltips. Next stays disabled while the page's answer can't work.
    /// </summary>
    public class SceneSystemSetupView : VisualElement
    {
        private enum Page
        {
            Welcome,
            Folder,
            NewScenes,
            LaunchScene,
            SceneGroups,
            Ready,
            Done
        }

        private const float LogoHeight = 88;

        private readonly Action<SceneSystemSettings> onDone;
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

        public SceneSystemSetupView(Action<SceneSystemSettings> _onDone)
        {
            onDone = _onDone;
            plan = SceneGroupMigration.Plan(SceneGroupAssets.FindAll());
            pages.Add(Page.Welcome);
            pages.Add(Page.Folder);
            pages.Add(Page.NewScenes);
            pages.Add(Page.LaunchScene);
            if (plan.HasWork)
            {
                pages.Add(Page.SceneGroups);
            }
            pages.Add(Page.Ready);
            pages.Add(Page.Done);

            style.flexGrow = 1;
            style.minHeight = 340;
            style.paddingTop = 18;
            VisualElement logo = SceneSystemUI.Logo(LogoHeight);
            logo.style.marginBottom = 14;
            Add(logo);
            VisualElement separator = SceneSystemUI.Separator();
            separator.style.marginBottom = 18;
            Add(separator);
            content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.justifyContent = Justify.FlexStart;
            content.style.paddingTop = 6;
            Add(content);

            VisualElement footer = new VisualElement();
            footer.style.marginTop = 14;
            stepLabel = new Label();
            stepLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            stepLabel.style.fontSize = 10;
            stepLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            stepLabel.style.marginBottom = 6;
            footer.Add(stepLabel);
            VisualElement buttons = SceneSystemUI.Row();
            backButton = SceneSystemUI.Button("Back", string.Empty, () => Go(-1));
            backButton.style.width = 100;
            buttons.Add(backButton);
            buttons.Add(SceneSystemUI.Spacer());
            nextButton = SceneSystemUI.Button("Next", string.Empty, OnNext);
            nextButton.style.width = 120;
            nextButton.style.height = 24;
            nextButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            buttons.Add(nextButton);
            footer.Add(buttons);
            Add(footer);

            Show();
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
            if (Current == Page.Ready)
            {
                RunSetup();
                return;
            }
            if (Current == Page.Done)
            {
                Finish();
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
                case Page.SceneGroups:
                    ShowSceneGroups();
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

        private void RefreshFooter()
        {
            bool isDone = Current == Page.Done;
            int questionCount = pages.Count - 1;
            stepLabel.text = isDone ? string.Empty : "Step " + (pageIndex + 1) + " of " + questionCount;
            backButton.style.visibility = pageIndex > 0 && !isDone ? Visibility.Visible : Visibility.Hidden;

            nextButton.text = "Next";
            nextButton.tooltip = string.Empty;
            nextButton.SetEnabled(true);
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
            if (Current == Page.Ready)
            {
                string blocking = SceneSystemSetup.BlockingProblem(choices, plan);
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

        private void ShowWelcome()
        {
            content.Add(SceneSystemUI.PageHeader("Scene System", "Let's set up scenes for this project.<br>It takes a minute."));
            Label steps = SceneSystemUI.Hint("You can change everything later in Project Settings.");
            steps.style.unityTextAlign = TextAnchor.MiddleCenter;
            content.Add(steps);
        }

        private void ShowFolder()
        {
            content.Add(SceneSystemUI.PageHeader("Settings Folder", "Where to keep the scene system's settings."));
            VisualElement row = SceneSystemUI.Row();
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
            row.Add(SceneSystemUI.Button("Browse…", "Pick a folder inside this project's Assets folder.", () =>
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
                folderStatus.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "Looks good.", string.Empty));
                return;
            }
            folderStatus.Add(SceneSystemUI.StatusRow(SceneSystemUI.Red, problem, string.Empty));
        }

        private void ShowNewScenes()
        {
            content.Add(SceneSystemUI.PageHeader("New Scenes", "Where scenes go so they load in a build."));
            bool addressablesInstalled = AddressableScenes.IsInstalled;
            content.Add(SceneSystemUI.ChoiceCard("Build Settings", "Simple. Right for most games.", !choices.NewScenesAreAddressable, true,
                "Scenes are added to File > Build Settings.", () => PickPlace(false)));
            string addressablesLine = addressablesInstalled ? "Load scenes on demand." : "Needs the Addressables package.";
            content.Add(SceneSystemUI.ChoiceCard("Addressables", addressablesLine, choices.NewScenesAreAddressable, addressablesInstalled,
                addressablesInstalled ? "Scenes get an Addressables entry instead." : SceneMoves.NeedsAddressablesTooltip, () => PickPlace(true)));
        }

        private void PickPlace(bool _addressables)
        {
            choices.NewScenesAreAddressable = _addressables;
            Show();
        }

        // Optional: a scene, such as a splash, that the game opens first. It is put at build index 0.
        private void ShowLaunchScene()
        {
            content.Add(SceneSystemUI.PageHeader("Launch Scene", "Optional: a splash or intro the game opens first."));
            content.Add(SceneSystemUI.ChoiceCard("No launch scene", "The game opens its start scene group straight away.", choices.LaunchScene == LaunchSceneChoice.None, true,
                "You can add one later in Project Settings > Abstract Pixel > Scene System > Start-up.", () => PickLaunchScene(LaunchSceneChoice.None)));
            content.Add(SceneSystemUI.ChoiceCard("Use a scene I have", "Pick the splash or intro scene.", choices.LaunchScene == LaunchSceneChoice.Existing, true,
                "It becomes the launch scene and moves to build index 0, the scene a build opens first.", () => PickLaunchScene(LaunchSceneChoice.Existing)));
            content.Add(SceneSystemUI.ChoiceCard("Create one", "A new scene with a camera and a light.", choices.LaunchScene == LaunchSceneChoice.CreateNew, true,
                "Setup saves a new scene where you choose, makes it the launch scene and puts it at build index 0.", () => PickLaunchScene(LaunchSceneChoice.CreateNew)));

            if (choices.LaunchScene == LaunchSceneChoice.Existing)
            {
                UnityEditor.UIElements.ObjectField scenePicker = new UnityEditor.UIElements.ObjectField("Scene");
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
                VisualElement row = SceneSystemUI.Row();
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
                row.Add(SceneSystemUI.Button("Choose…", "Pick where to save the launch scene.", () =>
                {
                    string picked = LaunchSceneCreation.AskForPath(System.IO.Path.GetDirectoryName(choices.NewLaunchScenePath));
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

        private void ShowSceneGroups()
        {
            content.Add(SceneSystemUI.PageHeader("Scene Groups", "Found <b>" + plan.GroupsToMigrateCount + "</b> scene groups in this project."));
            content.Add(SceneSystemUI.ChoiceCard("Organize them (recommended)", "Into " + plan.SetNames.Count + " scene sets: " + string.Join(", ", plan.SetNames) + ".", choices.MigrateGroups, true,
                "Groups that share the same manager and shared scenes get one scene set. Every group keeps every scene; nothing is deleted. A report lists each group.", () => PickMigrate(true)));
            content.Add(SceneSystemUI.ChoiceCard("Leave them as they are", "Start with an empty scene set instead.", !choices.MigrateGroups, true,
                "You can organize them later from Project Settings > Abstract Pixel > Scene System.", () => PickMigrate(false)));
        }

        private void PickMigrate(bool _migrate)
        {
            choices.MigrateGroups = _migrate;
            Show();
        }

        private void ShowReady()
        {
            content.Add(SceneSystemUI.PageHeader("Ready", "This is what setup will make."));
            string folder = SceneSystemSetup.NormalizeFolder(choices.Folder);
            string place = choices.NewScenesAreAddressable && AddressableScenes.IsInstalled ? "Addressables" : "Build Settings";
            VisualElement list = SceneSystemUI.Card();
            list.Add(Check("Settings in " + folder));
            if (SceneSystemSetup.WillMigrate(choices, plan))
            {
                list.Add(Check(plan.SetNames.Count + " scene sets: " + string.Join(", ", plan.SetNames)));
            }
            else
            {
                string fade = SceneSystemSetup.LibraryFadeExists() ? " with a fade" : string.Empty;
                list.Add(Check("A starter scene set" + fade));
            }
            list.Add(Check("New scenes go to " + place));
            string launchLine = SceneSystemSetup.LaunchSceneLine(choices);
            if (!string.IsNullOrEmpty(launchLine))
            {
                list.Add(Check(launchLine));
            }
            list.tooltip = string.Join("\n", SceneSystemSetup.WillCreate(choices, plan, SceneSystemSetup.LibraryFadeExists(), AddressableScenes.IsInstalled));
            content.Add(list);

            string blocking = string.IsNullOrEmpty(setupError) ? SceneSystemSetup.BlockingProblem(choices, plan) : setupError;
            if (!string.IsNullOrEmpty(blocking))
            {
                HelpBox problem = new HelpBox(blocking, HelpBoxMessageType.Warning);
                problem.style.marginTop = 8;
                content.Add(problem);
            }
        }

        private static VisualElement Check(string _text)
        {
            VisualElement row = SceneSystemUI.StatusRow(SceneSystemUI.Green, _text, string.Empty);
            row.style.marginTop = 4;
            row.style.marginBottom = 4;
            return row;
        }

        private void ShowDone()
        {
            content.Add(SceneSystemUI.PageHeader("All set", "The scene system is ready."));
            VisualElement dot = SceneSystemUI.Dot(SceneSystemUI.Green);
            dot.style.width = 36;
            dot.style.height = 36;
            dot.style.alignSelf = Align.Center;
            dot.style.borderTopLeftRadius = 18;
            dot.style.borderTopRightRadius = 18;
            dot.style.borderBottomLeftRadius = 18;
            dot.style.borderBottomRightRadius = 18;
            content.Add(dot);
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
