using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The search index: page titles, headings, glossary terms and type names, each with the topic it opens and its
    /// category. Built once per domain load, on the first search.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsSearch
    {
        public class Result
        {
            public string Text;
            public string Topic;
            public string Category;
        }

        private static List<Result> index;

        public static List<Result> Index()
        {
            if (index != null)
            {
                return index;
            }
            index = new List<Result>();
            foreach (DocsContentsEntry entry in SceneDocsContents.Flatten())
            {
                AddPage(entry);
            }
            foreach (KeyValuePair<string, string> term in SceneDocsContents.TermTopics())
            {
                KeyValuePair<string, string> glossaryEntry;
                string name = SceneGlossary.Terms.TryGetValue(term.Key, out glossaryEntry) ? glossaryEntry.Key : term.Key;
                index.Add(new Result { Text = name, Topic = term.Value, Category = "Glossary" });
            }
            return index;
        }

        private static void AddPage(DocsContentsEntry _entry)
        {
            string page;
            string anchor;
            DocsTopicId.Split(_entry.Topic, out page, out anchor);
            string category = page.StartsWith("types/") ? "Types" : page.StartsWith("samples") ? "Samples" : "Pages";
            index.Add(new Result { Text = _entry.Title, Topic = _entry.Topic, Category = category });
            DocsPage parsed = SceneDocsPageCache.Get(page);
            if (parsed == null)
            {
                return;
            }
            foreach (DocsHeading heading in parsed.Headings)
            {
                index.Add(new Result { Text = heading.Text + " (" + _entry.Title + ")", Topic = page + "#" + heading.Id, Category = "Headings" });
            }
        }

        /// <summary>Results whose text has every word of the query, titles and types first.</summary>
        public static List<Result> Find(string _query)
        {
            List<Result> found = new List<Result>();
            string[] words = (_query ?? "").ToLowerInvariant().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return found;
            }
            foreach (Result result in Index())
            {
                string text = result.Text.ToLowerInvariant();
                bool hasAll = true;
                foreach (string word in words)
                {
                    if (!text.Contains(word))
                    {
                        hasAll = false;
                        break;
                    }
                }
                if (hasAll)
                {
                    found.Add(result);
                }
            }
            found.Sort((_a, _b) => Rank(_a).CompareTo(Rank(_b)));
            return found;
        }

        private static int Rank(Result _result)
        {
            switch (_result.Category)
            {
                case "Pages": return 0;
                case "Types": return 1;
                case "Glossary": return 2;
                case "Samples": return 3;
                default: return 4;
            }
        }
    }
}
