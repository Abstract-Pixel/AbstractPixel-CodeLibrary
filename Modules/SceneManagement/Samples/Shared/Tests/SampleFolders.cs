using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Where the scene system samples live on disk, and which folders they are (shared by the sample checks).</summary>
    public static class SampleFolders
    {
        public const string SamplesAssetPath = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples";
        public const string SharedFolderName = "Shared";

        /// <summary>The ten samples this session builds (scene system S).</summary>
        public static readonly string[] ExpectedSamples =
        {
            "01 Button Changes Level",
            "02 Change Level From Code",
            "03 Fast Restart",
            "04 Play From Any Scene",
            "05 Persistent Player And Spawn Points",
            "06 Loading Scene",
            "07 Preload The Next Level",
            "08 Addressables",
            "09 Editor Tools",
            "10 Launch Scene"
        };

        private static readonly Regex NumberedFolder = new Regex("^[0-9]{2} ");

        public static string SamplesFullPath()
        {
            return Path.GetFullPath(SamplesAssetPath);
        }

        /// <summary>The numbered sample folders that exist, by folder name, sorted.</summary>
        public static List<string> NumberedSampleFolders()
        {
            List<string> folders = new List<string>();
            string root = SamplesFullPath();
            if (Directory.Exists(root) == false)
            {
                return folders;
            }
            foreach (string folder in Directory.GetDirectories(root))
            {
                string name = Path.GetFileName(folder);
                if (NumberedFolder.IsMatch(name))
                {
                    folders.Add(name);
                }
            }
            folders.Sort();
            return folders;
        }

        /// <summary>The asset path of a file, from its full path.</summary>
        public static string ToAssetPath(string _fullPath)
        {
            string root = SamplesFullPath();
            string relative = _fullPath.Substring(root.Length).Replace('\\', '/');
            return SamplesAssetPath + relative;
        }

        /// <summary>Every file under the Samples folder, .meta files left out, as asset paths.</summary>
        public static List<string> AllFiles()
        {
            List<string> files = new List<string>();
            string root = SamplesFullPath();
            if (Directory.Exists(root) == false)
            {
                return files;
            }
            foreach (string fullPath in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                if (fullPath.EndsWith(".meta"))
                {
                    continue;
                }
                files.Add(ToAssetPath(fullPath));
            }
            files.Sort();
            return files;
        }
    }
}
