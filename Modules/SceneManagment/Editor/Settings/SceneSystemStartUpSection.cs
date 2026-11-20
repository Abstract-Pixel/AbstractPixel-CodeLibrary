using UnityEditor;
using UnityEditor.UIElements;
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
        public const string LaunchSceneTooltip = "Optional scene, such as a splash, that Unity opens first (build index 0). Start-up opens the start scene group behind it one frame later, then raises OnGameStarted; the launch scene closes itself. Opened on its own in the Editor, it opens nothing else.";
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
            PropertyField startGroupField = new PropertyField(settingsObject.FindProperty("startSceneGroup"), "Start scene group");
            startGroupField.tooltip = StartSceneGroupTooltip;
            fields.Add(startGroupField);
            PropertyField launchSceneField = SceneSystemUI.SceneField(settingsObject.FindProperty("launchScene"), "Launch scene (optional)", LaunchSceneTooltip);
            launchSceneField.style.marginTop = 2;
            fields.Add(launchSceneField);
            savedSignature = Signature(_settings);
            startGroupField.RegisterValueChangeCallback(_change => SaveIfChanged(_settings));
            launchSceneField.RegisterValueChangeCallback(_change => SaveIfChanged(_settings));
            fields.Bind(settingsObject);
            _card.Add(fields);
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
            return startGroupId + "/" + launchScene;
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
