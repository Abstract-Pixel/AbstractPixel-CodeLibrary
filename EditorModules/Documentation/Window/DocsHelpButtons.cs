using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// The help buttons that open a documentation topic: a UI Toolkit "?" button (class docs-help-button), a "Learn more"
    /// link, a help row for inspectors, and the IMGUI forms Unity forces for a tab bar and a Project Settings title bar.
    /// A module makes one with its own way to open a topic.
    /// </summary>
    public class DocsHelpButtons
    {
        public const string ButtonClass = "docs-help-button";
        public const string LinkClass = "docs-learn-more";
        private const string HelpIcon = "_Help";
        private const float ButtonSize = 18f;

        private readonly Action<string> open;
        private readonly string documentationName;

        /// <summary>_documentationName finishes the tooltips: "Opens the {name} about this".</summary>
        public DocsHelpButtons(Action<string> _open, string _documentationName)
        {
            open = _open;
            documentationName = _documentationName;
        }

        /// <summary>A small flat "?" button that opens the topic.</summary>
        public Button Button(string _topicId, string _tooltip)
        {
            Button button = new Button(() => open(_topicId));
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
            ClearFrame(button);
            Image icon = new Image();
            icon.image = EditorGUIUtility.FindTexture(HelpIcon);
            icon.style.width = 16;
            icon.style.height = 16;
            button.Add(icon);
            return button;
        }

        /// <summary>A link-style "Learn more" button that opens the topic.</summary>
        public Button LearnMore(string _topicId, Color _linkColour)
        {
            Button link = new Button(() => open(_topicId));
            link.text = "Learn more";
            link.tooltip = "Opens the " + documentationName + " about this";
            link.userData = _topicId;
            link.AddToClassList(LinkClass);
            link.AddToClassList(ButtonClass);
            ClearFrame(link);
            link.style.color = _linkColour;
            link.style.alignSelf = Align.FlexStart;
            link.style.paddingLeft = 0;
            link.style.paddingRight = 0;
            link.style.marginLeft = 0;
            return link;
        }

        /// <summary>A row for the top of an inspector: a one-line summary and the help button.</summary>
        public VisualElement HelpRow(string _summary, string _topicId, Color _textColour)
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
            summary.style.color = _textColour;
            summary.style.opacity = 0.75f;
            row.Add(summary);
            row.Add(Button(_topicId, "Opens the " + documentationName + " about this"));
            return row;
        }

        /// <summary>The "?" in an EditorWindow's tab bar; call it from the window's ShowButton(Rect).</summary>
        public void TabBarButton(Rect _position, string _topicId)
        {
            GUIContent content = new GUIContent(EditorGUIUtility.FindTexture(HelpIcon), "Open the " + documentationName + " about this window");
            if (GUI.Button(_position, content, GUIStyle.none))
            {
                open(_topicId);
            }
        }

        /// <summary>The "?" in a Project Settings or Preferences page's title bar (SettingsProvider.titleBarGuiHandler).</summary>
        public void TitleBarButton(string _topicId)
        {
            GUIContent content = new GUIContent(EditorGUIUtility.FindTexture(HelpIcon), "Open the " + documentationName + " about this page");
            if (GUILayout.Button(content, EditorStyles.label, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                open(_topicId);
            }
        }

        private static void ClearFrame(Button _button)
        {
            _button.style.backgroundColor = new StyleColor(Color.clear);
            _button.style.borderTopWidth = 0;
            _button.style.borderBottomWidth = 0;
            _button.style.borderLeftWidth = 0;
            _button.style.borderRightWidth = 0;
        }
    }
}
