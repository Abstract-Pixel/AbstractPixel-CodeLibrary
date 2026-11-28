using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The input actions of an Own input effect: a numbered entry each, naming the action and the map it turns on.</summary>
    internal class StateInputActionList : VisualElement
    {
        private const string ListPath = nameof(StateSO.Effects) + "." + nameof(StateEffectSettings.InputMaps);
        // The text Unity writes inside an object field.
        private const string FieldTextClass = "unity-object-field-display__label";

        private readonly StateSO state;
        private readonly StateListPanel panel = new StateListPanel("Input actions",
            "List one action from each action map this state turns on. Every other map is turned off.", "gm-state-input-actions");
        private readonly StateReferenceList list;

        public StateInputActionList(SerializedObject _data)
        {
            state = (StateSO)_data.targetObject;
            list = new StateReferenceList(_data, ListPath, typeof(InputActionReference), "input action",
                "No actions listed, so every action map is turned off.", ShowAction, BuildEntry);
            list.AddToClassList("si-action-list");
            panel.Body.Add(list);
            Add(panel);
        }

        public void Refresh()
        {
            List<InputActionReference> actions = state.Effects.InputMaps;
            panel.ShowCount(actions.Count);
            list.Refresh();
        }

        // Two captioned lines: the field, showing only the action's name, and the map's name under it.
        private static void BuildEntry(VisualElement _body, ObjectField _field)
        {
            VisualElement actionLine = Line("Action");
            actionLine.Add(_field);
            _body.Add(actionLine);
            VisualElement mapLine = Line("Map");
            mapLine.AddToClassList("si-map-line");
            Label map = new Label { enableRichText = false };
            map.AddToClassList("si-map-name");
            mapLine.Add(map);
            _body.Add(mapLine);

            // Unity writes "Map/Action (Input Action Reference)" there, which hides the action behind a long map name
            // and repeats the line below. It writes it again whenever the field changes, so this answers each time.
            Label text = _field.Q<Label>(className: FieldTextClass);
            text?.RegisterValueChangedCallback(_change => ShowActionName(_field, _field.value));
        }

        private static void ShowActionName(ObjectField _field, Object _value)
        {
            Label text = _field.Q<Label>(className: FieldTextClass);
            InputActionReference reference = _value as InputActionReference;
            if (text != null && reference != null && reference.action != null)
            {
                ((INotifyValueChanged<string>)text).SetValueWithoutNotify(reference.action.name);
            }
        }

        private static VisualElement Line(string _caption)
        {
            VisualElement line = new VisualElement();
            line.AddToClassList("si-name-line");
            Label caption = new Label(_caption);
            caption.AddToClassList("si-name-caption");
            line.Add(caption);
            return line;
        }

        private static void ShowAction(VisualElement _row, Object _value)
        {
            InputActionReference reference = _value as InputActionReference;
            InputAction action = reference != null ? reference.action : null;
            InputActionMap map = action != null ? action.actionMap : null;
            _row.Q(className: "si-map-line").style.display = map != null ? DisplayStyle.Flex : DisplayStyle.None;
            _row.Q<Label>(className: "si-map-name").text = map != null ? map.name : "";
            _row.tooltip = map != null ? "Action: " + action.name + "\nMap: " + map.name : "Pick an input action.";
            ShowActionName(_row.Q<ObjectField>(), _value);
        }
    }
}
