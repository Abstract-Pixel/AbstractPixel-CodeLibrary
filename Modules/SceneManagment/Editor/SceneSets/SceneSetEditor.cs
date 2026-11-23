using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>A scene set's Inspector: Open Scene System, how many scene groups use it, and the same fields as on the Scene System page.</summary>
    [CustomEditor(typeof(SceneSet))]
    public class SceneSetEditor : UnityEditor.Editor
    {
        private const string OpenSceneSystemTooltip = "Opens Project Settings > Abstract Pixel > Scene System at Scene sets, which lists every scene set and the default one.";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SceneSet set = (SceneSet)target;

            root.Add(SceneSystemHelp.HelpRow("The manager scenes, shared scenes and Reset shared scenes for every scene group using this set.", SceneSystemDocTopics.SceneSetInspector));
            VisualElement buttons = new VisualElement();
            buttons.style.marginBottom = 6;
            buttons.Add(SceneSystemUI.PrimaryButton("Open Scene System", OpenSceneSystemTooltip, () => SceneSystemSettingsProvider.OpenAt(SceneSystemSettingsProvider.SceneSetsSection)));
            root.Add(buttons);

            int users = SceneSetSync.GroupsUsing(set).Count;
            Label usage = SceneSystemUI.Hint("Used by " + users + " scene group(s).");
            usage.tooltip = "Changes fill each of them, except the parts a group overrides.";
            usage.style.marginBottom = 4;
            root.Add(usage);
            root.Add(SceneSystemUI.SceneSetFields(serializedObject));
            return root;
        }
    }
}
