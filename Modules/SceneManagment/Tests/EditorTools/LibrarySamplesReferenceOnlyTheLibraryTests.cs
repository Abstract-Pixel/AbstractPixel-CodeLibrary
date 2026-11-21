using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// Library samples are graphics-free (session 5A, stage 0): every sample scene and asset references only the
    /// library's own assets and Unity's built-in resources. A terrain asset, a URP material or a component from a game
    /// (such as Rally Shift's "Prefab Indexer", which its play mode saver adds on every save) fails the test by name.
    /// </summary>
    public class LibrarySamplesReferenceOnlyTheLibraryTests
    {
        private const string PackageRoot = "Packages/com.abstractpixel.library";

        // Unity's built-in resources ("unity default resources", "unity_builtin_extra" and the built-in editor assets).
        private static readonly HashSet<string> UnityBuiltInGuids = new HashSet<string>
        {
            "0000000000000000d000000000000000",
            "0000000000000000e000000000000000",
            "0000000000000000f000000000000000"
        };

        private static readonly Regex GuidPattern = new Regex("guid: ([0-9a-fA-F]{32})");

        /// <summary>Every text-serialized scene and asset in a Samples folder of the library.</summary>
        public static IEnumerable<string> SampleAssets()
        {
            List<string> assets = new List<string>();
            string root = Path.GetFullPath(PackageRoot);
            if (!Directory.Exists(root))
            {
                return assets;
            }
            foreach (string fullPath in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                string relative = fullPath.Substring(root.Length).Replace('\\', '/');
                if (IsSampleAsset(relative))
                {
                    assets.Add(PackageRoot + relative);
                }
            }
            assets.Sort();
            return assets;
        }

        [TestCaseSource(nameof(SampleAssets))]
        public void ReferencesOnlyTheLibraryAndUnityBuiltIns(string _assetPath)
        {
            string fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Assert.Ignore(_assetPath + " was deleted after the test list was read.");
            }
            List<string> outsiders = new List<string>();
            string text = File.ReadAllText(fullPath);
            foreach (Match match in GuidPattern.Matches(text))
            {
                string guid = match.Groups[1].Value.ToLowerInvariant();
                if (IsAllowed(guid))
                {
                    continue;
                }
                string owner = AssetDatabase.GUIDToAssetPath(guid);
                string named = guid + " (" + (string.IsNullOrEmpty(owner) ? "not in this project" : owner) + ")";
                if (!outsiders.Contains(named))
                {
                    outsiders.Add(named);
                }
            }

            Assert.IsEmpty(outsiders, _assetPath + " references assets outside the library: " + string.Join(", ", outsiders));
        }

        // A scene or asset under a Samples (or Samples~) folder, serialized as text; .meta files and code are left out.
        private static bool IsSampleAsset(string _relativePath)
        {
            bool inSamples = _relativePath.Contains("/Samples/") || _relativePath.Contains("/Samples~/");
            if (!inSamples || _relativePath.Contains("/Editor/"))
            {
                return false;
            }
            string extension = Path.GetExtension(_relativePath).ToLowerInvariant();
            return extension == ".unity" || extension == ".asset" || extension == ".prefab" || extension == ".mat" || extension == ".controller" || extension == ".playable";
        }

        private static bool IsAllowed(string _guid)
        {
            if (UnityBuiltInGuids.Contains(_guid))
            {
                return true;
            }
            string path = AssetDatabase.GUIDToAssetPath(_guid);
            return path.StartsWith(PackageRoot + "/");
        }
    }
}
