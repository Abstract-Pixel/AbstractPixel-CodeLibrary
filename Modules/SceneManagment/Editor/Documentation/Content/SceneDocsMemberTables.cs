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
            Type type = SceneDocsMembers.FindType(_fullName);
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
            foreach (SceneDocsMember member in _members)
            {
                VisualElement row = Row(member, _type, _context);
                row.EnableInClassList("docs-mrow-alt", table.childCount % 2 == 1);
                table.Add(row);
            }
            section.Add(table);
            return section;
        }

        private static VisualElement Row(SceneDocsMember _member, Type _type, DocsRenderContext _context)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-mrow");
            row.AddToClassList("docs-kind-" + _member.Kind);
            string anchor = SceneDocsMember.AnchorFor(_member.Name);
            if (!_context.HeadingRows.ContainsKey(anchor))
            {
                _context.HeadingRows[anchor] = row;
            }
            Label typeLabel = RichLabel(TypeRich(_member.TypeText), "docs-mrow-type", _context);
            row.Add(typeLabel);
            VisualElement main = new VisualElement();
            main.AddToClassList("docs-mrow-main");
            main.Add(RichLabel(Signature(_member), "docs-mrow-signature", _context));
            if (_member.Summary.Length > 0)
            {
                main.Add(DocsText.Create(_member.Summary, "docs-mrow-summary", _context));
            }
            if (_member.Remarks.Count > 0)
            {
                Foldout more = DocsSessionFoldout.Create("More", _context.Page.Path + "/" + _member.Name);
                more.AddToClassList("docs-mrow-more");
                foreach (string remark in _member.Remarks)
                {
                    VisualElement line = new VisualElement();
                    line.AddToClassList("docs-list-row");
                    Label marker = new Label("•");
                    marker.AddToClassList("docs-list-marker");
                    line.Add(marker);
                    line.Add(DocsText.Create(remark, "docs-list-content", _context));
                    more.Add(line);
                }
                row.Add(main);
                // Full width under the type and the signature, not indented.
                row.Add(more);
                return row;
            }
            row.Add(main);
            return row;
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
            DocsLinkEvents.Register(label, _target => host.OpenLink(_target, pagePath), host.TooltipFor);
            return label;
        }

        // public static ChangeTo(SceneGroup _sceneGroup)   or   public static LoadingProgress { get; }
        private static string Signature(SceneDocsMember _member)
        {
            StringBuilder text = new StringBuilder();
            if (_member.Modifiers.Count > 0)
            {
                text.Append("<color=").Append(DocsTheme.Keyword).Append(">").Append(string.Join(" ", _member.Modifiers)).Append("</color> ");
            }
            text.Append("<b>").Append(_member.Name).Append("</b>");
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
                if (SceneDocsTypeIndex.TypeNamed(value) != null)
                {
                    text.Append("<link=\"").Append(SceneDocsTypeIndex.Scheme).Append(value).Append("\"><color=").Append(DocsTheme.Link).Append(">").Append(value).Append("</color></link>");
                    continue;
                }
                text.Append("<color=").Append(DocsTheme.TypeName).Append(">").Append(value).Append("</color>");
            }
            return text.ToString();
        }
    }
}
