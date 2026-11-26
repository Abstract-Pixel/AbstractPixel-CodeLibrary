using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// A numbered list whose steps tick themselves: each step names the message that proves it happened ("done-when").
    /// Pending is a hollow circle, done a filled one with a tick; the first pending step is the current one.
    /// </summary>
    public class DocsStepList
    {
        private const string Tick = "✓";
        private readonly List<VisualElement> rows = new List<VisualElement>();
        private readonly List<Label> circles = new List<Label>();
        private readonly List<string> doneWhen = new List<string>();
        private readonly List<bool> done = new List<bool>();

        public VisualElement Root { get; private set; }

        public DocsStepList()
        {
            Root = new VisualElement();
            Root.AddToClassList("docs-steps");
        }

        public int Count
        {
            get { return rows.Count; }
        }

        public void AddStep(VisualElement _content, string _doneWhen)
        {
            int number = rows.Count + 1;
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-step");
            Label circle = new Label(number.ToString());
            circle.AddToClassList("docs-step-circle");
            row.Add(circle);
            _content.AddToClassList("docs-step-content");
            row.Add(_content);
            if (!string.IsNullOrEmpty(_doneWhen))
            {
                circle.tooltip = "Ticks when the scene system reports " + _doneWhen + ".";
            }
            Root.Add(row);
            rows.Add(row);
            circles.Add(circle);
            doneWhen.Add(_doneWhen);
            done.Add(false);
            RefreshCurrent();
        }

        /// <summary>The message the step waits for, or null.</summary>
        public string DoneWhenOf(int _index)
        {
            return doneWhen[_index];
        }

        public bool IsDone(int _index)
        {
            return done[_index];
        }

        /// <summary>
        /// Steps tick in order: the message ticks the first pending step that waits for a message, when it is the one
        /// that step waits for, and marks the steps before it (which wait for nothing) done too. Other messages are
        /// ignored, so a later step never ticks early. Returns true when a step ticked.
        /// </summary>
        public bool Report(string _message)
        {
            int next = NextWaitingStep();
            if (next < 0 || doneWhen[next] != _message)
            {
                return false;
            }
            for (int index = 0; index <= next; index++)
            {
                if (!done[index])
                {
                    MarkDone(index);
                }
            }
            return true;
        }

        private int NextWaitingStep()
        {
            for (int index = 0; index < rows.Count; index++)
            {
                bool waits = !done[index] && !string.IsNullOrEmpty(doneWhen[index]);
                if (waits)
                {
                    return index;
                }
            }
            return -1;
        }

        public void MarkDone(int _index)
        {
            done[_index] = true;
            circles[_index].text = Tick;
            rows[_index].AddToClassList("docs-step-done");
            RefreshCurrent();
        }

        /// <summary>Back to all pending (a new Play).</summary>
        public void Reset()
        {
            for (int index = 0; index < rows.Count; index++)
            {
                done[index] = false;
                circles[index].text = (index + 1).ToString();
                rows[index].RemoveFromClassList("docs-step-done");
            }
            RefreshCurrent();
        }

        private void RefreshCurrent()
        {
            bool currentFound = false;
            for (int index = 0; index < rows.Count; index++)
            {
                bool isCurrent = !currentFound && !done[index];
                rows[index].EnableInClassList("docs-step-current", isCurrent);
                if (isCurrent)
                {
                    currentFound = true;
                }
            }
        }
    }
}
