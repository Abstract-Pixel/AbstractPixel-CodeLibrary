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

        // Unity's built-in extra resources, which hold Default-Material and the Built-in Standard shader.
        private const string BuiltInExtraGuid = "0000000000000000f000000000000000";

        // Default-Material (10303) and the Standard shader (46): only the Built-in render pipeline draws them, so URP
        // and HDRP show every sample object that uses them magenta (scene system 5A2). Samples use the library's
        // sample material ("Abstract Pixel/Samples/Sample Lit") instead.
        private static readonly string[] BuiltInPipelineOnlyReferences =
        {
            "{fileID: 10303, guid: " + BuiltInExtraGuid,
            "{fileID: 46, guid: " + BuiltInExtraGuid
        };

        // The InputRebinding sample lives outside a Samples folder; its game materials follow the same rule.
        private const string RebindingSampleGameFolder = PackageRoot + "/Modules/InputRebinding/Rebinding UI/Game";

        private const string UrpLitShaderGuid = "933532a4fcc9baf4fa0491de14d08ed7";
        private const string SampleLitShaderGuid = "bba8fa45afa4fcf4f8308fa38d3cdfff";
        private const string SampleDefaultMaterialGuid = "a68bea6173504a34b87eccdc0443691a";

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

        /// <summary>Every library sample scene and asset, plus the InputRebinding sample's game materials.</summary>
        public static IEnumerable<string> SampleAssetsAndSampleMaterials()
        {
            List<string> assets = new List<string>(SampleAssets());
            string folder = Path.GetFullPath(RebindingSampleGameFolder);
            if (!Directory.Exists(folder))
            {
                return assets;
            }
            foreach (string fullPath in Directory.GetFiles(folder, "*.mat"))
            {
                assets.Add(RebindingSampleGameFolder + "/" + Path.GetFileName(fullPath));
            }
            return assets;
        }

        /// <summary>
        /// A sample object must look the same in every render pipeline, in edit mode, in Play and in a player, so no
        /// sample uses a material or shader that only the Built-in render pipeline draws.
        /// </summary>
        [TestCaseSource(nameof(SampleAssetsAndSampleMaterials))]
        public void UsesNoMaterialOnlyTheBuiltInPipelineDraws(string _assetPath)
        {
            string fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Assert.Ignore(_assetPath + " was deleted after the test list was read.");
            }
            string text = File.ReadAllText(fullPath);
            int count = 0;
            foreach (string reference in BuiltInPipelineOnlyReferences)
            {
                count += Regex.Matches(text, Regex.Escape(reference)).Count;
            }

            Assert.AreEqual(0, count, _assetPath + " uses Default-Material or the Standard shader " + count + " times; use the library's sample material.");
        }

        /// <summary>The guard's own rule: library and Unity built-in GUIDs pass, a package's (URP Lit) fails.</summary>
        [TestCase(UrpLitShaderGuid, false, TestName = "AllowedGuid(URP Lit shader is outside the library)")]
        [TestCase(SampleLitShaderGuid, true, TestName = "AllowedGuid(the library's Sample Lit shader)")]
        [TestCase(SampleDefaultMaterialGuid, true, TestName = "AllowedGuid(the library's Sample Default material)")]
        [TestCase(BuiltInExtraGuid, true, TestName = "AllowedGuid(Unity built-in extra resources)")]
        public void AllowedGuid(string _guid, bool _expected)
        {
            Assert.AreEqual(_expected, IsAllowed(_guid));
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
