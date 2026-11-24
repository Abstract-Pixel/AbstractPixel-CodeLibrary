using System.Collections.Generic;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The loading scene sandbox: the Loading scenes settings and a scene group's loading scene fields as they look in
    /// the project, each with what it means under it, then a run of one scene change with a small screen and a log of
    /// what happens and why. Everything lives in this page: nothing reads back into the project or saves an asset.
    /// </summary>
    public class SceneDocsSandbox
    {
        private static readonly List<string> ShowChoices = new List<string> { "Never", "When arriving from another scene group", "Every time, Retry included" };
        private static readonly List<string> ChangeChoices = new List<string> { "From another scene group", "Retry the same group", "Switch to a preloaded group" };
        private const string NoDefault = "None";

        public readonly VisualElement Root = new VisualElement();
        private SceneDocsSandboxModel model;
        private readonly VisualElement settingsPanel = new VisualElement();
        private readonly VisualElement groupPanel = new VisualElement();
        private readonly Label verdict = new Label();
        private readonly ScrollView console = new ScrollView();
        private readonly VisualElement screen = new VisualElement();
        private readonly Label screenLabel = new Label();
        private readonly VisualElement bar = new VisualElement();
        private readonly VisualElement barFill = new VisualElement();
        private readonly VisualElement cover = new VisualElement();
        private IVisualElementScheduledItem ticker;

        public static VisualElement Render(string _name)
        {
            SceneDocsSandbox sandbox = new SceneDocsSandbox();
            sandbox.Root.userData = sandbox;
            return sandbox.Root;
        }

        public SceneDocsSandbox()
        {
            model = SceneDocsSandboxModel.FromProject();
            Root.AddToClassList("docs-sandbox");
            Root.Add(Header());
            VisualElement panels = new VisualElement();
            panels.AddToClassList("docs-sandbox-panels");
            settingsPanel.AddToClassList("docs-sandbox-panel");
            groupPanel.AddToClassList("docs-sandbox-panel");
            panels.Add(settingsPanel);
            panels.Add(groupPanel);
            Root.Add(panels);
            Root.Add(RunPanel());
            Root.Add(Output());
            Rebuild();
        }

        private VisualElement Header()
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-sandbox-header");
            Label pill = new Label("Sandbox");
            pill.AddToClassList("docs-sandbox-pill");
            header.Add(pill);
            Label line = new Label("A copy of your settings that lives only in this page. Change anything, then press Run. Your project never changes.");
            line.AddToClassList("docs-sandbox-lede");
            header.Add(line);
            Button reset = new Button(() => { model = SceneDocsSandboxModel.FromProject(); Rebuild(); });
            reset.text = "Reset";
            reset.tooltip = "Copies your project's settings into the sandbox again";
            header.Add(reset);
            return header;
        }

        private void Rebuild()
        {
            StopRun();
            settingsPanel.Clear();
            groupPanel.Clear();
            settingsPanel.Add(PanelTitle("Project Settings › Scene System › " + SceneSystemLoadingScenesSection.Title));
            settingsPanel.Add(Explained(LoadingScenesList(), "The loading scenes this project has. Each is one scene, never a scene group. A scene group can only pick one from this list."));
            settingsPanel.Add(Explained(DefaultPopup(), "What a scene group shows when it shows a loading scene but picked none. With None, such a group shows nothing and the Console warns."));
            FloatField minimum = new FloatField("Minimum time on screen (s)");
            minimum.tooltip = SceneSystemLoadingScenesSection.MinimumTooltip;
            minimum.value = model.MinimumSeconds;
            minimum.RegisterValueChangedCallback(_change => model.MinimumSeconds = Mathf.Max(0f, _change.newValue));
            settingsPanel.Add(Explained(minimum, "Unscaled seconds the loading scene stays up, counted from Show screen starting, so a fast load does not flash it. 0 turns it off."));

            groupPanel.Add(PanelTitle("Scene group › " + SceneDocsSandboxModel.GroupName));
            PopupField<string> show = new PopupField<string>("Show the loading scene", ShowChoices, (int)model.Show);
            show.RegisterValueChangedCallback(_change => { model.Show = (ShowLoadingScene)ShowChoices.IndexOf(_change.newValue); Rebuild(); });
            groupPanel.Add(Explained(show, "Never: changes into this group never show one. When arriving from another scene group: every change in, except a Retry. Every time: a Retry too."));
            if (model.Show != ShowLoadingScene.Never)
            {
                groupPanel.Add(Explained(GroupPick(), "Which loading scene this group shows. Default uses the project's default loading scene."));
            }
            UpdateVerdict();
        }

        private static Label PanelTitle(string _text)
        {
            Label title = new Label(_text);
            title.AddToClassList("docs-sandbox-panel-title");
            return title;
        }

        private static VisualElement Explained(VisualElement _field, string _help)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-sandbox-field");
            _field.AddToClassList("docs-sandbox-control");
            row.Add(_field);
            Label help = new Label(_help);
            help.AddToClassList("docs-sandbox-help");
            row.Add(help);
            return row;
        }

        private VisualElement LoadingScenesList()
        {
            ListView list = new ListView(model.LoadingScenes, 20, () => new TextField(), (_element, _index) =>
            {
                TextField field = (TextField)_element;
                field.userData = _index;
                field.SetValueWithoutNotify(model.LoadingScenes[_index]);
            });
            list.headerTitle = SceneSystemLoadingScenesSection.Title;
            list.showFoldoutHeader = true;
            list.showAddRemoveFooter = true;
            list.showBorder = true;
            list.reorderable = true;
            list.tooltip = SceneSystemLoadingScenesSection.ListTooltip;
            list.itemsAdded += _indexes => { foreach (int index in _indexes) { model.LoadingScenes[index] = "New Loading Scene " + (index + 1); } list.schedule.Execute(Rebuild); };
            list.itemsRemoved += _indexes => list.schedule.Execute(Rebuild);
            list.RegisterCallback<ChangeEvent<string>>(_change =>
            {
                TextField field = _change.target as TextField;
                if (field != null && field.userData is int index && index < model.LoadingScenes.Count)
                {
                    model.LoadingScenes[index] = _change.newValue;
                }
            });
            list.RegisterCallback<FocusOutEvent>(_focus => list.schedule.Execute(Rebuild));
            return list;
        }

        private PopupField<string> DefaultPopup()
        {
            List<string> choices = new List<string> { NoDefault };
            choices.AddRange(Names());
            string current = choices.Contains(model.DefaultLoadingScene) ? model.DefaultLoadingScene : NoDefault;
            PopupField<string> popup = new PopupField<string>("Default loading scene", choices, current);
            popup.tooltip = SceneSystemLoadingScenesSection.DefaultTooltip;
            popup.RegisterValueChangedCallback(_change => { model.DefaultLoadingScene = _change.newValue == NoDefault ? "" : _change.newValue; Rebuild(); });
            return popup;
        }

        private PopupField<string> GroupPick()
        {
            string defaultLabel = string.IsNullOrEmpty(model.DefaultLoadingScene) ? "Default (none set)" : "Default (" + model.DefaultLoadingScene + ")";
            List<string> choices = new List<string> { defaultLabel };
            choices.AddRange(Names());
            string current = choices.Contains(model.GroupPick) ? model.GroupPick : defaultLabel;
            PopupField<string> pick = new PopupField<string>("Loading scene", choices, current);
            pick.RegisterValueChangedCallback(_change => { model.GroupPick = _change.newValue == defaultLabel ? "" : _change.newValue; UpdateVerdict(); });
            return pick;
        }

        private List<string> Names()
        {
            List<string> names = new List<string>();
            foreach (string name in model.LoadingScenes)
            {
                if (!string.IsNullOrEmpty(name) && !names.Contains(name))
                {
                    names.Add(name);
                }
            }
            return names;
        }

        private VisualElement RunPanel()
        {
            VisualElement panel = new VisualElement();
            panel.AddToClassList("docs-sandbox-run-panel");
            panel.Add(PanelTitle("Run a scene change into " + SceneDocsSandboxModel.GroupName));
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-sandbox-run-row");
            PopupField<string> change = new PopupField<string>("Change", ChangeChoices, (int)model.Change);
            change.RegisterValueChangedCallback(_change => { model.Change = (SandboxChange)ChangeChoices.IndexOf(_change.newValue); UpdateVerdict(); });
            change.AddToClassList("docs-sandbox-run-change");
            row.Add(change);
            Slider load = new Slider("Load takes (s)", 0.1f, 3f);
            load.showInputField = true;
            load.value = model.LoadSeconds;
            load.tooltip = "How long the new scenes take to open in this run";
            load.RegisterValueChangedCallback(_change => model.LoadSeconds = _change.newValue);
            load.AddToClassList("docs-sandbox-run-load");
            row.Add(load);
            Toggle effect = new Toggle("Screen effect (fade)");
            effect.value = model.HasScreenEffect;
            effect.RegisterValueChangedCallback(_change => model.HasScreenEffect = _change.newValue);
            row.Add(effect);
            Button run = new Button(Run);
            run.text = "▶  Run";
            run.AddToClassList("docs-sandbox-run");
            row.Add(run);
            panel.Add(row);
            verdict.AddToClassList("docs-sandbox-verdict");
            panel.Add(verdict);
            return panel;
        }

        private VisualElement Output()
        {
            VisualElement output = new VisualElement();
            output.AddToClassList("docs-sandbox-output");
            screen.AddToClassList("docs-sandbox-screen");
            screenLabel.AddToClassList("docs-sandbox-screen-label");
            screen.Add(screenLabel);
            bar.AddToClassList("docs-sandbox-bar");
            barFill.AddToClassList("docs-sandbox-bar-fill");
            bar.Add(barFill);
            screen.Add(bar);
            cover.AddToClassList("docs-sandbox-cover");
            cover.pickingMode = PickingMode.Ignore;
            screen.Add(cover);
            output.Add(screen);
            console.AddToClassList("docs-sandbox-console");
            output.Add(console);
            ShowScreen(SandboxScreen.OldLevel, 0f);
            SetCovered(false, 0f);
            Print("", "Press Run to play the change here.", "", false);
            return output;
        }

        private void UpdateVerdict()
        {
            string text;
            SceneDocsSandboxRun.Build(model, out text);
            verdict.text = "This change: " + text;
        }

        private void Run()
        {
            StopRun();
            console.Clear();
            string text;
            List<SandboxEvent> events = SceneDocsSandboxRun.Build(model, out text);
            verdict.text = "This change: " + text;
            ShowScreen(SandboxScreen.OldLevel, 0f);
            SetCovered(false, 0f);
            double startedAt = EditorApplication.timeSinceStartup;
            int next = 0;
            float fade = model.HasScreenEffect ? SceneDocsSandboxRun.FadeSeconds : 0f;
            ticker = Root.schedule.Execute(() =>
            {
                float elapsed = (float)(EditorApplication.timeSinceStartup - startedAt);
                while (next < events.Count && events[next].Time <= elapsed)
                {
                    Apply(events[next], fade);
                    next++;
                }
                if (next >= events.Count)
                {
                    StopRun();
                }
            }).Every(16);
        }

        /// <summary>Shows the run as it stands a number of seconds in, at once: the screen and the log so far.</summary>
        public void ShowAt(float _seconds)
        {
            StopRun();
            console.Clear();
            string text;
            List<SandboxEvent> events = SceneDocsSandboxRun.Build(model, out text);
            verdict.text = "This change: " + text;
            ShowScreen(SandboxScreen.OldLevel, 0f);
            SetCovered(false, 0f);
            foreach (SandboxEvent item in events)
            {
                if (item.Time <= _seconds)
                {
                    Apply(item, 0f);
                }
            }
        }

        private void StopRun()
        {
            if (ticker != null)
            {
                ticker.Pause();
                ticker = null;
            }
        }

        private void Apply(SandboxEvent _event, float _fade)
        {
            if (_event.Screen.HasValue)
            {
                ShowScreen(_event.Screen.Value, _event.Progress < 0f ? 0f : _event.Progress);
            }
            else if (_event.Progress >= 0f)
            {
                barFill.style.width = Length.Percent(_event.Progress * 100f);
            }
            if (_event.Covered.HasValue)
            {
                SetCovered(_event.Covered.Value, _fade);
            }
            if (_event.Prints)
            {
                Print(_event.Time.ToString("0.00") + " s", _event.What, _event.Why, _event.IsWarning);
            }
        }

        private void ShowScreen(SandboxScreen _screen, float _progress)
        {
            screen.RemoveFromClassList("docs-sandbox-screen-old");
            screen.RemoveFromClassList("docs-sandbox-screen-loading");
            screen.RemoveFromClassList("docs-sandbox-screen-new");
            switch (_screen)
            {
                case SandboxScreen.OldLevel:
                    screen.AddToClassList("docs-sandbox-screen-old");
                    screenLabel.text = "Old level";
                    break;
                case SandboxScreen.LoadingScene:
                    screen.AddToClassList("docs-sandbox-screen-loading");
                    screenLabel.text = "Loading scene";
                    break;
                default:
                    screen.AddToClassList("docs-sandbox-screen-new");
                    screenLabel.text = "New level";
                    break;
            }
            bar.style.display = _screen == SandboxScreen.LoadingScene ? DisplayStyle.Flex : DisplayStyle.None;
            barFill.style.width = Length.Percent(_progress * 100f);
        }

        private void SetCovered(bool _covered, float _seconds)
        {
            cover.style.transitionDuration = new List<TimeValue> { new TimeValue(_seconds) };
            cover.style.opacity = _covered ? 1f : 0f;
        }

        private void Print(string _time, string _what, string _why, bool _isWarning)
        {
            VisualElement line = new VisualElement();
            line.AddToClassList("docs-sandbox-line");
            line.EnableInClassList("docs-sandbox-line-warning", _isWarning);
            Label time = new Label(_time);
            time.AddToClassList("docs-sandbox-line-time");
            DocsCodeBlock.ApplyMonoFont(time);
            line.Add(time);
            VisualElement text = new VisualElement();
            text.AddToClassList("docs-sandbox-line-text");
            Label what = new Label(_what);
            what.AddToClassList("docs-sandbox-line-what");
            text.Add(what);
            if (_why.Length > 0)
            {
                Label why = new Label(_why);
                why.AddToClassList("docs-sandbox-line-why");
                text.Add(why);
            }
            line.Add(text);
            console.Add(line);
            console.schedule.Execute(() => console.scrollOffset = new Vector2(0, float.MaxValue)).StartingIn(10);
        }
    }
}
