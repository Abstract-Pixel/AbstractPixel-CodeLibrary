using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The toolbar's history, on its right: "History  Sample 06 › Start-up › Loading scene", oldest first and the
    /// page you are on last. Each earlier page is a link back to it.
    /// </summary>
    public class DocsTrail
    {
        public const int MaxPages = 7;
        public readonly VisualElement Root = new VisualElement();
        private readonly Action<string> navigate;

        public DocsTrail(Action<string> _navigate)
        {
            navigate = _navigate;
            Root.AddToClassList("docs-trail");
        }

        /// <summary>Redraws the trail from the recent topics (oldest first) and the current page's place in the contents.</summary>
        public void Refresh(List<string> _recentTopics, string _whereCurrentSits)
        {
            Root.Clear();
            Label lead = new Label("History");
            lead.AddToClassList("docs-trail-lead");
            Root.Add(lead);
            List<string> pages = _recentTopics.FindAll(_topic => TitleOf(_topic) != null);
            for (int index = 0; index < pages.Count; index++)
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
                    Label current = new Label(TitleOf(topic));
                    current.tooltip = _whereCurrentSits;
                    current.AddToClassList("docs-trail-current");
                    Root.Add(current);
                    continue;
                }
                Button link = new Button(() => navigate(topic));
                link.text = TitleOf(topic);
                link.tooltip = "Back to " + link.text;
                link.AddToClassList("docs-trail-link");
                Root.Add(link);
            }
        }

        private static string TitleOf(string _topic)
        {
            string page;
            string anchor;
            SceneDocsTopics.Split(_topic, out page, out anchor);
            DocsPage parsed = SceneDocsPageCache.Get(page);
            return parsed == null || string.IsNullOrEmpty(parsed.Title) ? null : parsed.Title;
        }
    }
}
