using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// The sample folder layout (scene system S, decision D4): a sample folder holds only README.md at its root, and
    /// every other file sits in the one folder for its asset type. Samples/Shared follows the same rule with no README.
    /// </summary>
    public class SceneSystemSamplesLayoutTests
    {
        private static readonly string[] AllowedFolders =
        {
            "Scenes", "Scene Groups", "Prefabs", "Materials", "Relocation Labels", "Scripts", "Tests", "Timelines"
        };

        public static IEnumerable<string> ExpectedSamples()
        {
            return SampleFolders.ExpectedSamples;
        }

        public static IEnumerable<string> FoldersToCheck()
        {
            List<string> folders = new List<string>(SampleFolders.NumberedSampleFolders());
            folders.Add(SampleFolders.SharedFolderName);
            return folders;
        }

        [TestCaseSource(nameof(ExpectedSamples))]
        public void TheSampleFolderExists(string _sampleFolder)
        {
            string fullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder);

            Assert.IsTrue(Directory.Exists(fullPath), "Sample '" + _sampleFolder + "' is missing: no folder " + fullPath);
        }

        [TestCaseSource(nameof(FoldersToCheck))]
        public void TheRootHoldsOnlyTheReadme(string _sampleFolder)
        {
            string fullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder);
            bool isShared = _sampleFolder == SampleFolders.SharedFolderName;
            List<string> strayFiles = new List<string>();
            foreach (string file in Directory.GetFiles(fullPath))
            {
                string fileName = Path.GetFileName(file);
                if (IsAllowedAtRoot(fileName, isShared, fullPath))
                {
                    continue;
                }
                strayFiles.Add(fileName);
            }

            Assert.IsEmpty(strayFiles, _sampleFolder + " holds files at its root that belong in a type folder");
            if (isShared == false)
            {
                Assert.IsTrue(File.Exists(Path.Combine(fullPath, "README.md")), _sampleFolder + " has no README.md");
            }
        }

        [TestCaseSource(nameof(FoldersToCheck))]
        public void EveryFolderIsATypeFolder(string _sampleFolder)
        {
            string fullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder);
            List<string> unknown = new List<string>();
            foreach (string folder in Directory.GetDirectories(fullPath))
            {
                string name = Path.GetFileName(folder);
                if (System.Array.IndexOf(AllowedFolders, name) < 0)
                {
                    unknown.Add(name);
                }
                foreach (string nested in Directory.GetDirectories(folder))
                {
                    unknown.Add(name + "/" + Path.GetFileName(nested));
                }
            }

            Assert.IsEmpty(unknown, _sampleFolder + " has folders that are not type folders (" + string.Join(", ", AllowedFolders) + ")");
        }

        [TestCaseSource(nameof(FoldersToCheck))]
        public void EveryFileSitsInTheFolderOfItsType(string _sampleFolder)
        {
            string fullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder);
            List<string> misplaced = new List<string>();
            foreach (string folder in Directory.GetDirectories(fullPath))
            {
                string folderName = Path.GetFileName(folder);
                foreach (string file in Directory.GetFiles(folder))
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }
                    string assetPath = SampleFolders.ToAssetPath(file);
                    if (FitsFolder(folderName, assetPath) == false)
                    {
                        misplaced.Add(folderName + "/" + Path.GetFileName(file));
                    }
                }
            }

            Assert.IsEmpty(misplaced, _sampleFolder + " has files in the wrong type folder");
        }

        [TestCaseSource(nameof(FoldersToCheck))]
        public void ScriptsAndTestsHoldExactlyOneAssemblyDefinition(string _sampleFolder)
        {
            string fullPath = Path.Combine(SampleFolders.SamplesFullPath(), _sampleFolder);
            foreach (string folderName in new[] { "Scripts", "Tests" })
            {
                string folder = Path.Combine(fullPath, folderName);
                if (Directory.Exists(folder) == false)
                {
                    continue;
                }
                int asmdefCount = Directory.GetFiles(folder, "*.asmdef").Length;
                Assert.AreEqual(1, asmdefCount, _sampleFolder + "/" + folderName + " must hold exactly one .asmdef");
            }
        }

        private static bool IsAllowedAtRoot(string _fileName, bool _isShared, string _folderFullPath)
        {
            if (_fileName.EndsWith(".meta"))
            {
                string withoutMeta = _fileName.Substring(0, _fileName.Length - ".meta".Length);
                bool isFolderMeta = Directory.Exists(Path.Combine(_folderFullPath, withoutMeta));
                if (isFolderMeta)
                {
                    return true;
                }
                return _isShared == false && withoutMeta == "README.md";
            }
            return _isShared == false && _fileName == "README.md";
        }

        private static bool FitsFolder(string _folderName, string _assetPath)
        {
            string extension = Path.GetExtension(_assetPath).ToLowerInvariant();
            switch (_folderName)
            {
                case "Scenes":
                    return extension == ".unity";
                case "Prefabs":
                    return extension == ".prefab";
                case "Materials":
                    return extension == ".mat";
                case "Timelines":
                    return extension == ".playable";
                case "Scripts":
                case "Tests":
                    return extension == ".cs" || extension == ".asmdef";
                case "Scene Groups":
                    return extension == ".asset" && IsAssetOfType(_assetPath, "SceneGroup");
                case "Relocation Labels":
                    return extension == ".asset" && IsAssetOfType(_assetPath, "RelocationLabel");
                default:
                    return false;
            }
        }

        private static bool IsAssetOfType(string _assetPath, string _typeName)
        {
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(_assetPath);
            if (type == null)
            {
                return false;
            }
            return type.Name == _typeName;
        }
    }
}
