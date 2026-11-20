using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// The scene field's status box in UI Toolkit: Unity's help box with a coloured square, one status line and, for a
    /// scene that is Not in build, one fix button. It is the one place a scene's status is shown.
    /// </summary>
    public class SceneStatusBox : VisualElement
    {
        private const string FixTooltipAddToBuild = "Adds this scene to Build Settings (or switches it on there) and removes its Addressables entry, so it loads from one place.";
        private const string FixTooltipMakeAddressable = "Gives this scene an Addressables entry and removes it from Build Settings, so it loads from one place.";

        private readonly VisualElement square;
        private readonly Label message;
        private readonly Button fixButton;
        private Action fixClicked;

        public SceneStatusBox()
        {
            AddToClassList(HelpBox.ussClassName);
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.minHeight = 30;
            style.marginTop = 6;
            style.marginBottom = 6;
            style.paddingLeft = 7;

            square = MakeSquare();
            Add(square);

            message = new Label();
            message.enableRichText = true;
            message.style.flexGrow = 1;
            message.style.flexShrink = 1;
            message.style.whiteSpace = WhiteSpace.Normal;
            message.style.marginLeft = 7;
            Add(message);

            fixButton = new Button(OnFixClicked);
            fixButton.style.flexShrink = 0;
            Add(fixButton);
        }

        /// <summary>Shows a report; &lt;paramref name="_fixClicked"/&gt; runs when its fix button is clicked.</summary>
        public void Show(SceneStatusReport _report, Action _fixClicked)
        {
            fixClicked = _fixClicked;
            square.style.backgroundColor = SceneStatusColours.For(_report.Status);
            message.text = _report.Message;

            bool showFix = _report.HasFix;
            fixButton.style.display = showFix ? DisplayStyle.Flex : DisplayStyle.None;
            fixButton.text = _report.FixLabel;
            fixButton.tooltip = FixTooltip(_report.Fix);
        }

        private void OnFixClicked()
        {
            if (fixClicked == null)
            {
                return;
            }
            fixClicked();
        }

        private static VisualElement MakeSquare()
        {
            VisualElement element = new VisualElement();
            element.style.width = 16;
            element.style.height = 16;
            element.style.flexShrink = 0;
            Color outline = SceneStatusColours.Outline;
            element.style.borderTopWidth = 1;
            element.style.borderBottomWidth = 1;
            element.style.borderLeftWidth = 1;
            element.style.borderRightWidth = 1;
            element.style.borderTopColor = outline;
            element.style.borderBottomColor = outline;
            element.style.borderLeftColor = outline;
            element.style.borderRightColor = outline;
            return element;
        }

        private static string FixTooltip(SceneFix _fix)
        {
            if (_fix == SceneFix.MakeAddressable)
            {
                return FixTooltipMakeAddressable;
            }
            return FixTooltipAddToBuild;
        }
    }
}
