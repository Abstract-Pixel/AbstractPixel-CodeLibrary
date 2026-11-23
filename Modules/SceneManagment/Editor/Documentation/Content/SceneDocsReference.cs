using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The design decisions list: title and first paragraph of each ADR, linked to it as a page.</summary>
    public static class SceneDocsReference
    {
        /// <summary>Title and first paragraph of each design decision, linked to the decision as a page.</summary>
        public static VisualElement AdrList(System.Action<string> _open)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-adr-list");
            foreach (string path in SceneDocsTopics.AdrPaths())
            {
                string text = SceneDocsPaths.Read(path) ?? "";
                string title;
                string firstParagraph;
                ReadAdr(text, out title, out firstParagraph);
                string topic = SceneDocsTopics.TopicFor(path);
                VisualElement row = new VisualElement();
                row.AddToClassList("docs-member");
                Button link = new Button(() => _open(topic));
                link.text = title;
                link.tooltip = "Opens this design decision";
                link.AddToClassList("docs-link-button");
                row.Add(link);
                Label summary = new Label(DocsInline.ToPlainText(firstParagraph));
                summary.AddToClassList("docs-text");
                summary.AddToClassList("docs-member-summary");
                row.Add(summary);
                box.Add(row);
            }
            return box;
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
                if (_title.Length == 0 || line.StartsWith("#"))
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
