using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>The save system guide that ships in the package (Modules/SaveSystem/Documentation/index.html).</summary>
    [InitializeOnLoad]
    internal static class SaveSystemGuide
    {
        static string docsRoot;
        static readonly HashSet<string> writtenJumpPages = new HashSet<string>();

        static SaveSystemGuide()
        {
            SaveSystemHelpAttribute.FindLink = LinkTo;
        }

        /// <summary>The docs folder's path, or null when the package can't be found.</summary>
        public static string DocsRoot
        {
            get
            {
                if (docsRoot == null)
                {
                    var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(SaveActions).Assembly);
                    docsRoot = package == null ? "" : Path.Combine(package.resolvedPath, "Modules", "SaveSystem", "Documentation");
                }
                return docsRoot.Length > 0 && Directory.Exists(docsRoot) ? docsRoot : null;
            }
        }

        /// <summary>
        /// A link that opens <paramref name="page"/>, a path under the docs folder with an optional "#anchor"
        /// (such as "api/SaveTransform.html" or "debug.html#saved-objects"), or index.html when <paramref name="page"/>
        /// is null. Null when the guide can't be found.
        /// </summary>
        public static string LinkTo(string page = null)
        {
            string root = DocsRoot;
            if (root == null)
            {
                return null;
            }

            string relativePage = string.IsNullOrEmpty(page) ? "index.html" : page;
            int hashIndex = relativePage.IndexOf('#');
            string file = hashIndex < 0 ? relativePage : relativePage.Substring(0, hashIndex);
            string anchor = hashIndex < 0 ? null : relativePage.Substring(hashIndex + 1);

            string fullPath = Path.Combine(root, file.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                return null;
            }

            string target = new Uri(fullPath).AbsoluteUri;
            if (string.IsNullOrEmpty(anchor))
            {
                return target;
            }
            target += "#" + anchor;

            // Windows drops the #anchor when it opens a file link, so a tiny page in Temp jumps there instead.
            string jumpName = (file + "_" + anchor).Replace('/', '_').Replace('\\', '_').Replace('.', '_');
            string jumpPage = Path.GetFullPath(Path.Combine("Temp", "SaveSystemGuide", jumpName + ".html"));
            if (writtenJumpPages.Add(jumpPage) || !File.Exists(jumpPage))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jumpPage));
                File.WriteAllText(jumpPage, $"<!doctype html><meta charset=\"utf-8\"><meta http-equiv=\"refresh\" content=\"0;url={target}\"><a href=\"{target}\">Open the save system guide</a>");
            }
            return new Uri(jumpPage).AbsoluteUri;
        }

        /// <summary>Opens <paramref name="page"/> (see <see cref="LinkTo"/>), or logs where it should be.</summary>
        public static void Open(string page = null)
        {
            string link = LinkTo(page);
            if (link == null)
            {
                SaveLog.Error("[SaveSystem] Can't find the save system guide. It ships in the Abstract Pixel Library package under Modules/SaveSystem/Documentation/index.html.");
                return;
            }
            Application.OpenURL(link);
        }
    }
}
