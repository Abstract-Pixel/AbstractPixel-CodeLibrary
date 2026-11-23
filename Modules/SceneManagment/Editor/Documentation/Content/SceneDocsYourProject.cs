using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// "Your project": live, read-only facts from this project's scene system settings, with a button to the Project
    /// Settings section that changes them. Never edits anything.
    /// </summary>
    public static class SceneDocsYourProject
    {
        public static VisualElement Render(string _which)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-your-project");
            Label title = new Label("Your project");
            title.AddToClassList("docs-your-project-title");
            box.Add(title);
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings == null)
            {
                box.Add(Fact("Scene system settings", "Not set up yet"));
                box.Add(DocsChip.Create(DocsIcons.GlyphSettings, "Set up...", "Opens the setup window", () => SceneSystemOnboardingWindow.Open()));
                return box;
            }
            box.Add(Fact("Scene system settings", AssetDatabase.GetAssetPath(settings)));
            box.Add(Fact("Start scene group", NameOf(settings.StartSceneGroup)));
            if (_which == "start-up")
            {
                string launch = settings.HasLaunchScene ? settings.LaunchScene.SceneName : "None";
                box.Add(Fact("Launch scene", launch));
                box.Add(Fact("Fallback scene group", NameOf(settings.FallbackSceneGroup)));
                box.Add(DocsChip.Create(DocsIcons.GlyphSettings, "Open Start-up...", "Opens Project Settings > Abstract Pixel > Scene System at Start-up", () => SceneSystemSettingsProvider.OpenAt(SceneSystemSettingsProvider.StartUpSection)));
                return box;
            }
            box.Add(Fact("Default scene set", NameOf(settings.DefaultSceneSet)));
            box.Add(Fact("Scene sets", settings.SceneSets.Count.ToString()));
            box.Add(Fact("Scene groups", settings.SceneGroups.Count.ToString()));
            box.Add(DocsChip.Create(DocsIcons.GlyphSettings, "Open Scene System...", "Opens Project Settings > Abstract Pixel > Scene System", () => SceneSystemSettingsProvider.Open()));
            return box;
        }

        private static string NameOf(UnityEngine.Object _asset)
        {
            return _asset != null ? _asset.name : "None";
        }

        private static VisualElement Fact(string _label, string _value)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-fact");
            Label label = new Label(_label);
            label.AddToClassList("docs-fact-label");
            row.Add(label);
            Label value = new Label(_value);
            value.AddToClassList("docs-fact-value");
            value.selection.isSelectable = true;
            row.Add(value);
            return row;
        }
    }
}
