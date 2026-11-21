using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// Session 4D stage 4 (ADR 0014): a sample scene never reaches Build Settings, a build that contains one stops with
    /// the scene named, and a scene used by both a sample and a game scene group is an error.
    /// </summary>
    public class SampleSceneGroupTests
    {
        private const string SampleLevel = "Assets/Samples/05/Sample Level A.unity";
        private const string SharedScene = "Assets/Samples/05/Sample Shared.unity";
        private const string GameLevel = "Assets/Game/Level 1.unity";
        private readonly List<SceneGroup> groups = new List<SceneGroup>();
        private System.Func<List<SceneGroup>> realFindGroups;

        [SetUp]
        public void UseOwnGroups()
        {
            realFindGroups = SampleSceneGroups.FindGroups;
            SampleSceneGroups.FindGroups = () => new List<SceneGroup>(groups);
            SampleSceneGroups.Forget();
        }

        [TearDown]
        public void RestoreGroups()
        {
            SampleSceneGroups.FindGroups = realFindGroups;
            SampleSceneGroups.Forget();
            foreach (SceneGroup group in groups)
            {
                Object.DestroyImmediate(group);
            }
            groups.Clear();
        }

        [Test]
        public void TheWriterLeavesSampleScenesOut()
        {
            AddGroup("Sample A", SampleLevel, SharedScene, true);
            EditorBuildSettingsScene[] wanted =
            {
                new EditorBuildSettingsScene(GameLevel, true),
                new EditorBuildSettingsScene(SampleLevel, true),
                new EditorBuildSettingsScene(SharedScene, true)
            };

            EditorBuildSettingsScene[] written = SceneBuildSettingsWriter.WithoutScenesNeverInBuild(wanted, SampleSceneGroups.ScenePaths());

            Assert.AreEqual(1, written.Length);
            Assert.AreEqual(GameLevel, written[0].path);
        }

        [Test]
        public void ASampleGroupAddsNothingToBuildSettings()
        {
            SceneGroup sample = AddGroup("Sample A", SampleLevel, SharedScene, true);

            BuildSettingsChange change = SceneGroupBuildSettings.PreviewForProject(sample);

            Assert.IsFalse(change.HasChanges);
        }

        [Test]
        public void ABuildContainingASampleSceneStopsAndNamesIt()
        {
            AddGroup("Sample A", SampleLevel, SharedScene, true);
            List<string> inBuild = new List<string> { GameLevel, SampleLevel };

            string message = SampleSceneBuildCheck.FailureMessage(inBuild, SampleSceneGroups.ScenePaths(), _path => false);

            StringAssert.Contains("The build stopped", message);
            StringAssert.Contains("'Sample Level A' (sample scene group 'Sample A') is in Build Settings", message);
            StringAssert.DoesNotContain("Sample Shared", message, "a sample scene that is not in the build is not named");
        }

        [Test]
        public void AnAddressableSampleSceneStopsTheBuildToo()
        {
            AddGroup("Sample A", SampleLevel, SharedScene, true);

            string message = SampleSceneBuildCheck.FailureMessage(new List<string>(), SampleSceneGroups.ScenePaths(), _path => _path == SharedScene);

            StringAssert.Contains("'Sample Shared' (sample scene group 'Sample A') is in Addressables", message);
        }

        [Test]
        public void ABuildWithoutSampleScenesGoesOn()
        {
            AddGroup("Sample A", SampleLevel, SharedScene, true);

            string message = SampleSceneBuildCheck.FailureMessage(new List<string> { GameLevel }, SampleSceneGroups.ScenePaths(), _path => false);

            Assert.AreEqual(string.Empty, message);
        }

        [Test]
        public void ASceneUsedByBothKindsOfGroupIsAnError()
        {
            AddGroup("Sample A", SampleLevel, SharedScene, true);
            SceneGroup game = AddGroup("Level 1 group", GameLevel, SharedScene, false);

            List<SceneGroupProblem> problems = SceneGroupCheck.Find(game, _scene => SceneStatus.InBuild);

            SceneGroupProblem shared = problems.Find(_problem => _problem.Kind == SceneGroupProblemKind.SharedWithOtherKind);
            Assert.IsNotNull(shared, "the game group lists it");
            StringAssert.Contains("'Sample Shared' is also used by sample scene group 'Sample A'", shared.Message);
            Assert.IsTrue(shared.BlocksBuild);
        }

        [Test]
        public void ASampleGroupIsNotFlaggedForScenesNotInBuild()
        {
            SceneGroup sample = AddGroup("Sample A", SampleLevel, SharedScene, true);

            List<SceneGroupProblem> problems = SceneGroupCheck.Find(sample, _scene => SceneStatus.NotInBuild);

            Assert.AreEqual(0, problems.Count, "a sample's scenes are never in build on purpose");
        }

        private SceneGroup AddGroup(string _name, string _mainScenePath, string _sharedScenePath, bool _isSample)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _name;
            SceneReference main = new SceneReference(System.IO.Path.GetFileNameWithoutExtension(_mainScenePath), string.Empty, _mainScenePath);
            SceneReference shared = new SceneReference(System.IO.Path.GetFileNameWithoutExtension(_sharedScenePath), string.Empty, _sharedScenePath);
            group.Initialize(new SceneReference[0], new List<SceneReference> { shared }, main, false);
            group.IsSample = _isSample;
            groups.Add(group);
            SampleSceneGroups.Forget();
            return group;
        }
    }
}
