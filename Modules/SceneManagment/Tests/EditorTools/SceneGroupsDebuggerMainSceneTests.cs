using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// The Scene Groups Debugger's main scene field shows the scene even when the reference's asset link did not load
    /// (found in Rally Shift on 2026-11-23: every level group showed "None (Scene Asset)" while the group check said "No
    /// problems"). The scene is found again by GUID, then by path.
    /// </summary>
    public class SceneGroupsDebuggerMainSceneTests
    {
        private const string SampleScenePath = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/05 Persistent Player And Spawn Points/Sample 05 Area A.unity";

        private SceneGroup group;

        [SetUp]
        public void CreateGroup()
        {
            group = ScriptableObject.CreateInstance<SceneGroup>();
        }

        [TearDown]
        public void DestroyGroup()
        {
            Object.DestroyImmediate(group);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void AMainSceneKnownOnlyByGuidOrPathStillShows(bool _withGuid, bool _withPath)
        {
            string guid = _withGuid ? AssetDatabase.AssetPathToGUID(SampleScenePath) : string.Empty;
            string path = _withPath ? SampleScenePath : string.Empty;
            SceneReference mainScene = new SceneReference("Sample 05 Area A", guid, path);
            group.Initialize(new List<SceneReference>(), new List<SceneReference>(), mainScene, false);

            SceneAsset shown = SceneGroupsDebugger.MainSceneAssetOf(group);

            Assert.IsNotNull(shown, "the field shows the scene, not None");
            Assert.AreEqual(SampleScenePath, AssetDatabase.GetAssetPath(shown));
        }

        [Test]
        public void ADeletedMainSceneShowsNone()
        {
            SceneReference mainScene = new SceneReference("Old Level", "0123456789abcdef0123456789abcdef", "Assets/Old/Old Level.unity");
            group.Initialize(new List<SceneReference>(), new List<SceneReference>(), mainScene, false);

            Assert.IsNull(SceneGroupsDebugger.MainSceneAssetOf(group));
        }
    }
}
