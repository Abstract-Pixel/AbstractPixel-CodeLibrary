using System;
using System.Collections.Generic;
using System.IO;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Topic ids and the files behind them. A topic is a page path under Pages/ without ".md", lowercase ("guide/preload",
    /// "types/sceneactions", "home"); "samples" and "samples/05" are the sample READMEs; "adr/0003" is a design decision.
    /// An id may end in "#" and a heading id.
    /// </summary>
    public static class SceneDocsTopics
    {
        public const string SamplesTopic = "samples";
        public const string SamplePrefix = "samples/";
        public const string AdrPrefix = "adr/";

        /// <summary>Splits "guide/preload#minimum-time" into its page and its anchor (empty when none).</summary>
        public static void Split(string _topicId, out string _page, out string _anchor)
        {
            string id = (_topicId ?? "").Trim();
            int hash = id.IndexOf('#');
            _page = hash < 0 ? id : id.Substring(0, hash);
            _anchor = hash < 0 ? "" : id.Substring(hash + 1);
            _page = _page.ToLowerInvariant();
        }

        /// <summary>The asset path of a page topic (no anchor), or null when no file has it.</summary>
        public static string FileFor(string _page)
        {
            if (string.IsNullOrEmpty(_page))
            {
                return null;
            }
            if (_page == SamplesTopic)
            {
                return SceneDocsPaths.Samples + "README.md";
            }
            if (_page.StartsWith(SamplePrefix))
            {
                return SampleReadme(_page.Substring(SamplePrefix.Length));
            }
            if (_page.StartsWith(AdrPrefix))
            {
                return AdrFile(_page.Substring(AdrPrefix.Length));
            }
            foreach (string pagePath in AllPagePaths())
            {
                if (TopicFor(pagePath) == _page)
                {
                    return pagePath;
                }
            }
            return null;
        }

        /// <summary>The topic of a file, or null when the file is not part of the documentation.</summary>
        public static string TopicFor(string _assetPath)
        {
            string path = _assetPath.Replace('\\', '/');
            if (path.StartsWith(SceneDocsPaths.Pages) && path.EndsWith(".md"))
            {
                string relative = path.Substring(SceneDocsPaths.Pages.Length);
                return relative.Substring(0, relative.Length - 3).ToLowerInvariant();
            }
            if (path == SceneDocsPaths.Samples + "README.md")
            {
                return SamplesTopic;
            }
            if (path.StartsWith(SceneDocsPaths.Samples) && path.EndsWith("/README.md"))
            {
                string folder = path.Substring(SceneDocsPaths.Samples.Length).Split('/')[0];
                return SamplePrefix + folder.Split(' ')[0];
            }
            if (path.StartsWith(SceneDocsPaths.Adr) && path.EndsWith(".md"))
            {
                string name = Path.GetFileName(path);
                return AdrPrefix + name.Split('-')[0];
            }
            return null;
        }

        /// <summary>Every Markdown page under Pages/, as asset paths (pages starting with "_" included).</summary>
        public static List<string> AllPagePaths()
        {
            List<string> paths = new List<string>();
            string root = SceneDocsPaths.Physical(SceneDocsPaths.Pages);
            if (!Directory.Exists(root))
            {
                return paths;
            }
            foreach (string file in Directory.GetFiles(root, "*.md", SearchOption.AllDirectories))
            {
                string relative = file.Substring(root.Length).Replace('\\', '/').TrimStart('/');
                paths.Add(SceneDocsPaths.Pages + relative);
            }
            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        /// <summary>Every sample folder number ("01" to "10") with a README.</summary>
        public static List<string> SampleNumbers()
        {
            List<string> numbers = new List<string>();
            foreach (string folder in SampleFolders())
            {
                numbers.Add(Path.GetFileName(folder).Split(' ')[0]);
            }
            return numbers;
        }

        /// <summary>The asset path of a sample's folder ("Samples/05 Persistent Player And Spawn Points"), or null.</summary>
        public static string SampleFolder(string _number)
        {
            foreach (string folder in SampleFolders())
            {
                if (Path.GetFileName(folder).StartsWith(_number + " "))
                {
                    return SceneDocsPaths.Samples + Path.GetFileName(folder);
                }
            }
            return null;
        }

        private static List<string> SampleFolders()
        {
            List<string> folders = new List<string>();
            string root = SceneDocsPaths.Physical(SceneDocsPaths.Samples);
            if (!Directory.Exists(root))
            {
                return folders;
            }
            foreach (string folder in Directory.GetDirectories(root))
            {
                string name = Path.GetFileName(folder);
                bool isNumbered = name.Length > 3 && char.IsDigit(name[0]) && char.IsDigit(name[1]) && name[2] == ' ';
                if (isNumbered && File.Exists(Path.Combine(folder, "README.md")))
                {
                    folders.Add(folder);
                }
            }
            folders.Sort(StringComparer.Ordinal);
            return folders;
        }

        private static string SampleReadme(string _number)
        {
            string folder = SampleFolder(_number);
            return folder == null ? null : folder + "/README.md";
        }

        /// <summary>Every design decision file, in order.</summary>
        public static List<string> AdrPaths()
        {
            List<string> paths = new List<string>();
            string root = SceneDocsPaths.Physical(SceneDocsPaths.Adr);
            if (!Directory.Exists(root))
            {
                return paths;
            }
            foreach (string file in Directory.GetFiles(root, "*.md"))
            {
                paths.Add(SceneDocsPaths.Adr + Path.GetFileName(file));
            }
            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        private static string AdrFile(string _number)
        {
            foreach (string path in AdrPaths())
            {
                if (Path.GetFileName(path).StartsWith(_number + "-"))
                {
                    return path;
                }
            }
            return null;
        }
    }
}
