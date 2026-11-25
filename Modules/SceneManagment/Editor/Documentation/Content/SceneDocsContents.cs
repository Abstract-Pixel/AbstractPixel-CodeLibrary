using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Pages/Contents.md: the sidebar tree (a list of links under "## Guide", "## Reference" and so on) and, under
    /// "## Terms", the page each glossary term opens ("- scene-group: guide/scene-groups-and-scene-sets.md#scene-groups").
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsContents
    {
        public const string TermsSection = "Terms";
        private static readonly Regex LinkItem = new Regex(@"^\s*-\s+\[(.+?)\]\((.+?)\)\s*$");
        private static readonly Regex TermItem = new Regex(@"^\s*-\s+([a-z0-9-]+):\s+(\S+)\s*$");

        /// <summary>The top-level tree: Home first, then one entry per section with its pages.</summary>
        public static List<DocsContentsEntry> Tree()
        {
            List<DocsContentsEntry> roots = new List<DocsContentsEntry>();
            string text = DocsFiles.Read(SceneDocsPaths.Contents) ?? "";
            DocsContentsEntry section = null;
            DocsContentsEntry category = null;
            foreach (string rawLine in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = rawLine.TrimEnd();
                if (line.StartsWith("## "))
                {
                    string title = line.Substring(3).Trim();
                    section = title == TermsSection ? null : new DocsContentsEntry(title, "");
                    category = null;
                    if (section != null)
                    {
                        roots.Add(section);
                    }
                    continue;
                }
                // "### Scene loaders" groups the links under it inside its section, as a folder in the tree.
                if (line.StartsWith("### ") && section != null)
                {
                    category = new DocsContentsEntry(line.Substring(4).Trim(), "");
                    section.Children.Add(category);
                    continue;
                }
                Match link = LinkItem.Match(line);
                if (!link.Success)
                {
                    continue;
                }
                string topic = TopicOfLink(link.Groups[2].Value, SceneDocsPaths.Contents);
                DocsContentsEntry entry = new DocsContentsEntry(link.Groups[1].Value, topic ?? "");
                if (section == null)
                {
                    roots.Add(entry);
                    continue;
                }
                (category ?? section).Children.Add(entry);
            }
            return roots;
        }

        /// <summary>Every link target in the tree, flattened (for tests and the pager).</summary>
        public static List<DocsContentsEntry> Flatten()
        {
            List<DocsContentsEntry> all = new List<DocsContentsEntry>();
            foreach (DocsContentsEntry root in Tree())
            {
                AddAll(root, all);
            }
            return all;
        }

        private static void AddAll(DocsContentsEntry _entry, List<DocsContentsEntry> _into)
        {
            if (!string.IsNullOrEmpty(_entry.Topic))
            {
                _into.Add(_entry);
            }
            foreach (DocsContentsEntry child in _entry.Children)
            {
                AddAll(child, _into);
            }
        }

        /// <summary>Term slug to the topic id its page section has.</summary>
        public static Dictionary<string, string> TermTopics()
        {
            Dictionary<string, string> topics = new Dictionary<string, string>();
            string text = DocsFiles.Read(SceneDocsPaths.Contents) ?? "";
            bool inTerms = false;
            foreach (string rawLine in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = rawLine.TrimEnd();
                if (line.StartsWith("## "))
                {
                    inTerms = line.Substring(3).Trim() == TermsSection;
                    continue;
                }
                Match term = TermItem.Match(line);
                if (inTerms && term.Success)
                {
                    topics[term.Groups[1].Value] = TopicOfLink(term.Groups[2].Value, SceneDocsPaths.Contents);
                }
            }
            return topics;
        }

        /// <summary>The topic id of a relative .md link written in a file ("../guide/preload.md#x"), or null.</summary>
        public static string TopicOfLink(string _link, string _fromAssetPath)
        {
            string decoded = System.Uri.UnescapeDataString(_link);
            int hash = decoded.IndexOf('#');
            string path = hash < 0 ? decoded : decoded.Substring(0, hash);
            string anchor = hash < 0 ? "" : decoded.Substring(hash);
            if (path.Length == 0)
            {
                string own = SceneDocsTopics.TopicFor(_fromAssetPath);
                return own == null ? null : own + anchor;
            }
            string target = DocsFiles.Combine(DocsFiles.FolderOf(_fromAssetPath), path);
            string topic = SceneDocsTopics.TopicFor(target);
            return topic == null ? null : topic + anchor;
        }
    }
}
