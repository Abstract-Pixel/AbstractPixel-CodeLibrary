using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The design decisions list: title and first paragraph of each ADR, linked to it as a page.</summary>
    public static class SceneDocsReference
    {
        /// <summary>"Amended" when the decision has an "Amended" section, otherwise "Accepted".</summary>
        public static string StatusOf(string _text)
        {
            foreach (string raw in (_text ?? "").Replace("\r\n", "\n").Split('\n'))
            {
                if (raw.Trim().StartsWith("## Amended"))
                {
                    return "Amended";
                }
            }
            return "Accepted";
        }

        /// <summary>The new-layout list: one row per decision with its number, title and status. The title is the decision.</summary>
        public static VisualElement DecisionList(System.Action<string> _open)
        {
            VisualElement list = new VisualElement();
            list.AddToClassList("docs-decision-list");
            foreach (string path in SceneDocsTopics.AdrPaths())
            {
                string text = DocsFiles.Read(path) ?? "";
                string title;
                string firstParagraph;
                ReadAdr(text, out title, out firstParagraph);
                string topic = SceneDocsTopics.TopicFor(path);
                Button row = new Button(() => _open(topic));
                row.tooltip = "Opens this design decision";
                row.AddToClassList("docs-decision-row");
                Label number = new Label(topic.Substring(SceneDocsTopics.AdrPrefix.Length).TrimStart('0'));
                number.AddToClassList("docs-decision-number");
                row.Add(number);
                Label name = new Label(title);
                name.AddToClassList("docs-decision-title");
                row.Add(name);
                row.Add(StatusPill(StatusOf(text)));
                list.Add(row);
            }
            return list;
        }

        /// <summary>The line under a decision's title: "Design decision 3" and its status.</summary>
        public static VisualElement DecisionHeader(string _pagePath)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-decision-header");
            string topic = _pagePath.EndsWith(".md") ? _pagePath.Substring(0, _pagePath.Length - 3) : _pagePath;
            string number = topic.StartsWith(SceneDocsTopics.AdrPrefix) ? topic.Substring(SceneDocsTopics.AdrPrefix.Length).TrimStart('0') : "";
            Label caption = new Label("Design decision " + number);
            caption.AddToClassList("docs-decision-caption");
            row.Add(caption);
            string file = SceneDocsTopics.FileFor(topic);
            row.Add(StatusPill(StatusOf(file == null ? "" : DocsFiles.Read(file))));
            return row;
        }

        private static Label StatusPill(string _status)
        {
            Label pill = new Label(_status);
            pill.AddToClassList("docs-pill");
            pill.AddToClassList("docs-status");
            pill.AddToClassList("docs-status-" + _status.ToLowerInvariant());
            return pill;
        }

        public static void ReadAdr(string _text, out string _title, out string _firstParagraph)
        {
            _title = "";
            _firstParagraph = "";
            string[] lines = _text.Replace("\r\n", "\n").Split('\n');
            List<string> paragraph = new List<string>();
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.StartsWith("# ") && _title.Length == 0)
                {
                    _title = line.Substring(2).Trim();
                    continue;
                }
                if (_title.Length == 0 || line.StartsWith("#") || line.StartsWith("<!--"))
                {
                    continue;
                }
                if (line.Length == 0)
                {
                    if (paragraph.Count > 0)
                    {
                        break;
                    }
                    continue;
                }
                paragraph.Add(line);
            }
            _firstParagraph = string.Join(" ", paragraph);
        }
    }
}
