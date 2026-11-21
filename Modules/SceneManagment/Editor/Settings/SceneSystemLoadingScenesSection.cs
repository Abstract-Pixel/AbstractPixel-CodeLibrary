using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Scene System page's Loading scenes section: the project's loading scenes, the default one and the minimum
    /// time a loading scene stays on screen. Each scene group says when it shows one (Show the loading scene).
    /// </summary>
    public static class SceneSystemLoadingScenesSection
    {
        public const string Title = "Loading scenes";
        public const string Tooltip = "Scenes shown while the old place closes and the new one opens. A scene group says when it shows one and which; the default is preselected.";
        public const string ListTooltip = "The loading scenes of this project. Each is one scene, never a scene group.";
        public const string DefaultTooltip = "The loading scene a scene group shows when it picked none.";
        public const string MinimumTooltip = "Minimum time on screen, in unscaled seconds, so a fast load does not flash the loading scene. It counts from Show screen starting (or from the cut when there is no screen effect).";
        private const string NoDefault = "None";

        // What the section held when it was built or last saved; a save only happens when it changes.
        private static string savedSignature;

        public static void Fill(VisualElement _card, SceneSystemSettings _settings)
        {
            _card.Clear();
            if (_settings == null)
            {
                _card.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                return;
            }

            SerializedObject settingsObject = new SerializedObject(_settings);
            VisualElement fields = new VisualElement();
            fields.Add(SceneSystemUI.SceneList(settingsObject, "loadingScenes", "Loading scenes", ListTooltip));
            fields.Add(DefaultPopup(_settings));
            FloatField minimum = new FloatField("Minimum time on screen (s)");
            minimum.tooltip = MinimumTooltip;
            minimum.style.marginTop = 4;
            minimum.BindProperty(settingsObject.FindProperty("loadingSceneMinimumSeconds"));
            fields.Add(minimum);
            savedSignature = Signature(_settings);
            fields.TrackSerializedObjectValue(settingsObject, _changed => SaveIfChanged(_settings));
            fields.Bind(settingsObject);
            _card.Add(fields);
            if (_settings.LoadingScenes.Count == 0)
            {
                _card.Add(SceneSystemUI.Hint("No loading scene yet: the setup clones a ready-made one, or add your own with +."));
            }
        }

        private static PopupField<string> DefaultPopup(SceneSystemSettings _settings)
        {
            List<string> choices = new List<string> { NoDefault };
            foreach (SceneReference scene in _settings.LoadingScenes)
            {
                bool hasName = scene != null && !string.IsNullOrEmpty(scene.SceneName);
                if (hasName && !choices.Contains(scene.SceneName))
                {
                    choices.Add(scene.SceneName);
                }
            }

            string current = NoDefault;
            SceneReference currentDefault = _settings.DefaultLoadingScene;
            if (currentDefault != null && choices.Contains(currentDefault.SceneName))
            {
                current = currentDefault.SceneName;
            }

            PopupField<string> popup = new PopupField<string>("Default loading scene", choices, current);
            popup.tooltip = DefaultTooltip;
            popup.style.marginTop = 4;
            popup.RegisterValueChangedCallback(_change => PickDefault(_settings, _change.newValue));
            return popup;
        }

        private static void PickDefault(SceneSystemSettings _settings, string _sceneName)
        {
            Undo.RecordObject(_settings, "Default loading scene");
            _settings.DefaultLoadingScene = new SceneReference(string.Empty);
            foreach (SceneReference scene in _settings.LoadingScenes)
            {
                if (scene != null && scene.SceneName == _sceneName)
                {
                    _settings.DefaultLoadingScene = new SceneReference(scene.SceneName, scene.SceneGuid, scene.ScenePath);
                }
            }
            SceneSystemSettingsAssets.Save(_settings);
        }

        private static void SaveIfChanged(SceneSystemSettings _settings)
        {
            string signature = Signature(_settings);
            if (signature == savedSignature)
            {
                return;
            }
            savedSignature = signature;
            SceneSystemSettingsAssets.Save(_settings);
        }

        private static string Signature(SceneSystemSettings _settings)
        {
            List<string> parts = new List<string>();
            foreach (SceneReference scene in _settings.LoadingScenes)
            {
                parts.Add(scene == null ? "-" : scene.SceneGuid + "|" + scene.SceneName);
            }
            parts.Add(_settings.LoadingSceneMinimumSeconds.ToString("R"));
            return string.Join("/", parts);
        }
    }
}
