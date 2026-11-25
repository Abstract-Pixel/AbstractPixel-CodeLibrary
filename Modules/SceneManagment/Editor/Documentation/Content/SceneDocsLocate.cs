using AbstractPixel.EditorModules.Documentation;
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
            string sceneName = Path.GetFileNameWithoutExtension(_scenePath);
            // Each step of the path is a button: the scene shows in the Project window, the object and its component
            // in the Hierarchy and Inspector, with the same select and ping the Select in Hierarchy button uses.
            bar.Add(Item("SceneAsset Icon", sceneName, "Shows the scene " + sceneName + " in the Project window", () => SceneDocsActions.SelectAsset(_scenePath, true)));
            bar.Add(Separator());
            bar.Add(Item("GameObject Icon", _objectPath, "Opens " + sceneName + " if it is closed and selects " + _objectPath + " in the Hierarchy", () => Select(_scenePath, _objectPath)));
            bar.Add(Separator());
            bar.Add(Item("cs Script Icon", _component, "Selects " + _objectPath + ", so the Inspector shows its " + _component, () => Select(_scenePath, _objectPath)));
            VisualElement space = new VisualElement();
            space.style.flexGrow = 1;
            bar.Add(space);
            // A divider, so the last item reads as a button of its own.
            VisualElement divider = new VisualElement();
            divider.AddToClassList("docs-locate-divider");
            bar.Add(divider);
            ToolbarButton select = new ToolbarButton(() => Select(_scenePath, _objectPath));
            select.text = "Select in Hierarchy";
            select.tooltip = "Opens " + Path.GetFileNameWithoutExtension(_scenePath) + " if it is closed and selects " + _objectPath + ", so the Inspector shows its " + _component;
            select.AddToClassList("docs-locate-select");
            bar.Add(select);
            return bar;
        }

        private static VisualElement Item(string _icon, string _text, string _tooltip, System.Action _clicked)
        {
            ToolbarButton item = new ToolbarButton(() => RunFromDocs(_clicked));
            item.AddToClassList("docs-locate-item");
            item.tooltip = _tooltip;
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

        private static void RunFromDocs(System.Action _action)
        {
            SceneDocsSampleFollower.IsDocsOpeningScenes = true;
            try
            {
                _action();
            }
            finally
            {
                SceneDocsSampleFollower.IsDocsOpeningScenes = false;
            }
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
