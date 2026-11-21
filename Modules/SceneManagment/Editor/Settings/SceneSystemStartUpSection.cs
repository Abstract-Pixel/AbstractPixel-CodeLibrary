using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Scene System page's Start-up section: the start scene group (required) and the optional launch scene that
    /// start-up reads when the game launches or Play is pressed.
    /// </summary>
    public static class SceneSystemStartUpSection
    {
        public const string Title = "Start-up";
        public const string Tooltip = "What the game opens when it launches or Play is pressed. No start-up object is needed in any scene.";
        public const string StartSceneGroupTooltip = "The scene group the game opens at launch. Play from a scene that is a scene group's main scene opens that group instead.";
        public const string LaunchSceneTooltip = "Optional scene, such as a splash, that Unity opens first. Choosing it puts it at build index 0. Start-up opens the start scene group behind it one frame later, then raises OnGameStarted; the launch scene closes itself. Opened on its own in the Editor, it opens nothing else.";
        public const string BuildIndexTooltip = "Unity opens build index 0 first in a build. The scene system keeps the launch scene there: choosing it here moves it, and a build puts it back if Build Settings were reordered.";
        public const string CreateLaunchSceneTooltip = "Makes a new scene with a camera and a light where you choose, sets it as the launch scene and puts it at build index 0.";
        public const string FallbackLabel = "If a scene change fails, go to";
        public const string FallbackTooltip = "The fallback scene group: where the scene system goes when a scene of the target group fails to load (missing from the build, broken address). Empty means the start scene group. A failed load is never tried again, and if the fallback fails too the screen is shown on whatever is open.";
        public const string MissingStartSceneGroup = "Start scene group missing: a build, and Play from a manager, shared or launch scene, open nothing. Pick the scene group the game opens at launch.";

        // What the fields held when the section was built or last saved. A save only happens when it changes: binding a
        // field, or the page rebuilding after a save, must never save again.
        private static string savedSignature;

        public static void Fill(VisualElement _card, SceneSystemSettings _settings)
        {
            _card.Clear();
            if (_settings == null)
            {
                _card.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                return;
            }

            _card.Add(StatusRow(_settings));

            SerializedObject settingsObject = new SerializedObject(_settings);
            VisualElement fields = new VisualElement();
            PropertyField startGroupField = new PropertyField(settingsObject.FindProperty("startSceneGroup"), string.Empty);
            startGroupField.tooltip = StartSceneGroupTooltip;
            fields.Add(LabelledRow("Start scene group", StartSceneGroupTooltip, startGroupField));
            PropertyField launchSceneField = SceneSystemUI.SceneField(settingsObject.FindProperty("launchScene"), string.Empty, LaunchSceneTooltip);
            VisualElement launchRow = LabelledRow("Launch scene (optional)", LaunchSceneTooltip, launchSceneField);
            launchRow.style.marginTop = 2;
            fields.Add(launchRow);
            PropertyField fallbackField = new PropertyField(settingsObject.FindProperty("fallbackSceneGroup"), string.Empty);
            fallbackField.tooltip = FallbackTooltip;
            VisualElement fallbackRow = LabelledRow(FallbackLabel, FallbackTooltip, fallbackField);
            fallbackRow.style.marginTop = 2;
            fields.Add(fallbackRow);
            fields.Add(FallbackHint(_settings));
            savedSignature = Signature(_settings);
            fallbackField.RegisterValueChangeCallback(_change => SaveIfChanged(_settings));
            startGroupField.RegisterValueChangeCallback(_change => SaveIfChanged(_settings));
            launchSceneField.RegisterValueChangeCallback(_change => SaveIfChanged(_settings));
            fields.Bind(settingsObject);
            _card.Add(fields);
            _card.Add(LaunchSceneRow(_settings));
        }

        // Under the fallback field: which group a failed change opens when the field is empty.
        private static VisualElement FallbackHint(SceneSystemSettings _settings)
        {
            string text = "Empty: a failed scene change goes to the start scene group.";
            if (_settings.FallbackSceneGroupSetting == null && _settings.StartSceneGroup != null)
            {
                text = "Empty: a failed scene change goes to the start scene group, '" + _settings.StartSceneGroup.name + "'.";
            }
            Label hint = SceneSystemUI.Hint(text);
            hint.style.marginLeft = 160;
            return hint;
        }

        // "Label        [field]" with the same label width as the Setup section's rows, so the fields line up.
        private static VisualElement LabelledRow(string _label, string _tooltip, VisualElement _field)
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.alignItems = Align.FlexStart;
            Label label = new Label(_label);
            label.tooltip = _tooltip;
            label.style.width = 160;
            label.style.flexShrink = 0;
            label.style.paddingTop = 2;
            row.Add(label);
            _field.style.flexGrow = 1;
            _field.style.flexShrink = 1;
            _field.style.minWidth = 60;
            row.Add(_field);
            return row;
        }

        // Under the fields: whether the launch scene opens first in a build, or a button to make one.
        private static VisualElement LaunchSceneRow(SceneSystemSettings _settings)
        {
            VisualElement row = new VisualElement();
            row.style.marginTop = 4;
            if (!_settings.HasLaunchScene)
            {
                Button create = SceneSystemUI.Button("Create a launch scene…", CreateLaunchSceneTooltip, () => CreateLaunchScene(_settings));
                create.style.alignSelf = Align.FlexStart;
                row.Add(create);
                return row;
            }
            if (LaunchSceneBuildOrder.IsInPlace(_settings))
            {
                row.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "'" + _settings.LaunchScene.SceneName + "' is build index 0, so a build opens it first", BuildIndexTooltip));
                return row;
            }
            string first = System.IO.Path.GetFileNameWithoutExtension(LaunchSceneBuildOrder.FirstScenePath());
            string message = "'" + _settings.LaunchScene.SceneName + "' is not build index 0: a build would open '" + first + "' first. The next build puts it back, or fix it now.";
            row.Add(SceneSystemUI.HelpBoxWithButton(message, HelpBoxMessageType.Warning, "Fix now", "Moves the launch scene to build index 0 in Build Settings.", () =>
            {
                LaunchSceneBuildOrder.EnsureFirst(_settings);
                SceneSystemSettingsAssets.NotifyChanged();
            }));
            return row;
        }

        private static void CreateLaunchScene(SceneSystemSettings _settings)
        {
            string path = LaunchSceneCreation.AskForPath(SceneSystemSettingsAssets.FolderOf(_settings));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            SceneAsset scene = LaunchSceneCreation.Create(path, out string error);
            if (scene == null)
            {
                EditorUtility.DisplayDialog("Create a launch scene", error, "OK");
                return;
            }
            LaunchSceneCreation.UseAsLaunchScene(_settings, scene);
            Debug.Log("[Scene system] Created the launch scene '" + scene.name + "' at " + path + ".", scene);
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
            LaunchSceneBuildOrder.EnsureFirst(_settings);
        }

        private static string Signature(SceneSystemSettings _settings)
        {
            string startGroupId = "none";
            if (_settings.StartSceneGroup != null)
            {
                startGroupId = _settings.StartSceneGroup.GetInstanceID().ToString();
            }
            string launchScene = "none";
            if (_settings.LaunchScene != null)
            {
                launchScene = _settings.LaunchScene.SceneGuid + "|" + _settings.LaunchScene.SceneName;
            }
            string fallbackId = "none";
            if (_settings.FallbackSceneGroupSetting != null)
            {
                fallbackId = _settings.FallbackSceneGroupSetting.GetInstanceID().ToString();
            }
            return startGroupId + "/" + launchScene + "/" + fallbackId;
        }

        private static VisualElement StatusRow(SceneSystemSettings _settings)
        {
            SceneGroup startGroup = _settings.StartSceneGroup;
            if (startGroup == null)
            {
                return SceneSystemUI.StatusRow(SceneSystemUI.Red, MissingStartSceneGroup, StartSceneGroupTooltip);
            }

            string text = "Opens '" + startGroup.name + "'";
            if (_settings.HasLaunchScene)
            {
                text += " behind '" + _settings.LaunchScene.SceneName + "'";
            }
            return SceneSystemUI.StatusRow(SceneSystemUI.Green, text, Tooltip);
        }
    }
}
