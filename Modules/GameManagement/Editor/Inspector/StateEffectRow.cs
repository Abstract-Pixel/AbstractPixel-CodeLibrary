using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>One effect: its two-way switch, then either its own value or the muted line saying what it follows.</summary>
    internal class StateEffectRow : VisualElement
    {
        private readonly StateSO state;
        private readonly StateEffectKind kind;
        private readonly RadioButtonGroup choice;
        private readonly Foldout details = new Foldout { value = false };
        private readonly Label inherited = new Label();
        private readonly Label numbersNote = new Label("The numbers are the rows of the Parents list.");
        private readonly VisualElement ownValue;
        private bool? labelledForSubState;

        public StateEffectRow(SerializedObject _data, StateEffectKind _kind, string _label, VisualElement _ownValue, Action _changed)
        {
            state = (StateSO)_data.targetObject;
            kind = _kind;
            ownValue = _ownValue;
            AddToClassList("si-effect");

            choice = new RadioButtonGroup(_label);
            choice.AddToClassList("si-switch");
            choice.AddToClassList("si-field");
            choice.RegisterValueChangedCallback(_change =>
            {
                if (_change.newValue < 0)
                {
                    return;
                }
                _data.ApplyModifiedProperties();
                StateOwnValues.SetSource(state, kind, _change.newValue == 1 ? StateEffectSource.Own : StateEffectSource.SameAsParent);
                _data.Update();
                if (_change.newValue == 0)
                {
                    details.value = true;
                }
                _changed();
            });
            Add(choice);
            details.AddToClassList("si-details");
            inherited.AddToClassList("si-inherited");
            details.Add(inherited);
            numbersNote.AddToClassList("si-inherited");
            details.Add(numbersNote);
            Add(details);
            Add(ownValue);
        }

        public void Refresh(bool _showsSubState)
        {
            if (labelledForSubState != _showsSubState)
            {
                labelledForSubState = _showsSubState;
                // A main state has no parent, so its first choice keeps the last value instead.
                choice.choices = new List<string> { _showsSubState ? "Same as parent" : "Keep last", "Own" };
                choice.tooltip = _showsSubState
                    ? "Same as parent: use the value of the state under this one. Own: use this state's value."
                    : "Keep last: leave whatever was set before. Own: use this state's value.";
            }
            bool isOwn = StateOwnValues.SourceOf(state.Effects, kind) == StateEffectSource.Own;
            choice.SetValueWithoutNotify(isOwn ? 1 : 0);
            ownValue.style.display = isOwn ? DisplayStyle.Flex : DisplayStyle.None;
            details.style.display = isOwn ? DisplayStyle.None : DisplayStyle.Flex;
            details.text = _showsSubState ? "What it uses over each parent" : "What this means";
            if (!isOwn)
            {
                string line = _showsSubState && state.Parents.Count == 0
                    ? "No parent yet, so there is nothing to follow"
                    : StateParentValues.InheritedLine(state, kind);
                inherited.text = Emphasised(line);
                bool usesNumbers = line.StartsWith("Over parent", StringComparison.Ordinal);
                numbersNote.style.display = usesNumbers ? DisplayStyle.Flex : DisplayStyle.None;
                inherited.tooltip = usesNumbers ? NumberedParents() : null;
            }
        }

        private string NumberedParents()
        {
            return string.Join("\n", state.Parents.Select((_parent, _index) => (_index + 1) + "  " + (_parent != null ? _parent.name : "None")));
        }

        // With several values, each value is bold, so it stands out from the parents it belongs to.
        // State names are shown as written, never read as rich text.
        private static string Emphasised(string _line)
        {
            string[] lines = _line.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int colon = lines[i].StartsWith("Over ", StringComparison.Ordinal) ? lines[i].LastIndexOf(": ", StringComparison.Ordinal) : -1;
                lines[i] = colon < 0
                    ? Plain(lines[i])
                    : Plain(lines[i].Substring(0, colon + 2)) + "<b>" + Plain(lines[i].Substring(colon + 2)) + "</b>";
            }
            return string.Join("\n", lines);
        }

        private static string Plain(string _text)
        {
            return "<noparse>" + _text + "</noparse>";
        }
    }
}
