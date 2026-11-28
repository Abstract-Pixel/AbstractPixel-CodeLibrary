using AbstractPixel.Core.Editor.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Preferences &gt; Abstract Pixel &gt; Scenes: the two scene opening preferences.</summary>
    public static class SceneOpeningPreferencesProvider
    {
        public const string PagePath = "Preferences/Abstract Pixel/Scenes";
        public const string PlayNote = "Play always loads them, so your managers run.";

        // Both labels fit, so both checkboxes line up in one column, as on Unity's own preference pages.
        private const float LabelWidth = 320f;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            SettingsProvider provider = new SettingsProvider(PagePath, SettingsScope.User);
            provider.label = "Scenes";
            provider.keywords = new HashSet<string> { "scene", "scene group", "main scene", "scene set", "scene in no group", "open", "hierarchy", "docs", "sample" };
            provider.activateHandler = (_search, _root) => Build(_root);
            provider.titleBarGuiHandler = () => SceneSystemHelp.TitleBarButton(SceneSystemDocTopics.ScenesPreferences);
            return provider;
        }

        private static void Build(VisualElement _root)
        {
            VisualElement page = new VisualElement();
            page.style.paddingLeft = 10;
            page.style.paddingTop = 6;
            page.Add(SceneSystemUI.PageTitleWithHelp("Scenes", "Scene tools for you, in this project.", SceneSystemDocTopics.ScenesPreferences));
            page.Add(PreferenceToggle(SceneOpeningPreferences.MainSceneWithGroupLabel, SceneOpeningPreferences.MainSceneWithGroupTooltip, true));
            page.Add(PreferenceToggle(SceneOpeningPreferences.NoGroupSceneWithDefaultSetLabel, SceneOpeningPreferences.NoGroupSceneWithDefaultSetTooltip, false));
            page.Add(EditorUI.Hint(PlayNote));
            Toggle follow = new Toggle(SceneDocsFollowPreference.Label);
            follow.tooltip = SceneDocsFollowPreference.Tooltip;
            follow.labelElement.style.minWidth = LabelWidth;
            follow.labelElement.style.width = LabelWidth;
            follow.style.marginTop = 8;
            follow.SetValueWithoutNotify(SceneDocsFollowPreference.FollowSampleScenes);
            follow.RegisterValueChangedCallback(_change => SceneDocsFollowPreference.FollowSampleScenes = _change.newValue);
            page.Add(follow);
            _root.Add(page);
        }

        // A toggle that writes one preference and follows changes made elsewhere (the Tools menu, the scene picker).
        private static Toggle PreferenceToggle(string _label, string _tooltip, bool _isMainSceneWithGroup)
        {
            Toggle toggle = new Toggle(_label);
            toggle.tooltip = _tooltip;
            toggle.labelElement.style.minWidth = LabelWidth;
            toggle.labelElement.style.width = LabelWidth;
            toggle.SetValueWithoutNotify(ReadPreference(_isMainSceneWithGroup));
            toggle.RegisterValueChangedCallback(_change => WritePreference(_isMainSceneWithGroup, _change.newValue));
            System.Action refresh = () => toggle.SetValueWithoutNotify(ReadPreference(_isMainSceneWithGroup));
            toggle.RegisterCallback<AttachToPanelEvent>(_event => SceneOpeningPreferences.Changed += refresh);
            toggle.RegisterCallback<DetachFromPanelEvent>(_event => SceneOpeningPreferences.Changed -= refresh);
            return toggle;
        }

        private static bool ReadPreference(bool _isMainSceneWithGroup)
        {
            if (_isMainSceneWithGroup)
            {
                return SceneOpeningPreferences.MainSceneWithGroup;
            }
            return SceneOpeningPreferences.NoGroupSceneWithDefaultSet;
        }

        private static void WritePreference(bool _isMainSceneWithGroup, bool _value)
        {
            if (_isMainSceneWithGroup)
            {
                SceneOpeningPreferences.MainSceneWithGroup = _value;
                return;
            }
            SceneOpeningPreferences.NoGroupSceneWithDefaultSet = _value;
        }
    }
}
