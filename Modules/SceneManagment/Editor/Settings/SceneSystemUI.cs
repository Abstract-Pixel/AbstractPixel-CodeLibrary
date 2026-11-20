using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene system's look in one place: section headers with a one-line explanation, cards, status rows with a
    /// coloured dot (green fine, yellow needs attention, red broken), help boxes with a fix button, scene lists, and the
    /// tooltips in glossary words. Used by the Project Settings page, the setup window and the inspectors.
    /// </summary>
    public static class SceneSystemUI
    {
        public static readonly Color Green = new Color(0.30f, 0.75f, 0.36f);
        public static readonly Color Yellow = new Color(0.96f, 0.74f, 0.19f);
        public static readonly Color Red = new Color(0.89f, 0.29f, 0.24f);
        public static readonly Color OverrideBlue = new Color(0.17f, 0.52f, 0.92f);

        public const string MainSceneTooltip = "The one scene that makes this place, such as 'Level 7'. It becomes Unity's active scene. A scene is the main scene of at most one scene group.";
        public const string ManagerScenesTooltip = "Scenes that open once and stay open across scene changes, such as the scene holding audio and save managers.";
        public const string SharedScenesTooltip = "Scenes several scene groups use next to their main scene, such as the scene holding the player's car. They stay open between two groups that both use them, unless the group resets shared scenes.";
        public const string ResetSharedScenesTooltip = "Closes and reopens every shared scene on each scene change, so state kept in them starts fresh.";
        public const string SceneSetTooltip = "The scene set that fills this group's manager scenes, shared scenes and Reset shared scenes. A part you change here becomes an override and keeps your value.";
        public const string OverrideTooltip = "This part keeps the group's own value instead of following its scene set, until you click Revert to set.";
        public const string FromSetTooltip = "This part follows the scene set: changing the set changes it here too. Editing it here makes it an override.";
        public const string RevertTooltip = "Ends the override: the part takes the scene set's value again and follows the set from now on.";

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

        /// <summary>A page or window title with a short line under it.</summary>
        public static VisualElement Title(string _title, string _subtitle)
        {
            VisualElement header = new VisualElement();
            header.style.marginBottom = 4;
            Label title = new Label(_title);
            title.style.fontSize = 19;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 2;
            header.Add(title);
            header.Add(Hint(_subtitle));
            return header;
        }

        /// <summary>Adds a section (header, one-line explanation, card) to the parent and returns the card to fill.</summary>
        public static VisualElement Section(VisualElement _parent, string _title, string _explanation)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = 14;
            Label header = new Label(_title);
            header.style.fontSize = 14;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Add(header);
            Label explanation = Hint(_explanation);
            explanation.style.marginBottom = 5;
            section.Add(explanation);
            VisualElement card = Card();
            section.Add(card);
            _parent.Add(section);
            return card;
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
            header.style.marginBottom = 14;
            Label title = new Label(_title);
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 4;
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

        /// <summary>A reorderable list of scenes, each drawn by the scene field drawer.</summary>
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
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            list.style.marginTop = 2;
            list.makeItem = () => new PropertyField();
            list.bindItem = (_element, _index) =>
            {
                PropertyField field = (PropertyField)_element;
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(_index);
                SerializedProperty sceneFieldProperty = elementProperty.FindPropertyRelative("scene");
                field.label = "Scene " + _index;
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
