using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// Draws a <see cref="DocsGraph"/> like the save system's code map: boxes placed by <see cref="DocsGraphLayout"/>,
    /// curved arrows drawn with Painter2D behind them, and hover tracing (the hovered box's arrows stand out, the rest
    /// fade). A box with an inner graph opens it (crumbs lead back); any other box runs its action.
    /// </summary>
    public class DocsGraphView : VisualElement
    {
        private readonly List<DocsGraph> path = new List<DocsGraph>();
        private readonly VisualElement crumbs;
        private readonly VisualElement canvas;
        private readonly Dictionary<string, VisualElement> boxes = new Dictionary<string, VisualElement>();
        private Dictionary<string, Rect> rects = new Dictionary<string, Rect>();
        private readonly VisualElement everything;
        private readonly Toggle showEverything;
        private string hovered;
        private float laidOutWidth = -1f;

        public DocsGraphView(DocsGraph _graph)
        {
            AddToClassList("docs-graph");
            crumbs = new VisualElement();
            crumbs.AddToClassList("docs-graph-crumbs");
            Add(crumbs);
            canvas = new VisualElement();
            canvas.AddToClassList("docs-graph-canvas");
            canvas.generateVisualContent += DrawEdges;
            Add(canvas);
            Label hint = new Label("Hover a box to trace its links. A box marked › opens its own map.");
            hint.AddToClassList("docs-caption");
            Add(hint);
            Add(Legend());
            // Layers deep: the map shows the core boxes first; this toggle lays every box's own map out underneath.
            showEverything = new Toggle("Show everything");
            showEverything.tooltip = "Shows the map inside each box under this one";
            showEverything.AddToClassList("docs-graph-everything-toggle");
            showEverything.RegisterValueChangedCallback(_change => RefreshEverything());
            Add(showEverything);
            everything = new VisualElement();
            everything.AddToClassList("docs-graph-everything");
            Add(everything);
            path.Add(_graph);
            RegisterCallback<GeometryChangedEvent>(_event => Relayout(false));
            BuildCrumbs();
            RefreshEverything();
        }

        public DocsGraph Current
        {
            get { return path[path.Count - 1]; }
        }

        public void Open(DocsGraph _graph)
        {
            path.Add(_graph);
            BuildCrumbs();
            Relayout(true);
            RefreshEverything();
        }

        // Each box with a map of its own gets that map under this one, titled, when "Show everything" is on.
        private void RefreshEverything()
        {
            everything.Clear();
            bool hasInner = Current.Nodes.Exists(_node => _node.Inner != null);
            showEverything.EnableInClassList("docs-hidden", !hasInner);
            if (!showEverything.value || !hasInner)
            {
                return;
            }
            foreach (DocsGraphNode node in Current.Nodes)
            {
                if (node.Inner == null)
                {
                    continue;
                }
                VisualElement section = new VisualElement();
                section.AddToClassList("docs-graph-section");
                section.Add(new DocsGraphView(node.Inner));
                everything.Add(section);
            }
        }

        private void BuildCrumbs()
        {
            crumbs.Clear();
            for (int index = 0; index < path.Count; index++)
            {
                if (index > 0)
                {
                    Label separator = new Label("›");
                    separator.AddToClassList("docs-graph-separator");
                    crumbs.Add(separator);
                }
                int depth = index;
                bool isLast = index == path.Count - 1;
                Button crumb = new Button(() => GoBackTo(depth));
                crumb.text = path[index].Title;
                crumb.tooltip = isLast ? "The map shown now" : "Back to " + path[index].Title;
                crumb.AddToClassList("docs-link-button");
                crumb.EnableInClassList("docs-graph-crumb-current", isLast);
                crumb.SetEnabled(!isLast);
                crumbs.Add(crumb);
            }
        }

        private void GoBackTo(int _depth)
        {
            path.RemoveRange(_depth + 1, path.Count - _depth - 1);
            BuildCrumbs();
            Relayout(true);
            RefreshEverything();
        }

        // Places the boxes for the current width. Only a real width change (or a new graph) rebuilds.
        private void Relayout(bool _force)
        {
            float width = contentRect.width;
            if (float.IsNaN(width) || width <= 0f)
            {
                return;
            }
            if (!_force && Mathf.Abs(width - laidOutWidth) < 1f)
            {
                return;
            }
            laidOutWidth = width;
            Vector2 size;
            rects = DocsGraphLayout.Place(Current, width, out size);
            canvas.Clear();
            boxes.Clear();
            hovered = null;
            foreach (DocsGraphNode node in Current.Nodes)
            {
                VisualElement box = Box(node, rects[node.Id]);
                boxes[node.Id] = box;
                canvas.Add(box);
            }
            // Positions come from the layout, which USS cannot express.
            canvas.style.width = size.x;
            canvas.style.height = size.y;
            canvas.MarkDirtyRepaint();
        }

        private VisualElement Box(DocsGraphNode _node, Rect _rect)
        {
            Button box = new Button(() => OnClicked(_node));
            box.AddToClassList("docs-graph-box");
            box.EnableInClassList("docs-graph-key", _node.IsKey);
            if (!string.IsNullOrEmpty(_node.Category))
            {
                box.AddToClassList("docs-graph-cat-" + _node.Category);
            }
            box.style.left = _rect.x;
            box.style.top = _rect.y;
            box.style.width = _rect.width;
            box.style.height = _rect.height;
            Label title = new Label(_node.Inner != null ? _node.Title + "  ›" : _node.Title);
            title.AddToClassList("docs-graph-title");
            box.Add(title);
            if (!string.IsNullOrEmpty(_node.Line))
            {
                Label line = new Label(_node.Line);
                line.AddToClassList("docs-graph-line");
                box.Add(line);
            }
            string action = _node.Inner != null ? "Shows how the parts of " + _node.Title + " connect" : "Opens " + _node.Title;
            // The tooltip carries the whole line too, in case a long one still runs past two lines.
            box.tooltip = string.IsNullOrEmpty(_node.Line) ? action : _node.Line + "\n" + action;
            // A box that opens a script says so: the Visual Studio icon shows on hover, and the tooltip names the editor.
            bool opensScript = _node.Inner == null && _node.Clicked != null;
            if (opensScript)
            {
                VisualElement opensOutside = DocsCodeIcons.OpenInIde();
                opensOutside.AddToClassList("docs-graph-open-icon");
                box.Add(opensOutside);
                string line = string.IsNullOrEmpty(_node.Line) ? "" : _node.Line + "\n";
                box.tooltip = line + "Opens " + _node.Title + ".cs in your code editor, outside the docs.";
            }
            box.RegisterCallback<PointerEnterEvent>(_event => SetHovered(_node.Id));
            box.RegisterCallback<PointerLeaveEvent>(_event => SetHovered(null));
            box.RegisterCallback<FocusInEvent>(_event => SetHovered(_node.Id));
            box.RegisterCallback<FocusOutEvent>(_event => SetHovered(null));
            return box;
        }

        private void OnClicked(DocsGraphNode _node)
        {
            if (_node.Inner != null)
            {
                Open(_node.Inner);
                return;
            }
            if (_node.Clicked != null)
            {
                _node.Clicked();
            }
        }

        private void SetHovered(string _id)
        {
            hovered = _id;
            HashSet<string> linked = new HashSet<string>();
            if (_id != null)
            {
                linked.Add(_id);
                foreach (KeyValuePair<string, string> edge in Current.Edges)
                {
                    if (edge.Key == _id) { linked.Add(edge.Value); }
                    if (edge.Value == _id) { linked.Add(edge.Key); }
                }
            }
            foreach (KeyValuePair<string, VisualElement> box in boxes)
            {
                bool dim = _id != null && !linked.Contains(box.Key);
                box.Value.EnableInClassList("docs-graph-dim", dim);
            }
            canvas.MarkDirtyRepaint();
        }

        private void DrawEdges(MeshGenerationContext _context)
        {
            Painter2D painter = _context.painter2D;
            Color baseColour = resolvedStyle.color;
            foreach (KeyValuePair<string, string> edge in Current.Edges)
            {
                Rect from;
                Rect to;
                if (!rects.TryGetValue(edge.Key, out from) || !rects.TryGetValue(edge.Value, out to))
                {
                    continue;
                }
                bool isTraced = hovered != null && (edge.Key == hovered || edge.Value == hovered);
                bool isFaded = hovered != null && !isTraced;
                float alpha = isTraced ? 0.95f : (isFaded ? 0.12f : 0.5f);
                painter.strokeColor = new Color(baseColour.r, baseColour.g, baseColour.b, alpha);
                painter.fillColor = painter.strokeColor;
                painter.lineWidth = isTraced ? 2f : 1.25f;
                DrawArrow(painter, from, to);
            }
        }

        // Side to side when the target is to the right; top to bottom when it is below (narrow layout).
        private static void DrawArrow(Painter2D _painter, Rect _from, Rect _to)
        {
            bool goesRight = _to.xMin >= _from.xMax - 1f;
            Vector2 start;
            Vector2 end;
            Vector2 c1;
            Vector2 c2;
            Vector2 direction;
            if (goesRight)
            {
                start = new Vector2(_from.xMax, _from.center.y);
                end = new Vector2(_to.xMin - 2f, _to.center.y);
                float bend = (end.x - start.x) * 0.5f;
                c1 = new Vector2(start.x + bend, start.y);
                c2 = new Vector2(end.x - bend, end.y);
                direction = Vector2.right;
            }
            else
            {
                bool goesDown = _to.yMin >= _from.yMax - 1f;
                start = goesDown ? new Vector2(_from.center.x, _from.yMax) : new Vector2(_from.center.x, _from.yMin);
                end = goesDown ? new Vector2(_to.center.x, _to.yMin - 2f) : new Vector2(_to.center.x, _to.yMax + 2f);
                float bend = (end.y - start.y) * 0.5f;
                c1 = new Vector2(start.x, start.y + bend);
                c2 = new Vector2(end.x, end.y - bend);
                direction = goesDown ? Vector2.up * -1f : Vector2.up;
                direction = goesDown ? new Vector2(0f, 1f) : new Vector2(0f, -1f);
            }
            _painter.BeginPath();
            _painter.MoveTo(start);
            _painter.BezierCurveTo(c1, c2, end);
            _painter.Stroke();
            ArrowHead(_painter, end, direction);
        }

        private static void ArrowHead(Painter2D _painter, Vector2 _tip, Vector2 _direction)
        {
            Vector2 side = new Vector2(-_direction.y, _direction.x);
            Vector2 back = _tip - _direction * 6f;
            _painter.BeginPath();
            _painter.MoveTo(_tip);
            _painter.LineTo(back + side * 3.5f);
            _painter.LineTo(back - side * 3.5f);
            _painter.ClosePath();
            _painter.Fill();
        }

        // What the edge colours of script boxes mean.
        private static VisualElement Legend()
        {
            VisualElement legend = new VisualElement();
            legend.AddToClassList("docs-graph-legend");
            Label lead = new Label("Script boxes:");
            lead.AddToClassList("docs-caption");
            legend.Add(lead);
            AddKey(legend, "api", "Public API");
            AddKey(legend, "interface", "Interface");
            AddKey(legend, "enum", "Enum");
            AddKey(legend, "editor", "Editor");
            AddKey(legend, "other", "Other");
            return legend;
        }

        private static void AddKey(VisualElement _legend, string _category, string _text)
        {
            VisualElement swatch = new VisualElement();
            swatch.AddToClassList("docs-graph-swatch");
            swatch.AddToClassList("docs-graph-cat-" + _category);
            _legend.Add(swatch);
            Label label = new Label(_text);
            label.AddToClassList("docs-caption");
            _legend.Add(label);
        }
    }
}
