using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor.UI
{
    /// <summary>
    /// The look Abstract Pixel editor pages share: section headers (explanations in their tooltips), cards, status rows
    /// with a coloured dot (green fine, yellow needs attention, red broken), help boxes with a fix button and setup pages.
    /// </summary>
    public static class EditorUI
    {
        public static readonly Color Green = new Color(0.30f, 0.75f, 0.36f);

        public static readonly Color Yellow = new Color(0.96f, 0.74f, 0.19f);

        public static readonly Color Red = new Color(0.89f, 0.29f, 0.24f);

        /// <summary>Unity's default label text colour in the current skin.</summary>
        public static Color TextColour
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.769f, 0.769f, 0.769f) : new Color(0.035f, 0.035f, 0.035f); }
        }

        /// <summary>Unity's link blue, for a name that reacts to the pointer (a scene set's name on hover).</summary>
        public static Color LinkBlue
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.30f, 0.49f, 1f) : new Color(0.10f, 0.33f, 0.80f); }
        }

        public static readonly Color OverrideBlue = new Color(0.17f, 0.52f, 0.92f);

        private const string LogoResource = "AbstractPixelLogo";

        public static Color MutedText
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.30f, 0.30f, 0.30f); }
        }

        public static Color CardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.87f, 0.87f, 0.87f); }
        }

        public static Color CardBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.66f, 0.66f, 0.66f); }
        }

        public static Color InnerCardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.235f, 0.235f, 0.235f) : new Color(0.92f, 0.92f, 0.92f); }
        }

        public static Color HeaderHoverText
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.62f, 0.78f, 0.98f) : new Color(0.10f, 0.33f, 0.66f); }
        }

        private static Color PrimaryButtonBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.27f, 0.35f, 0.45f) : new Color(0.79f, 0.86f, 0.95f); }
        }

        private static Color PrimaryButtonHover
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.31f, 0.40f, 0.51f) : new Color(0.85f, 0.91f, 0.98f); }
        }

        /// <summary>A page title. What the page is for goes in its tooltip.</summary>
        public static Label PageTitle(string _title, string _tooltip)
        {
            Label title = new Label(_title);
            title.tooltip = _tooltip;
            title.style.fontSize = 19;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 2;
            title.style.alignSelf = Align.FlexStart;
            return title;
        }

        /// <summary>Adds a section (header, card) to the parent and returns the card to fill. The explanation is the header's tooltip.</summary>
        public static VisualElement Section(VisualElement _parent, string _title, string _tooltip)
        {
            return Section(_parent, _title, _tooltip, out VisualElement _);
        }

        /// <summary>Same, and hands back the header row, so a section can put a summary or a button next to its title.</summary>
        public static VisualElement Section(VisualElement _parent, string _title, string _tooltip, out VisualElement _headerRow)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = 14;
            VisualElement headerRow = Row();
            headerRow.style.marginBottom = 5;
            headerRow.style.minHeight = 20;
            Label header = new Label(_title);
            header.tooltip = _tooltip;
            header.style.fontSize = 14;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            AddHoverTint(header);
            headerRow.Add(header);
            headerRow.Add(Spacer());
            section.Add(headerRow);
            VisualElement card = Card();
            section.Add(card);
            _parent.Add(section);
            _headerRow = headerRow;
            return card;
        }

        /// <summary>A header's slight colour change while the pointer is over it, so its tooltip is easy to find.</summary>
        public static void AddHoverTint(VisualElement _header)
        {
            _header.RegisterCallback<PointerEnterEvent>(_event => _header.style.color = HeaderHoverText);
            _header.RegisterCallback<PointerLeaveEvent>(_event => _header.style.color = StyleKeyword.Null);
            _header.RegisterCallback<DetachFromPanelEvent>(_event => _header.style.color = StyleKeyword.Null);
        }

        public static VisualElement Card()
        {
            VisualElement card = new VisualElement();
            card.style.backgroundColor = CardBackground;
            SetBorder(card, CardBorder, 1);
            SetRadius(card, 5);
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            return card;
        }

        public static Label Hint(string _text)
        {
            Label hint = new Label(_text);
            hint.enableRichText = true;
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.style.color = MutedText;
            hint.style.fontSize = 11;
            return hint;
        }

        /// <summary>A coloured dot and a line of text: green fine, yellow needs attention, red broken.</summary>
        public static VisualElement StatusRow(Color _colour, string _text, string _tooltip)
        {
            VisualElement row = Row();
            row.style.marginTop = 3;
            row.style.marginBottom = 3;
            row.tooltip = _tooltip;
            VisualElement dot = Dot(_colour);
            row.Add(dot);
            Label text = new Label(_text);
            text.enableRichText = true;
            text.style.whiteSpace = WhiteSpace.Normal;
            text.style.flexShrink = 1;
            text.style.marginLeft = 6;
            row.Add(text);
            return row;
        }

        public static VisualElement Dot(Color _colour)
        {
            VisualElement dot = new VisualElement();
            dot.style.width = 10;
            dot.style.height = 10;
            dot.style.flexShrink = 0;
            dot.style.backgroundColor = _colour;
            SetRadius(dot, 5);
            return dot;
        }

        public static VisualElement Row()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            return row;
        }

        public static VisualElement Spacer()
        {
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            return spacer;
        }

        public static Button Button(string _text, string _tooltip, Action _clicked)
        {
            Button button = new Button(_clicked);
            button.text = _text;
            button.tooltip = _tooltip;
            return button;
        }

        /// <summary>The button that matters most in its row, such as Open Scene System: the same button, slightly blue.</summary>
        public static Button PrimaryButton(string _text, string _tooltip, Action _clicked)
        {
            Button button = Button(_text, _tooltip, _clicked);
            button.style.backgroundColor = PrimaryButtonBackground;
            button.RegisterCallback<PointerEnterEvent>(_event => button.style.backgroundColor = PrimaryButtonHover);
            button.RegisterCallback<PointerLeaveEvent>(_event => button.style.backgroundColor = PrimaryButtonBackground);
            return button;
        }

        /// <summary>Selects the asset and shows it in the Project window.</summary>
        public static void PingAndSelect(UnityEngine.Object _asset)
        {
            if (_asset == null)
            {
                return;
            }
            Selection.activeObject = _asset;
            EditorGUIUtility.PingObject(_asset);
        }

        /// <summary>A centred line, a little narrower than its parent, that sets a header apart from what follows.</summary>
        public static VisualElement Separator()
        {
            VisualElement line = new VisualElement();
            line.style.height = 2;
            line.style.width = Length.Percent(80);
            line.style.alignSelf = Align.Center;
            line.style.flexShrink = 0;
            line.style.backgroundColor = SeparatorColour;
            SetRadius(line, 1);
            return line;
        }

        private static Color SeparatorColour
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.78f, 0.78f, 0.80f, 0.75f) : new Color(0.35f, 0.35f, 0.37f, 0.75f); }
        }

        /// <summary>The Abstract Pixel logo, centred. Nothing shows when the image is missing from Resources.</summary>
        public static VisualElement Logo(float _height)
        {
            Texture2D texture = Resources.Load<Texture2D>(LogoResource);
            if (texture == null)
            {
                VisualElement nothing = new VisualElement();
                nothing.style.display = DisplayStyle.None;
                return nothing;
            }
            float aspect = (float)texture.width / texture.height;
            Image logo = new Image();
            logo.image = texture;
            logo.scaleMode = ScaleMode.ScaleToFit;
            logo.style.height = _height;
            logo.style.width = _height * aspect;
            logo.style.flexShrink = 0;
            logo.style.alignSelf = Align.Center;
            return logo;
        }

        public static HelpBox HelpBoxWithButton(string _message, HelpBoxMessageType _type, string _buttonText, string _tooltip, Action _clicked)
        {
            HelpBox box = new HelpBox(_message, _type);
            box.style.alignItems = Align.Center;
            if (_clicked == null)
            {
                return box;
            }
            Button button = Button(_buttonText, _tooltip, _clicked);
            button.style.flexShrink = 0;
            box.Add(button);
            return box;
        }

        /// <summary>A wizard page's big centred title and one short line under it.</summary>
        public static VisualElement PageHeader(string _title, string _line)
        {
            VisualElement header = new VisualElement();
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 16;
            Label title = new Label(_title);
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 6;
            header.Add(title);
            Label line = new Label(_line);
            line.enableRichText = true;
            line.style.whiteSpace = WhiteSpace.Normal;
            line.style.unityTextAlign = TextAnchor.MiddleCenter;
            line.style.color = MutedText;
            header.Add(line);
            return header;
        }

        /// <summary>A clickable option: radio dot, bold title, one grey line. The selected card has a blue border.</summary>
        public static VisualElement ChoiceCard(string _title, string _line, bool _selected, bool _enabled, string _tooltip, Action _clicked)
        {
            VisualElement card = Row();
            card.tooltip = _tooltip;
            card.style.marginBottom = 6;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.backgroundColor = CardBackground;
            SetBorder(card, _selected ? OverrideBlue : CardBorder, _selected ? 2 : 1);
            SetRadius(card, 6);

            VisualElement radio = new VisualElement();
            radio.style.width = 14;
            radio.style.height = 14;
            radio.style.flexShrink = 0;
            radio.style.marginRight = 10;
            SetRadius(radio, 7);
            SetBorder(radio, _selected ? OverrideBlue : MutedText, 2);
            if (_selected)
            {
                radio.style.backgroundColor = OverrideBlue;
            }
            card.Add(radio);

            VisualElement text = new VisualElement();
            text.style.flexShrink = 1;
            Label title = new Label(_title);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            text.Add(title);
            Label line = new Label(_line);
            line.style.whiteSpace = WhiteSpace.Normal;
            line.style.color = MutedText;
            line.style.fontSize = 11;
            text.Add(line);
            card.Add(text);

            card.SetEnabled(_enabled);
            if (_enabled && _clicked != null)
            {
                card.RegisterCallback<ClickEvent>(_event => _clicked());
            }
            return card;
        }

        /// <summary>A small label next to a part of a scene group: "From set 'Level'" in grey, or "Override" in blue.</summary>
        public static Label Badge(string _text, bool _isOverride, string _tooltip)
        {
            Label badge = new Label(_text);
            badge.tooltip = _tooltip;
            badge.style.fontSize = 11;
            badge.style.paddingLeft = 6;
            badge.style.paddingRight = 6;
            badge.style.paddingTop = 1;
            badge.style.paddingBottom = 1;
            SetRadius(badge, 3);
            if (_isOverride)
            {
                badge.style.unityFontStyleAndWeight = FontStyle.Bold;
                badge.style.color = Color.white;
                badge.style.backgroundColor = OverrideBlue;
            }
            else
            {
                badge.style.color = MutedText;
                SetBorder(badge, CardBorder, 1);
            }
            return badge;
        }

        public static void SetBorder(VisualElement _element, Color _colour, float _width)
        {
            _element.style.borderTopWidth = _width;
            _element.style.borderBottomWidth = _width;
            _element.style.borderLeftWidth = _width;
            _element.style.borderRightWidth = _width;
            _element.style.borderTopColor = _colour;
            _element.style.borderBottomColor = _colour;
            _element.style.borderLeftColor = _colour;
            _element.style.borderRightColor = _colour;
        }

        public static void SetRadius(VisualElement _element, float _radius)
        {
            _element.style.borderTopLeftRadius = _radius;
            _element.style.borderTopRightRadius = _radius;
            _element.style.borderBottomLeftRadius = _radius;
            _element.style.borderBottomRightRadius = _radius;
        }
    }
}
