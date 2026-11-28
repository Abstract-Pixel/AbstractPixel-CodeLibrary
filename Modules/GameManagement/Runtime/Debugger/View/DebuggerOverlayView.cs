using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>Builds the overlay's panel from a snapshot. Nothing in it can be picked or focused, so it never takes input.</summary>
    internal class DebuggerOverlayView
    {
        private readonly VisualElement panel;

        public VisualElement Root { get; }

        public DebuggerOverlayView()
        {
            Root = Box("gm-game-top");
            panel = Box("gm-overlay");
            Root.Add(panel);
            IgnoreInput(Root);
        }

        public void Show(DebuggerSnapshot _snapshot, string _keyHint)
        {
            panel.Clear();
            panel.Add(Text("GAME STATE", "gm-o-title"));
            panel.Add(Stack(_snapshot));
            panel.Add(Effects(_snapshot));
            if (_snapshot.LastRefusal != null)
            {
                panel.Add(Refusal(_snapshot.LastRefusal));
            }
            panel.Add(Journal(_snapshot));
            if (!string.IsNullOrEmpty(_keyHint))
            {
                panel.Add(Text(_keyHint, "gm-keyhint"));
            }
            IgnoreInput(Root);
        }

        private static VisualElement Stack(DebuggerSnapshot _snapshot)
        {
            VisualElement row = Box("gm-stack");
            if (_snapshot.Stack.Count == 0)
            {
                row.Add(Text("No state active", "gm-dim"));
                return row;
            }

            foreach (DebuggerStackEntry entry in _snapshot.Stack)
            {
                if (row.childCount > 0)
                {
                    row.Add(Text("›", "gm-sep"));
                }
                VisualElement chip = Box("gm-state-chip");
                chip.Add(Dot(entry.Color));
                chip.Add(Text(entry.Name, entry.IsTop ? "gm-top-state" : "gm-dim", "gm-name"));
                row.Add(chip);
            }
            // Text can only be measured once the line is laid out in a panel.
            row.RegisterCallback<GeometryChangedEvent>(_event => FitStackNames(row));
            return row;
        }

        private static void FitStackNames(VisualElement _row)
        {
            List<Label> names = _row.Query<Label>(className: "gm-name").ToList();
            float taken = 0f;
            foreach (VisualElement part in _row.Query<VisualElement>().ToList())
            {
                if (part.ClassListContains("gm-sep") || part.ClassListContains("gm-state-dot"))
                {
                    taken += part.layout.width + part.resolvedStyle.marginLeft + part.resolvedStyle.marginRight;
                }
            }

            float[] wanted = new float[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                Vector2 size = names[i].MeasureTextSize(names[i].text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined);
                // Rounded up with a pixel to spare, or a name that fits would still get an ellipsis.
                wanted[i] = Mathf.Ceil(size.x) + 1f;
            }

            float[] widths = StackNameFit.Widths(wanted, _row.contentRect.width - taken);
            for (int i = 0; i < names.Count; i++)
            {
                names[i].style.width = widths[i];
            }
        }

        private static VisualElement Effects(DebuggerSnapshot _snapshot)
        {
            VisualElement block = Block("EFFECTS");
            block.Add(KeyValue("Time", _snapshot.Time));
            block.Add(KeyValue("Cursor", _snapshot.Cursor));
            block.Add(KeyValue("HUD", _snapshot.Hud));
            block.Add(KeyValue("Input", _snapshot.Input));
            return block;
        }

        private static VisualElement KeyValue(string _key, string _value)
        {
            VisualElement row = Box("gm-kv");
            row.Add(Text(_key, "gm-k"));
            row.Add(Text(_value, "gm-v", "gm-name"));
            return row;
        }

        private static VisualElement Refusal(string _sentence)
        {
            VisualElement block = Block("LAST REFUSAL");
            VisualElement row = Box("gm-refusal");
            DebuggerIcon icon = new DebuggerIcon(DebuggerIconShape.Warning, "gm-refusal-icon");
            icon.Add(Text("!", "gm-refusal-mark"));
            row.Add(icon);
            row.Add(Text(_sentence, "gm-refusal-text"));
            block.Add(row);
            return block;
        }

        private static VisualElement Journal(DebuggerSnapshot _snapshot)
        {
            VisualElement block = Box("gm-o-block", "gm-journal");
            VisualElement head = Box("gm-j-section-head");
            head.Add(Text("JOURNAL", "gm-o-title"));
            head.Add(Text("newest first", "gm-j-subtitle"));
            block.Add(head);
            if (_snapshot.Journal.Count == 0)
            {
                block.Add(Text("No transitions yet.", "gm-j-empty"));
                return block;
            }

            foreach (DebuggerJournalGroup group in _snapshot.Journal)
            {
                VisualElement box = Box("gm-j-group");
                if (group.Number == 1)
                {
                    box.AddToClassList("gm-j-group-first");
                }
                VisualElement headRow = Box("gm-j-head-row");
                headRow.Add(Text(group.Number.ToString(), "gm-j-index"));
                headRow.Add(Text(group.Time, "gm-j-time"));
                box.Add(headRow);
                foreach (DebuggerJournalLine line in group.Lines)
                {
                    box.Add(JournalLine(line));
                }
                block.Add(box);
            }
            return block;
        }

        private static VisualElement JournalLine(DebuggerJournalLine _line)
        {
            VisualElement row = Box("gm-j-state-row");
            row.Add(Dot(_line.Color));
            row.Add(Text(_line.Name, "gm-j-state", "gm-name"));
            foreach (TransitionKind kind in _line.Events)
            {
                if (kind == TransitionKind.Refused)
                {
                    row.Add(Text("!", "gm-event-mark", "gm-event-refused"));
                    row.Add(Text("Refused", "gm-j-refused"));
                    continue;
                }
                row.Add(Icon(kind));
            }
            return row;
        }

        private static DebuggerIcon Icon(TransitionKind _kind)
        {
            if (_kind == TransitionKind.Entered)
            {
                return Event(DebuggerIconShape.Entered, "gm-event-entered");
            }
            if (_kind == TransitionKind.Applied)
            {
                return Event(DebuggerIconShape.Applied, "gm-event-applied");
            }
            return Event(DebuggerIconShape.Left, "gm-event-left");
        }

        private static DebuggerIcon Event(DebuggerIconShape _shape, string _class)
        {
            DebuggerIcon icon = new DebuggerIcon(_shape, _class);
            icon.AddToClassList("gm-event-icon");
            return icon;
        }

        private static VisualElement Dot(int _color)
        {
            VisualElement dot = Box("gm-state-dot");
            if (_color != StateColors.None)
            {
                dot.AddToClassList("gm-state-dot-" + _color);
            }
            return dot;
        }

        private static VisualElement Block(string _title)
        {
            VisualElement block = Box("gm-o-block");
            block.Add(Text(_title, "gm-o-title"));
            return block;
        }

        private static VisualElement Box(params string[] _classes)
        {
            VisualElement box = new VisualElement();
            foreach (string name in _classes)
            {
                box.AddToClassList(name);
            }
            return box;
        }

        private static Label Text(string _text, params string[] _classes)
        {
            Label label = new Label(_text);
            // A state name with angle brackets must show as typed, not as rich text.
            label.enableRichText = false;
            foreach (string name in _classes)
            {
                label.AddToClassList(name);
            }
            return label;
        }

        private static void IgnoreInput(VisualElement _element)
        {
            _element.pickingMode = PickingMode.Ignore;
            _element.focusable = false;
            foreach (VisualElement child in _element.hierarchy.Children())
            {
                IgnoreInput(child);
            }
        }
    }
}
