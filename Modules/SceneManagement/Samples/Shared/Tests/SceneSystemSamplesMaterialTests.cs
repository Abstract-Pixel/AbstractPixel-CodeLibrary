using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// The samples are graphics-free (scene system S, decision D1): every material uses the library's sample shader,
    /// scenes and prefabs use only sample materials, no colour is magenta-like, and no texture, font or audio file ships.
    /// </summary>
    public class SceneSystemSamplesMaterialTests
    {
        private const string SampleShaderName = "Abstract Pixel/Samples/Sample Lit";
        private const string SampleDefaultMaterialGuid = "a68bea6173504a34b87eccdc0443691a";
        private const string SampleGroundMaterialGuid = "da3afed60dee9134a94bf27c5b7dea79";

        private static readonly string[] ForbiddenExtensions =
        {
            ".png", ".jpg", ".jpeg", ".tga", ".psd", ".exr", ".ttf", ".otf", ".wav", ".mp3", ".ogg"
        };

        private static readonly Regex GuidPattern = new Regex("guid: ([0-9a-fA-F]{32})");
        private static readonly Regex ColorPattern = new Regex("- _Color: \\{r: ([-0-9.eE]+), g: ([-0-9.eE]+), b: ([-0-9.eE]+)");

        public static IEnumerable<string> SampleMaterials()
        {
            return FilesWithExtension(".mat");
        }

        public static IEnumerable<string> SampleScenesAndPrefabs()
        {
            List<string> files = FilesWithExtension(".unity");
            files.AddRange(FilesWithExtension(".prefab"));
            return files;
        }

        [TestCaseSource(nameof(SampleMaterials))]
        public void UsesTheSampleShader(string _materialPath)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(_materialPath);

            Assert.IsNotNull(material, _materialPath);
            Assert.AreEqual(SampleShaderName, material.shader.name, _materialPath);
            Assert.IsTrue(material.shader.isSupported, SampleShaderName + " is not supported here");
        }

        [TestCaseSource(nameof(SampleMaterials))]
        public void ItsColourIsNotMagentaLike(string _materialPath)
        {
            string text = File.ReadAllText(Path.GetFullPath(_materialPath));
            Match match = ColorPattern.Match(text);
            if (match.Success == false)
            {
                return;
            }
            float red = ParseFloat(match.Groups[1].Value);
            float green = ParseFloat(match.Groups[2].Value);
            float blue = ParseFloat(match.Groups[3].Value);
            bool isMagentaLike = red >= 0.8f && green <= 0.25f && blue >= 0.8f;

            Assert.IsFalse(isMagentaLike, _materialPath + " has a magenta-like _Color (" + red + ", " + green + ", " + blue + ")");
        }

        [TestCaseSource(nameof(SampleScenesAndPrefabs))]
        public void UsesOnlySampleMaterials(string _assetPath)
        {
            string text = File.ReadAllText(Path.GetFullPath(_assetPath));
            List<string> foreign = new List<string>();
            foreach (Match match in GuidPattern.Matches(text))
            {
                string guid = match.Groups[1].Value.ToLowerInvariant();
                string path = AssetDatabase.GUIDToAssetPath(guid);
                bool isMaterial = path.EndsWith(".mat");
                if (isMaterial == false)
                {
                    continue;
                }
                if (IsSampleMaterial(guid, path))
                {
                    continue;
                }
                if (foreign.Contains(path) == false)
                {
                    foreign.Add(path);
                }
            }

            Assert.IsEmpty(foreign, _assetPath + " uses materials that are not sample materials");
        }

        [Test]
        public void NoTextureFontOrAudioFileShips()
        {
            List<string> forbidden = new List<string>();
            foreach (string assetPath in SampleFolders.AllFiles())
            {
                string extension = Path.GetExtension(assetPath).ToLowerInvariant();
                if (System.Array.IndexOf(ForbiddenExtensions, extension) >= 0)
                {
                    forbidden.Add(assetPath);
                    continue;
                }
                if (extension == ".asset" && IsFontAsset(assetPath))
                {
                    forbidden.Add(assetPath);
                }
            }

            Assert.IsEmpty(forbidden, "the samples ship textures, fonts or audio files");
        }

        private static bool IsSampleMaterial(string _guid, string _path)
        {
            if (_guid == SampleDefaultMaterialGuid || _guid == SampleGroundMaterialGuid)
            {
                return true;
            }
            return _path.StartsWith(SampleFolders.SamplesAssetPath + "/");
        }

        private static bool IsFontAsset(string _assetPath)
        {
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(_assetPath);
            if (type == null)
            {
                return false;
            }
            return type.Name.Contains("FontAsset");
        }

        private static float ParseFloat(string _text)
        {
            return float.Parse(_text, CultureInfo.InvariantCulture);
        }

        private static List<string> FilesWithExtension(string _extension)
        {
            List<string> files = new List<string>();
            foreach (string assetPath in SampleFolders.AllFiles())
            {
                if (assetPath.EndsWith(_extension))
                {
                    files.Add(assetPath);
                }
            }
            return files;
        }
    }
}
