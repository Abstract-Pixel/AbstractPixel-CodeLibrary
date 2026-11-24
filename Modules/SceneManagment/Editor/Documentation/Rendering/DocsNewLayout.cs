using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Fact rows: a bullet list whose every item starts with "**Label:**" becomes label and value columns, one row
    /// each, with a divider between rows. (New layout only.)
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsFactRows
    {
        private static readonly Regex LabelPattern = new Regex(@"^\*\*([^*]+?):\*\*\s*(.*)$");

        public static bool Applies(DocsBlock _block)
        {
            if (_block.Ordered || _block.Items.Count < 2)
            {
                return false;
            }
            foreach (DocsListItem item in _block.Items)
            {
                if (!LabelPattern.IsMatch(item.Text ?? ""))
                {
                    return false;
                }
            }
            return true;
        }

        public static void Split(string _text, out string _label, out string _value)
        {
            Match match = LabelPattern.Match(_text ?? "");
            _label = match.Success ? match.Groups[1].Value : "";
            _value = match.Success ? match.Groups[2].Value : _text;
        }

        public static VisualElement Create(DocsBlock _block, DocsRenderContext _context)
        {
            VisualElement rows = new VisualElement();
            rows.AddToClassList("docs-facts");
            foreach (DocsListItem item in _block.Items)
            {
                string label;
                string value;
                Split(item.Text, out label, out value);
                VisualElement row = Row(label, value, _context);
                row.EnableInClassList("docs-facts-alt", rows.childCount % 2 == 1);
                rows.Add(row);
            }
            return rows;
        }

        public static VisualElement Row(string _label, string _valueMarkdown, DocsRenderContext _context)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-facts-row");
            Label label = new Label(_label);
            label.AddToClassList("docs-facts-label");
            row.Add(label);
            row.Add(DocsText.Create(_valueMarkdown, "docs-facts-value", _context));
            return row;
        }
    }

    /// <summary>
    /// A flow: a numbered list with no done-when, drawn as numbered circles joined by a line. An item with a nested
    /// numbered list is a phase: its text is the phase title and its steps carry on the numbering. (New layout only.)
    /// </summary>
    public static class DocsFlow
    {
        public static bool Applies(DocsBlock _block)
        {
            if (!_block.Ordered)
            {
                return false;
            }
            foreach (DocsListItem item in _block.Items)
            {
                if (!string.IsNullOrEmpty(item.DoneWhen))
                {
                    return false;
                }
            }
            return true;
        }

        public static VisualElement Create(DocsBlock _block, DocsRenderContext _context)
        {
            VisualElement flow = new VisualElement();
            flow.AddToClassList("docs-flow");
            int number = 1;
            foreach (DocsListItem item in _block.Items)
            {
                bool isPhase = item.NestedList != null && item.NestedList.Ordered;
                if (!isPhase)
                {
                    flow.Add(Step(number, DocsText.Create(item.Text, null, _context)));
                    number++;
                    continue;
                }
                VisualElement phase = new VisualElement();
                phase.AddToClassList("docs-flow-phase");
                phase.Add(DocsText.Create(item.Text, "docs-flow-phase-title", _context));
                foreach (DocsListItem step in item.NestedList.Items)
                {
                    phase.Add(Step(number, DocsText.Create(step.Text, null, _context)));
                    number++;
                }
                flow.Add(phase);
            }
            return flow;
        }

        private static VisualElement Step(int _number, VisualElement _content)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-flow-step");
            Label circle = new Label(_number.ToString());
            circle.AddToClassList("docs-flow-number");
            row.Add(circle);
            _content.AddToClassList("docs-flow-text");
            row.Add(_content);
            return row;
        }
    }

    /// <summary>A foldout that stays open for the rest of the editor session once opened. (New layout only.)</summary>
    public static class DocsSessionFoldout
    {
        public static Foldout Create(string _title, string _pagePath)
        {
            string key = "scene-docs-foldout:" + _pagePath + "#" + _title;
            Foldout foldout = new Foldout();
            foldout.text = _title;
            foldout.value = SessionState.GetBool(key, false);
            foldout.RegisterValueChangedCallback(_change => SessionState.SetBool(key, _change.newValue));
            foldout.AddToClassList("docs-foldout");
            foldout.AddToClassList("docs-fold");
            return foldout;
        }
    }
}
