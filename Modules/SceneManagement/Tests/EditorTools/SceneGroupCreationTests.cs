using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>"Create Scene Group" from a scene: where the group goes, and refusing a second group for one main scene.</summary>
    public class SceneGroupCreationTests
    {
        [TestCase("Assets/Levels/Level 7.unity", "Assets/Levels/Level 7Group.asset")]
        [TestCase("Assets/level 3.unity", "Assets/level 3Group.asset")]
        public void GroupPathFor_PutsTheGroupNextToTheScene(string _scenePath, string _expected)
        {
            Assert.AreEqual(_expected, SceneGroupCreation.GroupPathFor(_scenePath));
        }

        [Test]
        public void FindGroupWithMainScene_FindsTheGroupWhoseMainSceneHasThatGuid()
        {
            SceneGroup other = ScriptableObject.CreateInstance<SceneGroup>();
            other.Initialize(new SceneReference[0], new SceneReference[0], new SceneReference("Level 1", "guid-1", "Assets/Level 1.unity"));
            SceneGroup owner = ScriptableObject.CreateInstance<SceneGroup>();
            owner.Initialize(new SceneReference[0], new SceneReference[0], new SceneReference("Level 2", "guid-2", "Assets/Level 2.unity"));

            SceneGroup found = SceneGroupCreation.FindGroupWithMainScene(new List<SceneGroup> { other, null, owner }, "guid-2");
            SceneGroup notFound = SceneGroupCreation.FindGroupWithMainScene(new List<SceneGroup> { other }, "guid-2");

            Assert.AreSame(owner, found);
            Assert.IsNull(notFound);
            Object.DestroyImmediate(other);
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void SceneGroupPathsAmong_KeepsOnlySceneGroupAssets()
        {
            string[] saved = { "Assets/a.asset", "Assets/Level.unity", "Assets/b.asset", "Assets/mat.mat" };

            List<string> groupPaths = SceneGroupSaveFix.SceneGroupPathsAmong(saved, _path => _path == "Assets/b.asset");

            CollectionAssert.AreEqual(new[] { "Assets/b.asset" }, groupPaths);
        }
    }
}
