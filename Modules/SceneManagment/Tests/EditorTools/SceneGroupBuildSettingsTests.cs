using System.Collections.Generic;
using System.Linq;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Adding a scene group's scenes to Build Settings: adds what is missing, never twice.</summary>
    public class SceneGroupBuildSettingsTests
    {
        private const string LevelPath = "Assets/Levels/Level 1.unity";
        private const string ManagerPath = "Assets/Boot/Managers.unity";
        private const string SharedPath = "Assets/Boot/Car.unity";

        private static bool EveryFileExists(string _path)
        {
            return true;
        }

        private static EditorBuildSettingsScene[] Build(params string[] _paths)
        {
            return _paths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        }

        private static string[] PathsOf(EditorBuildSettingsScene[] _scenes)
        {
            return _scenes.Select(scene => scene.path).ToArray();
        }

        [Test]
        public void AddScenes_SceneNotInBuild_IsAddedAtTheEndAndSwitchedOn()
        {
            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { LevelPath }, Build(ManagerPath), EveryFileExists);

            CollectionAssert.AreEqual(new[] { ManagerPath, LevelPath }, PathsOf(change.Scenes));
            Assert.IsTrue(change.Scenes[1].enabled);
            CollectionAssert.AreEqual(new[] { LevelPath }, change.AddedPaths);
            Assert.IsTrue(change.HasChanges);
        }

        [Test]
        public void AddScenes_SceneAlreadyInBuild_IsNotAddedAgain()
        {
            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { ManagerPath, LevelPath }, Build(LevelPath, ManagerPath), EveryFileExists);

            CollectionAssert.AreEqual(new[] { LevelPath, ManagerPath }, PathsOf(change.Scenes));
            Assert.IsEmpty(change.AddedPaths);
            Assert.IsFalse(change.HasChanges);
        }

        [Test]
        public void AddScenes_SameSceneTwiceInTheGroup_IsAddedOnce()
        {
            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { SharedPath, SharedPath }, Build(), EveryFileExists);

            CollectionAssert.AreEqual(new[] { SharedPath }, PathsOf(change.Scenes));
            CollectionAssert.AreEqual(new[] { SharedPath }, change.AddedPaths);
        }

        [Test]
        public void AddScenes_SceneInBuildButSwitchedOff_IsSwitchedOnInPlace()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(ManagerPath, true), new EditorBuildSettingsScene(LevelPath, false) };

            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { LevelPath }, current, EveryFileExists);

            CollectionAssert.AreEqual(new[] { ManagerPath, LevelPath }, PathsOf(change.Scenes));
            Assert.IsTrue(change.Scenes[1].enabled);
            CollectionAssert.AreEqual(new[] { LevelPath }, change.EnabledPaths);
            Assert.IsEmpty(change.AddedPaths);
            Assert.IsTrue(change.HasChanges);
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddScenes_EmptyScenePath_IsIgnored(string _emptyPath)
        {
            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { _emptyPath }, Build(ManagerPath), EveryFileExists);

            CollectionAssert.AreEqual(new[] { ManagerPath }, PathsOf(change.Scenes));
            Assert.IsFalse(change.HasChanges);
            Assert.IsEmpty(change.MissingPaths);
        }

        [Test]
        public void AddScenes_SceneFileMissing_IsNotAddedAndIsReported()
        {
            BuildSettingsChange change = SceneGroupBuildSettings.AddScenes(new[] { LevelPath }, Build(), _path => false);

            Assert.IsEmpty(change.Scenes);
            CollectionAssert.AreEqual(new[] { LevelPath }, change.MissingPaths);
            Assert.IsFalse(change.HasChanges);
        }

        [Test]
        public void AddScenes_DoesNotChangeTheArrayItWasGiven()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(LevelPath, false) };

            SceneGroupBuildSettings.AddScenes(new[] { LevelPath, ManagerPath }, current, EveryFileExists);

            Assert.AreEqual(1, current.Length);
            Assert.IsFalse(current[0].enabled);
        }

        [Test]
        public void WithoutAddressableScenes_LeavesOutAddressableScenes_KeepsOrder()
        {
            string[] paths = { ManagerPath, SharedPath, LevelPath };

            List<string> forBuild = SceneGroupBuildSettings.WithoutAddressableScenes(paths, _path => _path == SharedPath);

            CollectionAssert.AreEqual(new[] { ManagerPath, LevelPath }, forBuild);
        }

        [Test]
        public void GroupScenePaths_ListsManagerThenSharedThenMainScene_SkippingEmptyOnes()
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            SceneReference manager = new SceneReference("Managers", string.Empty, ManagerPath);
            SceneReference shared = new SceneReference("Car", string.Empty, SharedPath);
            SceneReference empty = new SceneReference(string.Empty);
            SceneReference main = new SceneReference("Level 1", string.Empty, LevelPath);
            group.Initialize(new[] { manager }, new[] { shared, empty }, main);

            List<string> paths = SceneGroupBuildSettings.GroupScenePaths(group);

            CollectionAssert.AreEqual(new[] { ManagerPath, SharedPath, LevelPath }, paths);
            Object.DestroyImmediate(group);
        }
    }
}
