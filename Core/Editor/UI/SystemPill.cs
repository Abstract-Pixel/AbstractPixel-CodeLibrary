using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor.UI
{
    /// <summary>
    /// The rounded blue "system" label under the logo of a setup window (Game Management, Scene System), with a small gear.
    /// One look for every system, so a developer sees at once which setup they are in.
    /// </summary>
    public static class SystemPill
    {
        private static readonly Color Fill = new Color(64f / 255f, 140f / 255f, 212f / 255f, 0.16f);
        private static readonly Color Outline = new Color(100f / 255f, 170f / 255f, 230f / 255f, 0.65f);
        private static readonly Color Text = new Color(165f / 255f, 205f / 255f, 245f / 255f, 1f);

        /// <summary>The pill, centred in its parent. Large is for a welcome page.</summary>
        public static VisualElement Create(string _systemName, bool _large)
        {
            VisualElement pill = new VisualElement();
            pill.name = "systemPill";
            pill.style.flexDirection = FlexDirection.Row;
            pill.style.alignItems = Align.Center;
            pill.style.alignSelf = Align.Center;
            pill.style.flexShrink = 0;
            pill.style.paddingTop = _large ? 5 : 3;
            pill.style.paddingBottom = _large ? 5 : 3;
            pill.style.paddingLeft = _large ? 14 : 8;
            pill.style.paddingRight = _large ? 20 : 12;
            pill.style.backgroundColor = Fill;
            EditorUI.SetRadius(pill, _large ? 16 : 11);
            EditorUI.SetBorder(pill, Outline, 1);

            Image icon = new Image();
            icon.image = SystemPillIcon.Get();
            icon.scaleMode = ScaleMode.ScaleToFit;
            icon.tintColor = Text;
            icon.style.width = _large ? 18 : 13;
            icon.style.height = _large ? 18 : 13;
            icon.style.marginRight = _large ? 8 : 5;
            pill.Add(icon);

            Label label = new Label(_systemName);
            label.style.color = Text;
            label.style.fontSize = _large ? 16 : 10;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            pill.Add(label);
            return pill;
        }
    }
}
