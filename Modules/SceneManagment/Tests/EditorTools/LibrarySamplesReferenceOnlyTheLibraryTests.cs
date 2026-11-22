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

        /// <summary>Every text-serialized scene and asset of the scene system module outside its Samples folder.</summary>
        public static IEnumerable<string> SceneSystemAssets()
        {
            List<string> assets = new List<string>();
            string root = Path.GetFullPath(PackageRoot);
            string moduleRoot = Path.Combine(root, "Modules", "SceneManagment");
            if (!Directory.Exists(moduleRoot))
            {
                return assets;
            }
            foreach (string fullPath in Directory.GetFiles(moduleRoot, "*", SearchOption.AllDirectories))
            {
                string relative = fullPath.Substring(root.Length).Replace('\\', '/');
                bool inSamples = relative.Contains("/Samples/") || relative.Contains("/Samples~/");
                if (!inSamples && IsSerializedAsset(relative))
                {
                    assets.Add(PackageRoot + relative);
                }
            }
            assets.Sort();
            return assets;
        }

        /// <summary>
        /// The scene system's own scenes and assets (the ready-made loading scene, fades) may use Unity's packages, but
        /// never an asset of the game that hosts the library: such a reference breaks in every other project.
        /// </summary>
        [TestCaseSource(nameof(SceneSystemAssets))]
        public void SceneSystemAssetReferencesNoGameAsset(string _assetPath)
        {
            string fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Assert.Ignore(_assetPath + " was deleted after the test list was read.");
            }
            List<string> gameAssets = new List<string>();
            foreach (Match match in GuidPattern.Matches(File.ReadAllText(fullPath)))
            {
                string owner = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value.ToLowerInvariant());
                if (owner.StartsWith("Assets/") && !gameAssets.Contains(owner))
                {
                    gameAssets.Add(owner);
                }
            }

            Assert.IsEmpty(gameAssets, _assetPath + " references the game's assets: " + string.Join(", ", gameAssets));
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
            return IsSerializedAsset(_relativePath);
        }

        private static bool IsSerializedAsset(string _relativePath)
        {
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
