using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>5B known item 1: a sample scene has a status of its own, no fix, and no write claims it was added.</summary>
    public class NeverInBuildSceneTests
    {
        private const string Folder = "Assets/_NeverInBuildTest";
        private const string ScenePath = Folder + "/NeverInBuild.unity";
        private System.Func<HashSet<string>> realPaths;

        [SetUp]
        public void MakeAScene()
        {
            realPaths = SceneBuildSettingsWriter.ScenePathsNeverInBuild;
            SceneBuildSettingsWriter.ScenePathsNeverInBuild = () => new HashSet<string> { ScenePath };
            AssetDatabase.CreateFolder("Assets", "_NeverInBuildTest");
            // An empty scene file: the test runner's untitled scene rules out making one additively.
            System.IO.File.WriteAllText(ScenePath, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(ScenePath);
        }

        [TearDown]
        public void RemoveTheScene()
        {
            SceneBuildSettingsWriter.ScenePathsNeverInBuild = realPaths;
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void TheStatusSaysWhyAndOffersNoFix()
        {
            SceneStatusReport report = SceneStatusReader.Read(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), "NeverInBuild", string.Empty, ScenePath);

            Assert.AreEqual(SceneStatus.NeverInBuild, report.Status);
            Assert.IsFalse(report.HasFix);
            StringAssert.Contains("never goes into a build", report.Message);
            StringAssert.Contains("sample", report.Message);
            Assert.IsFalse(SceneStatusReader.CanMoveTo(report, ScenePlace.BuildSettings));
            Assert.IsFalse(SceneStatusReader.CanMoveTo(report, ScenePlace.Addressables));
        }

        [Test]
        public void AWriteThatDropsASceneNamesItAndTheReason()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            LogAssert.Expect(LogType.Warning, new Regex("'NeverInBuild'.*sample scene never goes into a build"));

            SceneBuildSettingsWriter.Write(scenes.ToArray());
        }

        [Test]
        public void AGameGroupUsingASampleSceneNeverSaysItAddedIt()
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = "Group under test";
            SceneReference main = new SceneReference("NeverInBuild", AssetDatabase.AssetPathToGUID(ScenePath), ScenePath);
            group.Initialize(new SceneReference[0], new SceneReference[0], main);
            LogAssert.Expect(LogType.Warning, new Regex("Group under test.*'NeverInBuild'.*never goes into a build"));

            BuildSettingsChange change = SceneGroupBuildSettings.ApplyToProject(group);

            Assert.AreEqual(0, change.AddedPaths.Count);
            LogAssert.NoUnexpectedReceived();
            Object.DestroyImmediate(group);
        }

        [Test]
        public void TheCreateLineSaysASampleGroupWasMadeAndWhy()
        {
            string line = CreateSceneGroupMenu.DescribeCreated("RandoGroup", "rando", true, "x.");

            StringAssert.Contains("sample scene group 'RandoGroup'", line);
            StringAssert.Contains("Samples folder", line);
            StringAssert.Contains("never go into a build", line);
        }
    }
}
