using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene system's look in one place: section headers (explanations in their tooltips), cards, status rows with a
    /// coloured dot (green fine, yellow needs attention, red broken), help boxes with a fix button, scene lists, the
    /// scene set dropdown, and the tooltips in glossary words. Used by the Scene System page, the setup window and the
    /// inspectors.
    /// </summary>
    public static class SceneSystemUI
    {
        public static readonly Color Green = new Color(0.30f, 0.75f, 0.36f);
        public static readonly Color Yellow = new Color(0.96f, 0.74f, 0.19f);
        public static readonly Color Red = new Color(0.89f, 0.29f, 0.24f);
        private static readonly Color OverrideBlue = new Color(0.17f, 0.52f, 0.92f);

        public const string SceneGroupTooltip = "An asset that describes one place the player can be, such as a level or the main menu. Click a name to select it.";
        public const string MainSceneTooltip = "The one scene that makes this place, such as 'Level 7'. It becomes Unity's active scene. A scene is the main scene of at most one scene group.";
        public const string ManagerScenesTooltip = "Scenes that open once and stay open across scene changes, such as the scene holding audio and save managers.";
        public const string SharedScenesTooltip = "Scenes several scene groups use next to their main scene, such as the scene holding the player's car. They stay open between two groups that both use them, unless the group resets shared scenes.";
        public const string ResetSharedScenesTooltip = "Closes and reopens every shared scene on each scene change, so state kept in them starts fresh.";
        public const string SceneSetTooltip = "The scene set that fills this group's manager scenes, shared scenes and Reset shared scenes. A part you change here becomes an override and keeps your value.";
        public const string OverrideTooltip = "This part keeps the group's own value instead of following its scene set, until you click Revert to set.";
        public const string FromSetTooltip = "This part follows the scene set: changing the set changes it here too. Editing it here makes it an override.";
        public const string RevertTooltip = "Ends the override: the part takes the scene set's value again and follows the set from now on.";
        public const string OpenGroupTooltip = "Opens the main scene with its manager scenes and shared scenes, as they are in Play.";
        private const string NoSetLabel = "None (the group's own scenes)";
        private const string LogoResource = "AbstractPixelLogo";

        private static Color MutedText
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.30f, 0.30f, 0.30f); }
        }

        private static Color CardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.87f, 0.87f, 0.87f); }
        }

        private static Color CardBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.66f, 0.66f, 0.66f); }
        }

        private static Color InnerCardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.235f, 0.235f, 0.235f) : new Color(0.92f, 0.92f, 0.92f); }
        }

        private static Color SelectedCardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.24f, 0.29f, 0.35f) : new Color(0.76f, 0.83f, 0.92f); }
        }

        private static Color DefaultCardBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.36f, 0.48f, 0.62f) : new Color(0.42f, 0.56f, 0.74f); }
        }

        private static Color HeaderHoverText
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

        private static Color FieldBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.165f, 0.165f, 0.165f) : new Color(0.94f, 0.94f, 0.94f); }
        }

        private static Color FieldBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.70f, 0.70f, 0.70f); }
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
        private static void PingAndSelect(UnityEngine.Object _asset)
        {
            if (_asset == null)
            {
                return;
            }
            Selection.activeObject = _asset;
            EditorGUIUtility.PingObject(_asset);
        }

        /// <summary>
        /// An asset's path in a read-only box that looks like a field, ending visible ("…/Scene Sets/Level.asset"). A
        /// click selects the asset and shows it in the Project window, which a disabled object field can't do.
        /// </summary>
        public static Label AssetPathField(UnityEngine.Object _asset)
        {
            string path = AssetDatabase.GetAssetPath(_asset);
            Label field = new Label(path);
            field.tooltip = path + "\nClick to show it in the Project window.";
            field.style.height = 18;
            field.style.paddingLeft = 4;
            field.style.paddingRight = 4;
            field.style.unityTextAlign = TextAnchor.MiddleLeft;
            field.style.backgroundColor = FieldBackground;
            field.style.overflow = Overflow.Hidden;
            field.style.whiteSpace = WhiteSpace.NoWrap;
            field.style.textOverflow = TextOverflow.Ellipsis;
            field.style.unityTextOverflowPosition = TextOverflowPosition.Start;
            SetBorder(field, FieldBorder, 1);
            SetRadius(field, 3);
            field.RegisterCallback<ClickEvent>(_event => PingAndSelect(_asset));
            return field;
        }

        /// <summary>An asset's name as a link: a click selects it and shows it in the Project window.</summary>
        public static Label AssetLink(UnityEngine.Object _asset, string _text)
        {
            Label link = new Label(_text);
            link.tooltip = AssetDatabase.GetAssetPath(_asset) + "\nClick to select it.";
            link.style.overflow = Overflow.Hidden;
            link.style.whiteSpace = WhiteSpace.NoWrap;
            link.style.textOverflow = TextOverflow.Ellipsis;
            AddHoverTint(link);
            link.RegisterCallback<ClickEvent>(_event => PingAndSelect(_asset));
            return link;
        }

        /// <summary>A scene set's card on the page. The default set's card has a quiet blue outline.</summary>
        public static VisualElement SetCard(bool _isDefault)
        {
            VisualElement card = new VisualElement();
            card.style.marginBottom = 4;
            card.style.paddingLeft = 4;
            card.style.paddingRight = 6;
            card.style.paddingTop = 3;
            card.style.paddingBottom = 3;
            SetRadius(card, 4);
            Color border = CardBorder;
            if (_isDefault)
            {
                border = DefaultCardBorder;
            }
            SetBorder(card, border, 1);
            ShowSelected(card, false);
            return card;
        }

        /// <summary>The background of a selected scene set card, like a selected row in a list.</summary>
        public static void ShowSelected(VisualElement _card, bool _isSelected)
        {
            Color background = InnerCardBackground;
            if (_isSelected)
            {
                background = SelectedCardBackground;
            }
            _card.style.backgroundColor = background;
        }

        /// <summary>The + and − buttons under a list, as Unity's lists have them.</summary>
        public static VisualElement AddRemoveFooter(string _addTooltip, Action _add, string _removeTooltip, Action _remove, out Button _removeButton)
        {
            VisualElement footer = Row();
            footer.style.alignSelf = Align.FlexEnd;
            footer.style.backgroundColor = InnerCardBackground;
            footer.style.paddingLeft = 2;
            footer.style.paddingRight = 2;
            SetBorder(footer, CardBorder, 1);
            footer.style.borderTopWidth = 0;
            footer.style.borderBottomLeftRadius = 4;
            footer.style.borderBottomRightRadius = 4;
            Button add = Button("+", _addTooltip, _add);
            Button remove = Button("−", _removeTooltip, _remove);
            foreach (Button button in new[] { add, remove })
            {
                button.style.width = 25;
                button.style.height = 16;
                button.style.marginTop = 1;
                button.style.marginBottom = 2;
                button.style.unityFontStyleAndWeight = FontStyle.Bold;
                footer.Add(button);
            }
            _removeButton = remove;
            return footer;
        }

        /// <summary>
        /// The scene set dropdown of the scene group inspector and the Scene Groups Debugger: None plus every scene set,
        /// plus the group's own set when the settings don't list it. Calls back only when the designer picks another set.
        /// </summary>
        public static PopupField<SceneSet> SceneSetPopup(string _label, SceneSystemSettings _settings, SceneSet _current, Action<SceneSet> _picked)
        {
            List<SceneSet> choices = new List<SceneSet> { null };
            if (_settings != null)
            {
                foreach (SceneSet set in _settings.SceneSets)
                {
                    if (set != null)
                    {
                        choices.Add(set);
                    }
                }
            }
            if (_current != null && !choices.Contains(_current))
            {
                choices.Add(_current);
            }

            // The index form, because the value form refuses null, and None is a real choice.
            int currentIndex = choices.IndexOf(_current);
            PopupField<SceneSet> field = new PopupField<SceneSet>(_label, choices, currentIndex, SetLabel, SetLabel);
            field.tooltip = SceneSetTooltip;
            field.RegisterValueChangedCallback(_change =>
            {
                // Binding and refreshes raise change events too; only a different set is the designer's choice.
                if (_change.newValue == _change.previousValue)
                {
                    return;
                }
                _picked(_change.newValue);
            });
            return field;
        }

        private static string SetLabel(SceneSet _set)
        {
            if (_set == null)
            {
                return NoSetLabel;
            }
            return _set.name;
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

        /// <summary>One scene field (a SceneReference's inner SceneField), drawn by the scene field drawer with its status box.</summary>
        public static PropertyField SceneField(SerializedProperty _sceneReferenceProperty, string _label, string _tooltip)
        {
            SerializedProperty sceneFieldProperty = _sceneReferenceProperty.FindPropertyRelative("scene");
            PropertyField field = new PropertyField(sceneFieldProperty, _label);
            field.tooltip = _tooltip;
            return field;
        }

        /// <summary>A reorderable list of scenes numbered from 1, each drawn by the scene field drawer. Select a row and click − to remove it.</summary>
        public static ListView SceneList(SerializedObject _serializedObject, string _propertyName, string _title, string _tooltip)
        {
            SerializedProperty listProperty = _serializedObject.FindProperty(_propertyName);
            ListView list = new ListView();
            list.headerTitle = _title;
            list.tooltip = _tooltip;
            list.showFoldoutHeader = !string.IsNullOrEmpty(_title);
            list.showAddRemoveFooter = true;
            list.reorderable = true;
            list.reorderMode = ListViewReorderMode.Animated;
            list.showBoundCollectionSize = false;
            list.selectionType = SelectionType.Single;
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            list.style.marginTop = 2;
            list.makeItem = () => new PropertyField();
            list.bindItem = (_element, _index) =>
            {
                PropertyField field = (PropertyField)_element;
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(_index);
                SerializedProperty sceneFieldProperty = elementProperty.FindPropertyRelative("scene");
                int rowNumber = _index + 1;
                field.label = rowNumber.ToString();
                field.tooltip = _tooltip;
                field.BindProperty(sceneFieldProperty);
            };
            list.BindProperty(listProperty);
            return list;
        }

        /// <summary>A scene set's manager scenes, shared scenes and Reset shared scenes, for the page and the scene set inspector.</summary>
        public static VisualElement SceneSetFields(SerializedObject _setObject)
        {
            VisualElement fields = new VisualElement();
            fields.Add(SceneList(_setObject, "managerScenes", "Manager scenes", ManagerScenesTooltip));
            fields.Add(SceneList(_setObject, "sharedScenes", "Shared scenes", SharedScenesTooltip));
            PropertyField reset = new PropertyField(_setObject.FindProperty("resetSharedScenes"), "Reset shared scenes");
            reset.tooltip = ResetSharedScenesTooltip;
            reset.style.marginTop = 6;
            fields.Add(reset);
            fields.Bind(_setObject);

            // A change to the set fills every scene group that uses it, right away.
            SceneSet set = _setObject.targetObject as SceneSet;
            fields.TrackSerializedObjectValue(_setObject, _changed =>
            {
                if (set == null)
                {
                    return;
                }
                EditorUtility.SetDirty(set);
                AssetDatabase.SaveAssetIfDirty(set);
                SceneSetSync.ApplyToGroupsUsing(set);
            });
            return fields;
        }

        private static void SetBorder(VisualElement _element, Color _colour, float _width)
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

        private static void SetRadius(VisualElement _element, float _radius)
        {
            _element.style.borderTopLeftRadius = _radius;
            _element.style.borderTopRightRadius = _radius;
            _element.style.borderBottomLeftRadius = _radius;
            _element.style.borderBottomRightRadius = _radius;
        }
    }
}
