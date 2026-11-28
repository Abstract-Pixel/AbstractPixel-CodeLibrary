using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using AbstractPixel.Core.Editor.UI;
using static AbstractPixel.Core.Editor.UI.EditorUI;

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
        private static Color SelectedCardBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.24f, 0.29f, 0.35f) : new Color(0.76f, 0.83f, 0.92f); }
        }

        private static Color DefaultCardBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.40f, 0.68f, 0.44f) : new Color(0.20f, 0.50f, 0.26f); }
        }

        private static Color ReadOnlyBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.84f, 0.84f, 0.84f); }
        }

        private static Color ReadOnlyBorder
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.76f, 0.76f, 0.76f); }
        }

        /// <summary>
        /// A page title with a help button at its right end. Unity 6.3 does not draw SettingsProvider.titleBarGuiHandler
        /// for a UI Toolkit page, so the page's "?" sits in its own title row.
        /// </summary>
        public static VisualElement PageTitleWithHelp(string _title, string _tooltip, string _topicId)
        {
            VisualElement row = Row();
            row.Add(PageTitle(_title, _tooltip));
            row.Add(Spacer());
            row.Add(SceneSystemHelp.Button(_topicId, "Opens the scene documentation about this page"));
            return row;
        }

        /// <summary>Same as Section, with a help button at the right end of the header that opens this topic.</summary>
        public static VisualElement Section(VisualElement _parent, string _title, string _tooltip, string _topicId)
        {
            VisualElement card = EditorUI.Section(_parent, _title, _tooltip, out VisualElement headerRow);
            if (!string.IsNullOrEmpty(_topicId))
            {
                headerRow.Add(SceneSystemHelp.Button(_topicId, "Opens the scene documentation about " + _title));
            }
            return card;
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
            // Flat and muted, so it reads as information and not as a field to type in.
            field.style.color = MutedText;
            field.style.backgroundColor = ReadOnlyBackground;
            field.style.overflow = Overflow.Hidden;
            field.style.whiteSpace = WhiteSpace.NoWrap;
            field.style.textOverflow = TextOverflow.Ellipsis;
            field.style.unityTextOverflowPosition = TextOverflowPosition.Start;
            SetBorder(field, ReadOnlyBorder, 1);
            SetRadius(field, 3);
            field.RegisterCallback<ClickEvent>(_event => PingAndSelect(_asset));
            return field;
        }

        /// <summary>
        /// A reference to an asset in Unity's object field, shown but not changeable: a click pings the asset, a double
        /// click selects it, and anything dropped or picked is put back. The picker button is hidden.
        /// </summary>
        public static ObjectField ReadOnlyObjectField(UnityEngine.Object _asset, Type _type, string _tooltip)
        {
            ObjectField field = new ObjectField();
            field.objectType = _type;
            field.allowSceneObjects = false;
            field.SetValueWithoutNotify(_asset);
            field.tooltip = _tooltip;
            field.style.marginLeft = 0;
            field.RegisterValueChangedCallback(_change =>
            {
                if (_change.newValue == _asset)
                {
                    return;
                }
                field.SetValueWithoutNotify(_asset);
            });
            VisualElement picker = field.Q(className: ObjectField.selectorUssClassName);
            if (picker != null)
            {
                picker.style.display = DisplayStyle.None;
            }
            // The same flat, muted look as the path box, so it doesn't look editable.
            VisualElement input = field.Q(className: ObjectField.inputUssClassName);
            if (input != null)
            {
                input.style.backgroundColor = ReadOnlyBackground;
                SetBorder(input, ReadOnlyBorder, 1);
            }
            Label display = field.Q<Label>(className: "unity-object-field-display__label");
            if (display != null)
            {
                display.style.color = MutedText;
            }
            return field;
        }

        /// <summary>The default scene set's accent: its name and its "Chosen default scene set" tag.</summary>
        public static Color DefaultAccent
        {
            get { return DefaultCardBorder; }
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

        /// <summary>A scene set's card on the page. The default set's card has a green outline with a thick left edge.</summary>
        public static VisualElement SetCard(bool _isDefault)
        {
            VisualElement card = new VisualElement();
            card.style.marginBottom = 4;
            card.style.paddingLeft = 4;
            card.style.paddingRight = 6;
            card.style.paddingTop = 3;
            card.style.paddingBottom = 3;
            SetRadius(card, 4);
            if (_isDefault)
            {
                // A green outline with a thick left edge, so the default card stands out at a glance and never looks
                // like the blue of a selected card.
                SetBorder(card, DefaultCardBorder, 1);
                card.style.borderLeftWidth = 4;
            }
            else
            {
                SetBorder(card, CardBorder, 1);
            }
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

        /// <summary>
        /// A setup step's "Terminology" block (session 5A2): a small heading, then one grey line per word, the word in
        /// bold italics followed by its plain definition from <see cref="SceneSystemTerms"/>. Empty when no words are given.
        /// </summary>
        public static VisualElement Terminology(params string[] _words)
        {
            VisualElement block = new VisualElement();
            block.AddToClassList("scene-system-terminology");
            if (_words == null || _words.Length == 0)
            {
                return block;
            }
            block.style.marginBottom = 12;
            block.style.paddingLeft = 4;
            Label heading = new Label("Terminology");
            heading.style.fontSize = 11;
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            heading.style.color = MutedText;
            heading.style.marginBottom = 3;
            block.Add(heading);
            foreach (string word in _words)
            {
                Label line = new Label("<b><i>" + word + "</i></b>  " + SceneSystemTerms.Definition(word));
                line.enableRichText = true;
                line.style.whiteSpace = WhiteSpace.Normal;
                line.style.color = MutedText;
                line.style.fontSize = 11;
                line.style.marginBottom = 2;
                block.Add(line);
            }
            return block;
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

    }
}
