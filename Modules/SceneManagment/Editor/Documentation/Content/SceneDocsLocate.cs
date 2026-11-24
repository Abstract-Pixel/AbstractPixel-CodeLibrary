using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Where a component lives, drawn like a Unity toolbar: the scene, the object and the component with their usual
    /// icons, then "Select". Select opens the scene if it is closed and selects the object, so its Inspector shows the
    /// fields the page describes. Written as <c>&lt;!-- locate "scene path" "Object/Path" "Component" --&gt;</c>.
    /// </summary>
    public static class SceneDocsLocate
    {
        public static VisualElement Render(string _scenePath, string _objectPath, string _component)
        {
            Toolbar bar = new Toolbar();
            bar.AddToClassList("docs-locate");
            Label lead = new Label("Find it in");
            lead.AddToClassList("docs-locate-lead");
            bar.Add(lead);
            bar.Add(Item("SceneAsset Icon", Path.GetFileNameWithoutExtension(_scenePath)));
            bar.Add(Separator());
            bar.Add(Item("GameObject Icon", _objectPath));
            bar.Add(Separator());
            bar.Add(Item("cs Script Icon", _component));
            VisualElement space = new VisualElement();
            space.style.flexGrow = 1;
            bar.Add(space);
            ToolbarButton select = new ToolbarButton(() => Select(_scenePath, _objectPath));
            select.text = "Select in Hierarchy";
            select.tooltip = "Opens " + Path.GetFileNameWithoutExtension(_scenePath) + " if it is closed and selects " + _objectPath + ", so the Inspector shows its " + _component;
            select.AddToClassList("docs-locate-select");
            bar.Add(select);
            return bar;
        }

        private static VisualElement Item(string _icon, string _text)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("docs-locate-item");
            Image icon = new Image();
            icon.image = EditorGUIUtility.IconContent(_icon).image;
            icon.AddToClassList("docs-locate-icon");
            item.Add(icon);
            Label label = new Label(_text);
            label.AddToClassList("docs-locate-text");
            item.Add(label);
            return item;
        }

        private static Label Separator()
        {
            Label separator = new Label("›");
            separator.AddToClassList("docs-locate-separator");
            return separator;
        }

        private static void Select(string _scenePath, string _objectPath)
        {
            SceneDocsSampleFollower.IsDocsOpeningScenes = true;
            try
            {
                SceneDocsActions.SelectInScene(_scenePath, _objectPath);
            }
            finally
            {
                SceneDocsSampleFollower.IsDocsOpeningScenes = false;
            }
        }
    }
}
