using System;
using System.Linq;
using AbstractPixel.Core.Editor.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    internal enum StateRowKind
    {
        Main,
        Sub,
        Empty
    }

    /// <summary>One row: reorder handle, rank badge, state field, the hover + of a main state, and its problems below.</summary>
    internal class StateRowView : VisualElement
    {
        public event Action<StateSO> StateChanged;
        public event Action<StateSO> AddSubStateClicked;

        public VisualElement Handle { get; }

        public StateRowView(GameStateCatalog _catalog, int _index, string _badge, StateRowKind _kind)
        {
            StateSO state = _catalog.States[_index];
            AddToClassList("gm-state-row");
            VisualElement line = new VisualElement();
            line.AddToClassList("gm-state-line");

            Handle = new Label("≡");
            Handle.tooltip = "Drag to change the rank";
            Handle.AddToClassList("gm-handle");
            line.Add(Handle);

            Label badge = new Label(_badge);
            badge.AddToClassList("gm-rank");
            if (_kind == StateRowKind.Sub)
            {
                badge.AddToClassList("gm-rank-sub");
            }
            line.Add(badge);

            VisualElement body = new VisualElement();
            body.AddToClassList("gm-state-body");
            ObjectField field = new ObjectField();
            field.objectType = typeof(StateSO);
            field.allowSceneObjects = false;
            field.value = state;
            field.tooltip = state != null ? state.name + "\nClick to select this state in the Project window." : "Pick a state for this slot.";
            field.AddToClassList("gm-state-field");
            field.RegisterValueChangedCallback(_change => StateChanged?.Invoke((StateSO)_change.newValue));
            field.RegisterCallback<ClickEvent>(_event => EditorUI.PingAndSelect(field.value));
            body.Add(field);
            if (_kind == StateRowKind.Sub && state != null)
            {
                string parents = string.Join(", ", state.Parents.Select(_parent => _parent != null ? _parent.name : "(empty)"));
                Label opensOver = new Label("Opens over: " + parents);
                opensOver.tooltip = opensOver.text;
                opensOver.AddToClassList("gm-opens-over");
                body.Add(opensOver);
            }
            line.Add(body);

            if (_kind == StateRowKind.Main)
            {
                Button addSub = new Button(() => AddSubStateClicked?.Invoke(state));
                addSub.text = "+";
                addSub.tooltip = "Add sub-state: make a new sub-state that opens over " + state.name;
                addSub.AddToClassList("gm-add-sub-button");
                line.Add(addSub);
            }
            else if (_kind == StateRowKind.Empty)
            {
                VisualElement spacer = new VisualElement();
                spacer.AddToClassList("gm-add-sub-space");
                line.Add(spacer);
            }
            Add(line);
        }

        public void AddProblem(StateProblem _problem, Action _fix)
        {
            Add(StateProblemBox.Create(_problem, _fix));
        }
    }
}
