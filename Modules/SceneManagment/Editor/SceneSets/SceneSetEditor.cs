using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>A scene set's Inspector: the same fields as on the Project Settings page, with how many scene groups use it.</summary>
    [CustomEditor(typeof(SceneSet))]
    public class SceneSetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SceneSet set = (SceneSet)target;
            int users = SceneSetSync.GroupsUsing(set).Count;
            Label usage = SceneSystemUI.Hint("Used by " + users + " scene group(s). Changes fill each of them, except the parts a group overrides. All scene sets are listed in Project Settings > Abstract Pixel > Scenes.");
            usage.style.marginBottom = 4;
            root.Add(usage);
            root.Add(SceneSystemUI.SceneSetFields(serializedObject));
            return root;
        }
    }
}
