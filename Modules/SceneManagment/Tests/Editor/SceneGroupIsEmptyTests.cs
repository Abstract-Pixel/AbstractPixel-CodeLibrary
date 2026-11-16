using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>A scene group is empty only when it has no main scene, no manager scenes and no shared scenes.</summary>
    public class SceneGroupIsEmptyTests
    {
        private SceneGroup group;

        [SetUp]
        public void CreateEmptyGroup()
        {
            group = ScriptableObject.CreateInstance<SceneGroup>();
        }

        [TearDown]
        public void DestroyGroup()
        {
            Object.DestroyImmediate(group);
        }

        [Test]
        public void GroupWithNothingInItIsEmpty()
        {
            Assert.IsTrue(group.IsEmpty());
        }

        [Test]
        public void GroupWithListsSetToNullIsEmpty()
        {
            group.ManagerialBootScenesList = null;
            group.ContextualBootScenesList = null;

            Assert.IsTrue(group.IsEmpty());
        }

        [Test]
        public void GroupWithOnlyAMainSceneIsNotEmpty()
        {
            group.MainScene = new SceneReference("Level1");

            Assert.IsFalse(group.IsEmpty());
        }

        [Test]
        public void GroupWithOnlyAManagerSceneIsNotEmpty()
        {
            group.ManagerialBootScenesList = new List<SceneReference> { new SceneReference("Manager") };

            Assert.IsFalse(group.IsEmpty());
        }

        [Test]
        public void GroupWithOnlyASharedSceneIsNotEmpty()
        {
            group.ContextualBootScenesList = new List<SceneReference> { new SceneReference("Car") };

            Assert.IsFalse(group.IsEmpty());
        }

        [Test]
        public void MainSceneWithoutANameCountsAsNoMainScene()
        {
            group.MainScene = new SceneReference("");

            Assert.IsTrue(group.IsEmpty());
        }
    }
}
