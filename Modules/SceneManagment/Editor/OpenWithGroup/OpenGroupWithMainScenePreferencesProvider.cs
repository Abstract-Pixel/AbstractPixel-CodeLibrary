using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Preferences &gt; Abstract Pixel &gt; Scenes: the on/off toggle for "Open the group with its main scene".</summary>
    public static class OpenGroupWithMainScenePreferencesProvider
    {
        public const string PagePath = "Preferences/Abstract Pixel/Scenes";
        public const string ToggleLabel = "Open the group with its main scene";
        public const string ToggleTooltip = "When you open a scene on its own, also open the scenes Play would load with it, so the Hierarchy shows what Play loads. A scene in no scene group opens the default scene set's scenes; the launch scene opens nothing. When this is off, Play still loads the missing scenes. For you only, in this project.";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.User);
            provider.label = "Scenes";
            provider.keywords = new HashSet<string> { "scene", "scene group", "main scene", "open", "hierarchy", "start-up" };
            provider.activateHandler = (_search, _root) => Build(_root);
            return provider;
        }

        private static void Build(VisualElement _root)
        {
            VisualElement page = new VisualElement();
            page.style.paddingLeft = 10;
            page.style.paddingTop = 6;
            page.Add(SceneSystemUI.PageTitle("Scenes", "Scene tools for you, in this project."));
            Toggle toggle = new Toggle(ToggleLabel);
            toggle.tooltip = ToggleTooltip;
            toggle.value = OpenGroupWithMainScenePreference.IsOn;
            toggle.RegisterValueChangedCallback(_change => OpenGroupWithMainScenePreference.IsOn = _change.newValue);
            page.Add(toggle);
            _root.Add(page);
        }
    }
}
