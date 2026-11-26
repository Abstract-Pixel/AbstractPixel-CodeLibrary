using System.IO;
using UnityEditor;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>Reading documentation files by asset path, and the path arithmetic their relative links need.</summary>
    public static class DocsFiles
    {
        /// <summary>The file on disk for an asset path (packages can live anywhere).</summary>
        public static string Physical(string _assetPath)
        {
            return FileUtil.GetPhysicalPath(_assetPath);
        }

        public static bool Exists(string _assetPath)
        {
            return File.Exists(Physical(_assetPath));
        }

        public static string Read(string _assetPath)
        {
            string physical = Physical(_assetPath);
            if (!File.Exists(physical))
            {
                return null;
            }
            return File.ReadAllText(physical);
        }

        /// <summary>Joins a relative path onto a folder and removes "." and ".." parts. Forward slashes only.</summary>
        public static string Combine(string _folder, string _relative)
        {
            string joined = (_folder.TrimEnd('/') + "/" + _relative).Replace('\\', '/');
            string[] parts = joined.Split('/');
            System.Collections.Generic.List<string> kept = new System.Collections.Generic.List<string>();
            foreach (string part in parts)
            {
                if (part == "." || part.Length == 0)
                {
                    continue;
                }
                if (part == ".." && kept.Count > 0)
                {
                    kept.RemoveAt(kept.Count - 1);
                    continue;
                }
                kept.Add(part);
            }
            return string.Join("/", kept);
        }

        public static string FolderOf(string _assetPath)
        {
            int slash = _assetPath.LastIndexOf('/');
            return slash < 0 ? "" : _assetPath.Substring(0, slash);
        }
    }
}
