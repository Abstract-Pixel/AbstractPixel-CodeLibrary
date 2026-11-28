using AbstractPixel.Core.Editor.UI;
using AbstractPixel.EditorModules.Documentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The help button ("?") every Scene System surface uses to open the scene documentation at the part about it:
    /// the documentation engine's buttons, wired to the scene documentation and its colours.
    /// </summary>
    public static class SceneSystemHelp
    {
        public const string ButtonClass = DocsHelpButtons.ButtonClass;
        public const string LinkClass = DocsHelpButtons.LinkClass;

        private static readonly DocsHelpButtons Buttons = new DocsHelpButtons(SceneSystemDocs.Open, "scene documentation");

        public static Button Button(string _topicId, string _tooltip)
        {
            return Buttons.Button(_topicId, _tooltip);
        }

        public static Button LearnMore(string _topicId)
        {
            return Buttons.LearnMore(_topicId, EditorUI.LinkBlue);
        }

        public static VisualElement HelpRow(string _summary, string _topicId)
        {
            return Buttons.HelpRow(_summary, _topicId, EditorUI.TextColour);
        }

        public static void TabBarButton(Rect _position, string _topicId)
        {
            Buttons.TabBarButton(_position, _topicId);
        }

        public static void TitleBarButton(string _topicId)
        {
            Buttons.TitleBarButton(_topicId);
        }
    }
}
