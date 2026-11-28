using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MeasureMode = UnityEngine.UIElements.VisualElement.MeasureMode;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The read-only "over each parent" table: one numbered row per parent slot.</summary>
    internal class StateParentTable : VisualElement
    {
        // A value cell's side padding and divider line, and one pixel so rounding never cuts a letter.
        private const float CellChrome = 10f;
        // The number beside a parent's name.
        private const float PillWidth = 22f;
        private static readonly string[] Titles = { "Time", "Cursor", "HUD", "Input" };

        // What the columns are sized from: each value column's cells, heading included, and the parents' names.
        private readonly List<VisualElement>[] valueCells = { new List<VisualElement>(), new List<VisualElement>(), new List<VisualElement>(), new List<VisualElement>() };
        private readonly List<Label> parentNames = new List<Label>();

        /// <summary>Goes under the table: the long input map names in full.</summary>
        public StateParentMapLegend Legend { get; } = new StateParentMapLegend();

        public StateParentTable()
        {
            AddToClassList("si-table");
            RegisterCallback<GeometryChangedEvent>(_event => FitColumns());
        }

        public void Rebuild(StateSO _state)
        {
            Clear();
            parentNames.Clear();
            foreach (List<VisualElement> column in valueCells)
            {
                column.Clear();
            }
            VisualElement head = Row(null, "Parent");
            head.AddToClassList("si-table-head");
            for (int column = 0; column < Titles.Length; column++)
            {
                VisualElement cell = Cell(null, Titles[column], Titles[column]);
                valueCells[column].Add(cell);
                head.Add(cell);
            }
            Add(head);
            List<KeyValuePair<int, StateEffectCell>> listed = new List<KeyValuePair<int, StateEffectCell>>();

            for (int i = 0; i < _state.Parents.Count; i++)
            {
                StateSO parent = _state.Parents[i];
                VisualElement row = Row((i + 1).ToString(), parent != null ? parent.name : "None");
                row.EnableInClassList("si-table-alt", i % 2 == 1);
                foreach (StateEffectKind kind in (StateEffectKind[])Enum.GetValues(typeof(StateEffectKind)))
                {
                    string title = Titles[(int)kind];
                    if (parent == null)
                    {
                        VisualElement empty = Cell(title, "-", "This parent slot is empty.");
                        valueCells[(int)kind].Add(empty);
                        row.Add(empty);
                        continue;
                    }
                    StateEffectCell value = StateParentValues.Cell(_state, parent, kind);
                    VisualElement cell = Cell(title, value.Text, value.Tooltip);
                    cell.Q<Label>(className: "si-value-text").EnableInClassList("si-value-changed", value.IsChanged);
                    valueCells[(int)kind].Add(cell);
                    row.Add(cell);
                    if (value.ListedMaps != null)
                    {
                        listed.Add(new KeyValuePair<int, StateEffectCell>(i + 1, value));
                    }
                }
                parentNames.Add(row.Q<Label>(className: "si-parent-name"));
                // A new row has no text sizes until it is first laid out.
                row.RegisterCallback<GeometryChangedEvent>(_event => FitColumns());
                Add(row);
            }
            Legend.Show(listed);
            schedule.Execute(FitColumns);
        }

        private void FitColumns()
        {
            float width = layout.width;
            if (float.IsNaN(width) || width <= 0f || panel == null)
            {
                return;
            }
            float[] needs = new float[valueCells.Length];
            for (int column = 0; column < valueCells.Length; column++)
            {
                foreach (VisualElement cell in valueCells[column])
                {
                    needs[column] = Mathf.Max(needs[column], TextWidth(cell.Q<Label>(className: "si-value-text")) + CellChrome);
                }
            }
            float parentNeed = 0f;
            foreach (Label name in parentNames)
            {
                parentNeed = Mathf.Max(parentNeed, TextWidth(name) + PillWidth);
            }

            float[] widths = StateTableColumns.ValueWidths(needs);
            bool fits = StateTableColumns.Fits(width, widths, parentNeed);
            // When the values can't all sit beside a readable name, each parent's values stack under its name.
            EnableInClassList("si-table-narrow", !fits);
            for (int column = 0; column < valueCells.Length; column++)
            {
                foreach (VisualElement cell in valueCells[column])
                {
                    cell.style.flexBasis = fits ? new StyleLength(widths[column]) : new StyleLength(StyleKeyword.Null);
                }
            }
        }

        // The width the whole text takes on one line, with the label's own padding and margins (a teal tag is bold and padded).
        private static float TextWidth(Label _label)
        {
            Vector2 size = _label.MeasureTextSize(_label.text, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined);
            IResolvedStyle look = _label.resolvedStyle;
            return Mathf.Ceil(size.x) + look.paddingLeft + look.paddingRight + look.marginLeft + look.marginRight;
        }

        private static VisualElement Cell(string _caption, string _text, string _tooltip)
        {
            VisualElement cell = new VisualElement { tooltip = _tooltip };
            cell.AddToClassList("si-col");
            cell.AddToClassList("si-col-" + (_caption ?? _text).ToLowerInvariant());
            if (_caption != null)
            {
                Label caption = new Label(_caption);
                caption.AddToClassList("si-cell-caption");
                cell.Add(caption);
            }
            Label label = new Label(_text) { tooltip = _tooltip, enableRichText = false };
            label.AddToClassList("si-value-text");
            cell.Add(label);
            return cell;
        }

        // The name box joins the pill, the same way the parent fields do.
        private static VisualElement Row(string _index, string _name)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("si-table-row");
            VisualElement left = new VisualElement();
            left.AddToClassList("si-col-parent");
            if (_index != null)
            {
                Label pill = new Label(_index);
                pill.AddToClassList("si-rank");
                left.Add(pill);
            }
            Label name = new Label(_name) { tooltip = _name, enableRichText = false };
            name.AddToClassList(_index == null ? "si-parent-head" : "si-parent-name");
            left.Add(name);
            row.Add(left);
            return row;
        }
    }
}
