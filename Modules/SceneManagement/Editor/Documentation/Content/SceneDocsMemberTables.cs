using AbstractPixel.EditorModules.Documentation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A type page's members in the new layout: one headed table per kind (Properties, Fields, Methods, Events,
    /// Values). Each row shows the type on the left, then the modifiers, name and parameters, the one-line summary
    /// under them, and the remarks in a "More" foldout. Rows are colour-coded by kind and every documented type links.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsMemberTables
    {
        private static readonly string[] Kinds = { "property", "field", "method", "event", "value" };
        private static readonly string[] Titles = { "Properties", "Fields", "Methods", "Events", "Values" };
        private static readonly Regex Tokens = new Regex(@"[A-Za-z_]\w*|[^A-Za-z_]+");
        private static readonly HashSet<string> Keywords = new HashSet<string> { "void", "bool", "int", "float", "double", "long", "string", "object", "byte", "char", "uint", "short", "out", "ref", "params", "null", "true", "false" };

        public static VisualElement Render(string _fullName, DocsRenderContext _context)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-member-tables");
            Type type = DocsTypeFinder.FindType(_fullName);
            if (type == null)
            {
                box.Add(new Label("No loaded type named " + _fullName));
                return box;
            }
            List<SceneDocsMember> members = SceneDocsMemberReader.Read(type);
            for (int index = 0; index < Kinds.Length; index++)
            {
                List<SceneDocsMember> ofKind = members.FindAll(_member => _member.Kind == Kinds[index]);
                if (ofKind.Count > 0)
                {
                    box.Add(Section(Titles[index], Kinds[index], ofKind, type, _context));
                }
            }
            return box;
        }

        private static VisualElement Section(string _title, string _kind, List<SceneDocsMember> _members, Type _type, DocsRenderContext _context)
        {
            VisualElement section = new VisualElement();
            section.AddToClassList("docs-section");
            section.AddToClassList("docs-member-section");
            string id = _title.ToLowerInvariant();
            _context.SectionCount++;
            VisualElement heading = DocsRenderer.HeadingRow(_context.SectionCount + ".  " + _title, id, 2, -1, _context);
            heading.AddToClassList("docs-kind-" + _kind);
            Label count = new Label(_members.Count.ToString());
            count.AddToClassList("docs-member-count");
            heading.Insert(1, count);
            section.Add(heading);
            if (!_context.Page.HasHeading(id))
            {
                // After the headings drawn so far, so On this page keeps the page order.
                int drawn = _context.Page.Headings.FindAll(_heading => _context.HeadingRows.ContainsKey(_heading.Id)).Count;
                _context.Page.Headings.Insert(drawn, new DocsHeading(id, _title, 2, -1));
            }
            VisualElement table = new VisualElement();
            table.AddToClassList("docs-member-table");
            int number = 0;
            foreach (SceneDocsMember member in _members)
            {
                number++;
                VisualElement row = Row(member, _type, number, _context);
                row.EnableInClassList("docs-mrow-alt", table.childCount % 2 == 1);
                table.Add(row);
            }
            if (table.childCount > 0)
            {
                table[table.childCount - 1].AddToClassList("docs-mrow-last");
            }
            section.Add(table);
            MakeFoldable(heading, table, _context.Page.Path + "#members-" + id);
            return section;
        }

        // The heading opens and closes its list; closed by default, and remembered for the session like other foldouts.
        private static void MakeFoldable(VisualElement _heading, VisualElement _table, string _key)
        {
            Label arrow = new Label();
            arrow.AddToClassList("docs-member-arrow");
            _heading.Insert(0, arrow);
            _heading.AddToClassList("docs-member-heading");
            _table.userData = arrow;
            bool isOpen = UnityEditor.SessionState.GetBool(_key, false);
            SetOpen(_table, isOpen);
            _heading.RegisterCallback<ClickEvent>(_click =>
            {
                bool open = _table.style.display == DisplayStyle.None;
                SetOpen(_table, open);
                UnityEditor.SessionState.SetBool(_key, open);
            });
        }

        /// <summary>Opens or closes a member list; a link to one of its members opens it first.</summary>
        public static void SetOpen(VisualElement _table, bool _isOpen)
        {
            _table.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            Label arrow = _table.userData as Label;
            if (arrow != null)
            {
                arrow.text = _isOpen ? "▾" : "▸";
            }
        }

        // One line: fold arrow, "1.", return type and signature. The summary sits under it; the arrow opens a box
        // with the remarks (More), so no column or row is spent on them while closed.
        private static VisualElement Row(SceneDocsMember _member, Type _type, int _number, DocsRenderContext _context)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-mrow");
            row.AddToClassList("docs-kind-" + _member.Kind);
            string anchor = SceneDocsMember.AnchorFor(_member.Name);
            if (!_context.HeadingRows.ContainsKey(anchor))
            {
                _context.HeadingRows[anchor] = row;
            }
            // An enum value is a name and what it means: the name takes the type column, the meaning sits beside it.
            bool isValue = _member.Kind == "value";
            if (isValue)
            {
                return ValueRow(row, _member, _type, _context);
            }
            row.AddToClassList("docs-mrow-line-layout");

            VisualElement line = new VisualElement();
            line.AddToClassList("docs-mrow-line");
            Label arrow = new Label(_member.Remarks.Count > 0 ? "▸" : "");
            arrow.AddToClassList("docs-mrow-arrow");
            line.Add(arrow);
            Label number = new Label(_number + ".");
            number.AddToClassList("docs-mrow-number");
            line.Add(number);
            line.Add(RichLabel(Signature(_member, true), "docs-mrow-signature", _context));
            row.Add(line);

            if (_member.Summary.Length > 0)
            {
                row.Add(DocsText.Create(_member.Summary, "docs-mrow-summary", _context));
            }
            if (_member.Remarks.Count > 0)
            {
                VisualElement more = MoreBox(_member, _context);
                row.Add(more);
                string key = _context.Page.Path + "/" + _member.Name;
                bool isOpen = UnityEditor.SessionState.GetBool(key, false);
                ShowMore(arrow, more, isOpen);
                line.RegisterCallback<ClickEvent>(_click =>
                {
                    bool open = more.style.display == DisplayStyle.None;
                    ShowMore(arrow, more, open);
                    UnityEditor.SessionState.SetBool(key, open);
                });
                line.AddToClassList("docs-mrow-line-foldable");
            }
            return row;
        }

        // The remarks in one inset box with a "More" header row.
        private static VisualElement MoreBox(SceneDocsMember _member, DocsRenderContext _context)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-mrow-morebox");
            Label header = new Label("More");
            header.AddToClassList("docs-mrow-morebox-header");
            box.Add(header);
            VisualElement body = new VisualElement();
            body.AddToClassList("docs-mrow-morebox-body");
            foreach (string remark in _member.Remarks)
            {
                VisualElement remarkLine = new VisualElement();
                remarkLine.AddToClassList("docs-list-row");
                Label marker = new Label("•");
                marker.AddToClassList("docs-list-marker");
                remarkLine.Add(marker);
                remarkLine.Add(DocsText.Create(remark, "docs-list-content", _context));
                body.Add(remarkLine);
            }
            box.Add(body);
            return box;
        }

        private static void ShowMore(Label _arrow, VisualElement _more, bool _isOpen)
        {
            _more.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            _arrow.text = _isOpen ? "▾" : "▸";
        }

        private static Label RichLabel(string _rich, string _className, DocsRenderContext _context)
        {
            Label label = new Label(_rich);
            label.enableRichText = true;
            label.selection.isSelectable = true;
            label.AddToClassList("docs-text");
            label.AddToClassList(_className);
            DocsCodeBlock.ApplyMonoFont(label);
            string pagePath = _context.Page.Path;
            IDocsHost host = _context.Host;
            DocsLinkEvents.Register(label, _target => host.OpenLink(_target, pagePath), _target => host.LinkTooltip(_target, pagePath));
            return label;
        }

        // "Done | The scene change ran to its end.", written as Type.Value so it reads like code.
        private static VisualElement ValueRow(VisualElement _row, SceneDocsMember _member, Type _type, DocsRenderContext _context)
        {
            _row.AddToClassList("docs-mrow-value");
            string name = "<color=" + DocsTypeColours.ColourOf(_type) + ">" + _type.Name + "</color>.<b>" + _member.Name + "</b>";
            _row.Add(RichLabel(name, "docs-mrow-value-name", _context));
            VisualElement main = new VisualElement();
            main.AddToClassList("docs-mrow-main");
            string meaning = _member.Summary.Length > 0 ? _member.Summary : "No description in the source.";
            main.Add(DocsText.Create(meaning, "docs-mrow-summary", _context));
            _row.Add(main);
            return _row;
        }

        // public static ChangeTo(SceneGroup _sceneGroup)   or   public static LoadingProgress { get; }
        private static string Signature(SceneDocsMember _member)
        {
            return Signature(_member, false);
        }

        // With the type: "public static Task<SceneChangeResult> ChangeTo(SceneGroup _sceneGroup)", in C# order.
        private static string Signature(SceneDocsMember _member, bool _withType)
        {
            StringBuilder text = new StringBuilder();
            if (_member.Modifiers.Count > 0)
            {
                text.Append("<color=").Append(DocsTheme.Keyword).Append(">").Append(string.Join(" ", _member.Modifiers)).Append("</color> ");
            }
            if (_withType && !string.IsNullOrEmpty(_member.TypeText))
            {
                text.Append(TypeRich(_member.TypeText)).Append(" ");
            }
            // Methods get the IDE's method colour; properties, fields and events keep the plain text colour.
            bool isMethod = _member.Parameters != null;
            if (isMethod)
            {
                text.Append("<color=").Append(DocsTheme.Method).Append("><b>").Append(_member.Name).Append("</b></color>");
            }
            else
            {
                text.Append("<b>").Append(_member.Name).Append("</b>");
            }
            if (_member.Parameters != null)
            {
                text.Append("(").Append(ParametersRich(_member.Parameters)).Append(")");
            }
            if (!string.IsNullOrEmpty(_member.Accessors))
            {
                text.Append(" <color=").Append(DocsTheme.Keyword).Append(">{ ").Append(_member.Accessors).Append(" }</color>");
            }
            return text.ToString();
        }

        private static string ParametersRich(string _parameters)
        {
            if (_parameters.Length == 0)
            {
                return "";
            }
            List<string> parts = new List<string>();
            foreach (string parameter in _parameters.Split(new[] { ", " }, StringSplitOptions.None))
            {
                int space = parameter.LastIndexOf(' ');
                int equals = parameter.IndexOf(" = ", StringComparison.Ordinal);
                if (equals > 0)
                {
                    space = parameter.LastIndexOf(' ', equals - 1);
                }
                string typePart = space < 0 ? parameter : parameter.Substring(0, space);
                string rest = space < 0 ? "" : parameter.Substring(space);
                parts.Add(TypeRich(typePart) + "<noparse>" + rest + "</noparse>");
            }
            return string.Join(", ", parts);
        }

        /// <summary>A type as rich text: documented types link, keywords take the keyword colour, others the type colour.</summary>
        public static string TypeRich(string _typeText)
        {
            StringBuilder text = new StringBuilder();
            foreach (Match token in Tokens.Matches(_typeText ?? ""))
            {
                string value = token.Value;
                bool isWord = char.IsLetter(value[0]) || value[0] == '_';
                if (!isWord)
                {
                    text.Append("<noparse>").Append(value).Append("</noparse>");
                    continue;
                }
                if (Keywords.Contains(value))
                {
                    text.Append("<color=").Append(DocsTheme.Keyword).Append(">").Append(value).Append("</color>");
                    continue;
                }
                // Each type takes its kind's colour (an enum never looks like a class); a documented one is also a link.
                string kindColour = DocsTypeColours.For(value) ?? DocsTheme.Class;
                if (SceneDocsTypeIndex.TypeNamed(value) != null)
                {
                    text.Append("<link=\"").Append(SceneDocsTypeIndex.Scheme).Append(value).Append("\"><color=").Append(kindColour).Append(">").Append(value).Append("</color></link>");
                    continue;
                }
                text.Append("<color=").Append(kindColour).Append(">").Append(value).Append("</color>");
            }
            return text.ToString();
        }
    }
}
