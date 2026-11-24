using AbstractPixel.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The scene field's real status box, with one button per state, so a reader can see each one.</summary>
    /// <remarks>
    /// - It draws Core's SceneStatusBox with reports from SceneStatusRules, the same code every scene field uses.
    /// - Its fix button does nothing here: the demo has no scene to fix.
    /// - Sample is the scene picker's grey state; the scene field itself never shows it.
    /// </remarks>
    public static class SceneDocsStatusDemo
    {
        private const string SceneName = "Level 1";
        private const string ScenePath = "Assets/Scenes/Level 1.unity";
        private static readonly Color SampleGrey = new Color(0.55f, 0.55f, 0.55f);

        public static VisualElement Render()
        {
            VisualElement demo = new VisualElement();
            demo.AddToClassList("docs-status-demo");

            VisualElement buttons = new VisualElement();
            buttons.AddToClassList("docs-status-demo-buttons");
            demo.Add(buttons);

            SceneStatusBox box = new SceneStatusBox();
            demo.Add(box);
            Label meaning = new Label();
            meaning.AddToClassList("docs-text");
            meaning.AddToClassList("docs-status-demo-meaning");
            demo.Add(meaning);

            AddState(buttons, box, meaning, "In build", SceneStatus.InBuild, ScenePlace.BuildSettings, "The scene is in Build Settings, switched on, so a build can open it.");
            AddState(buttons, box, meaning, "Addressable", SceneStatus.Addressable, ScenePlace.BuildSettings, "The scene has an Addressables entry and loads through the Addressables loader.");
            AddState(buttons, box, meaning, "Not in build", SceneStatus.NotInBuild, ScenePlace.BuildSettings, "A problem: neither place has the scene, so it can't load. The fix puts it where new scenes go, here Build Settings.");
            AddState(buttons, box, meaning, "Not in build (Addressables)", SceneStatus.NotInBuild, ScenePlace.Addressables, "The same problem in a project whose new scenes go to Addressables: the fix makes it Addressable.");
            AddState(buttons, box, meaning, "Missing", SceneStatus.Missing, ScenePlace.BuildSettings, "A problem: the scene asset is gone. Pick the scene again.");
            AddState(buttons, box, meaning, "No scene", SceneStatus.NoScene, ScenePlace.BuildSettings, "The field is empty.");
            AddSample(buttons, box, meaning);

            ShowState(box, meaning, SceneStatus.InBuild, ScenePlace.BuildSettings, "The scene is in Build Settings, switched on, so a build can open it.");
            return demo;
        }

        private static void AddState(VisualElement _buttons, SceneStatusBox _box, Label _meaning, string _label, SceneStatus _status, ScenePlace _place, string _explanation)
        {
            Button button = new Button(() => ShowState(_box, _meaning, _status, _place, _explanation));
            button.text = _label;
            button.AddToClassList("docs-status-demo-button");
            _buttons.Add(button);
        }

        private static void ShowState(SceneStatusBox _box, Label _meaning, SceneStatus _status, ScenePlace _place, string _explanation)
        {
            bool addressablesInstalled = true;
            SceneFix fix = SceneStatusRules.FixFor(_status, _place, addressablesInstalled);
            string message = SceneStatusRules.Message(_status, SceneName, ScenePath, addressablesInstalled);
            SceneStatusReport report = new SceneStatusReport(_status, fix, SceneName, ScenePath, string.Empty, message);
            _box.Show(report, null);
            _meaning.text = _explanation;
        }

        // The scene picker's grey sample state, drawn in the same box so it can be compared with the others.
        private static void AddSample(VisualElement _buttons, SceneStatusBox _box, Label _meaning)
        {
            Button button = new Button(() =>
            {
                ShowState(_box, _meaning, SceneStatus.InBuild, ScenePlace.BuildSettings, "Shown in the scene picker, not the scene field: a sample scene is grey, because it never goes into a build.");
                _box[0].style.backgroundColor = SampleGrey;
                _box.Q<Label>().text = "Sample: never goes into a build";
            });
            button.text = "Sample (scene picker)";
            button.AddToClassList("docs-status-demo-button");
            _buttons.Add(button);
        }
    }
}
