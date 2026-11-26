using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// The toolbar's history, on its right: "History  4 earlier ▾ › Start-up › Loading scene", the page you are on last.
    /// </summary>
    /// <remarks>
    /// - The last few pages show inline, each a link back. Older ones (up to MaxPages in all) sit behind an "N earlier" button:
    ///   hovering it lists them, a click opens them as a menu. So a long history never overflows the toolbar.
    /// </remarks>
    public class DocsTrail
    {
        public const int MaxPages = 11;
        private const int ShownInline = 3;
        public readonly VisualElement Root = new VisualElement();
        private readonly Action<string> navigate;

        private readonly Func<string, string> titleOf;

        /// <summary>_titleOf names a topic's page, or returns null for a topic with no page.</summary>
        public DocsTrail(Action<string> _navigate, Func<string, string> _titleOf)
        {
            navigate = _navigate;
            titleOf = _titleOf;
            Root.AddToClassList("docs-trail");
        }

        /// <summary>Redraws the trail from the recent topics (oldest first) and the current page's place in the contents.</summary>
        public void Refresh(List<string> _recentTopics, string _whereCurrentSits)
        {
            Root.Clear();
            Label lead = new Label("History");
            lead.AddToClassList("docs-trail-lead");
            Root.Add(lead);
            List<string> pages = _recentTopics.FindAll(_topic => titleOf(_topic) != null);
            int firstInline = Math.Max(0, pages.Count - ShownInline);
            if (firstInline > 0)
            {
                Root.Add(Overflow(pages.GetRange(0, firstInline)));
            }
            for (int index = firstInline; index < pages.Count; index++)
            {
                string topic = pages[index];
                bool isCurrent = index == pages.Count - 1;
                if (index > 0)
                {
                    Label separator = new Label("›");
                    separator.AddToClassList("docs-trail-separator");
                    Root.Add(separator);
                }
                if (isCurrent)
                {
                    Label current = new Label(titleOf(topic));
                    current.tooltip = _whereCurrentSits;
                    current.AddToClassList("docs-trail-current");
                    Root.Add(current);
                    continue;
                }
                Button link = new Button(() => navigate(topic));
                link.text = titleOf(topic);
                link.tooltip = "Back to " + link.text;
                link.AddToClassList("docs-trail-link");
                Root.Add(link);
            }
        }

        // The older pages behind one icon: the tooltip lists them, newest first; a click offers them as a menu.
        private VisualElement Overflow(List<string> _older)
        {
            List<string> newestFirst = new List<string>(_older);
            newestFirst.Reverse();
            System.Text.StringBuilder list = new System.Text.StringBuilder("Earlier pages (click to open one):");
            foreach (string topic in newestFirst)
            {
                list.Append("\n• ").Append(titleOf(topic));
            }
            Button more = new Button();
            more.text = newestFirst.Count + " earlier ▾";
            more.tooltip = list.ToString();
            more.AddToClassList("docs-trail-link");
            more.AddToClassList("docs-trail-more");
            more.clicked += () =>
            {
                UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
                foreach (string topic in newestFirst)
                {
                    string target = topic;
                    menu.AddItem(new UnityEngine.GUIContent(titleOf(topic)), false, () => navigate(target));
                }
                menu.DropDown(more.worldBound);
            };
            return more;
        }
    }
}
