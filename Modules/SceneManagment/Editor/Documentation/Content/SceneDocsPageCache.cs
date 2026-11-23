using System;
using System.Collections.Generic;
using System.IO;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Parsed pages by topic, read again only when the file changes on disk.</summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsPageCache
    {
        private class Entry
        {
            public DateTime WriteTime;
            public DocsPage Page;
        }

        private static readonly Dictionary<string, Entry> pages = new Dictionary<string, Entry>();

        /// <summary>The parsed page of a topic (no anchor), or null when it has no file.</summary>
        public static DocsPage Get(string _page)
        {
            string assetPath = SceneDocsTopics.FileFor(_page);
            if (assetPath == null)
            {
                return null;
            }
            string physical = SceneDocsPaths.Physical(assetPath);
            DateTime writeTime = File.GetLastWriteTimeUtc(physical);
            Entry cached;
            bool isFresh = pages.TryGetValue(_page, out cached) && cached.WriteTime == writeTime;
            if (isFresh)
            {
                return cached.Page;
            }
            string text = File.ReadAllText(physical);
            DocsPage page = MarkdownParser.Parse(text, _page + ".md", SceneDocsDirectives.Known);
            pages[_page] = new Entry { WriteTime = writeTime, Page = page };
            foreach (string problem in page.Problems)
            {
                UnityEngine.Debug.LogWarning("[Scene documentation] " + problem);
            }
            return page;
        }

        /// <summary>The asset path behind a parsed page's path ("guide/preload.md").</summary>
        public static string AssetPathOf(string _pagePath)
        {
            string topic = _pagePath.EndsWith(".md") ? _pagePath.Substring(0, _pagePath.Length - 3) : _pagePath;
            return SceneDocsTopics.FileFor(topic);
        }
    }
}
