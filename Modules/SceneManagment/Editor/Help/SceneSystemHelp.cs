using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The help button ("?", Unity's _Help icon) every Scene System surface uses to open the scene documentation at the
    /// part about it: a UI Toolkit button (class docs-help-button), a help row for inspectors, and the IMGUI forms Unity
    /// forces for a tab bar (ShowButton) and a Project Settings title bar.
    /// </summary>
    public static class SceneSystemHelp
    {
        public const string ButtonClass = "docs-help-button";
        public const string LinkClass = "docs-learn-more";
        private const string HelpIcon = "_Help";
        private const float ButtonSize = 18f;

        /// <summary>A small flat "?" button that opens the topic.</summary>
        public static Button Button(string _topicId, string _tooltip)
        {
            Button button = new Button(() => SceneSystemDocs.Open(_topicId));
            button.AddToClassList(ButtonClass);
            button.tooltip = _tooltip;
            button.userData = _topicId;
            button.style.width = ButtonSize;
            button.style.height = ButtonSize;
            button.style.flexShrink = 0;
            button.style.marginLeft = 4;
            button.style.marginRight = 0;
            button.style.paddingLeft = 1;
            button.style.paddingRight = 1;
            button.style.paddingTop = 1;
            button.style.paddingBottom = 1;
            button.style.backgroundColor = new StyleColor(Color.clear);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            Image icon = new Image();
            icon.image = EditorGUIUtility.FindTexture(HelpIcon);
            icon.style.width = 16;
            icon.style.height = 16;
            button.Add(icon);
            return button;
        }

        /// <summary>A link-style "Learn more" button that opens the topic.</summary>
        public static Button LearnMore(string _topicId)
        {
            Button link = new Button(() => SceneSystemDocs.Open(_topicId));
            link.text = "Learn more";
            link.tooltip = "Opens the scene documentation about this";
            link.userData = _topicId;
            link.AddToClassList(LinkClass);
            link.AddToClassList(ButtonClass);
            link.style.backgroundColor = new StyleColor(Color.clear);
            link.style.borderTopWidth = 0;
            link.style.borderBottomWidth = 0;
            link.style.borderLeftWidth = 0;
            link.style.borderRightWidth = 0;
            link.style.color = SceneSystemUI.LinkBlue;
            link.style.alignSelf = Align.FlexStart;
            link.style.paddingLeft = 0;
            link.style.paddingRight = 0;
            link.style.marginLeft = 0;
            return link;
        }

        /// <summary>A row for the top of an inspector: a one-line summary and the help button.</summary>
        public static VisualElement HelpRow(string _summary, string _topicId)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-help-row");
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;
            Label summary = new Label(_summary);
            summary.style.flexGrow = 1;
            summary.style.flexShrink = 1;
            summary.style.whiteSpace = WhiteSpace.Normal;
            summary.style.color = SceneSystemUI.TextColour;
            summary.style.opacity = 0.75f;
            row.Add(summary);
            row.Add(Button(_topicId, "Opens the scene documentation about this"));
            return row;
        }

        /// <summary>The "?" in an EditorWindow's tab bar; call it from the window's ShowButton(Rect).</summary>
        public static void TabBarButton(Rect _position, string _topicId)
        {
            GUIContent content = new GUIContent(EditorGUIUtility.FindTexture(HelpIcon), "Open the scene documentation about this window");
            if (GUI.Button(_position, content, GUIStyle.none))
            {
                SceneSystemDocs.Open(_topicId);
            }
        }

        /// <summary>The "?" in a Project Settings or Preferences page's title bar (SettingsProvider.titleBarGuiHandler).</summary>
        public static void TitleBarButton(string _topicId)
        {
            GUIContent content = new GUIContent(EditorGUIUtility.FindTexture(HelpIcon), "Open the scene documentation about this page");
            if (GUILayout.Button(content, EditorStyles.label, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                SceneSystemDocs.Open(_topicId);
            }
        }
    }
}
