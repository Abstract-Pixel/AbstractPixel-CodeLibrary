using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Rename tracking: which files under Assets/ hold a renamed scene's GUID.</summary>
    public class SceneRenameTrackingTests
    {
        private const string SceneGuid = "937f7f0f10b87af4aa8a6c14e441ea1c";

        [TestCase("Assets/Groups/level7Group.asset", true)]
        [TestCase("Assets/Prefabs/Door.prefab", true)]
        [TestCase("Assets/Scenes/Menu.unity", true)]
        [TestCase("Assets/Scripts/Door.cs", false)]
        [TestCase("Assets/Groups/level7Group.asset.meta", false)]
        [TestCase("Packages/com.example/Group.asset", false)]
        [TestCase("Assets/Textures/Road.png", false)]
        public void IsCandidate_OnlyAssetsPrefabsAndScenesUnderAssets(string _path, bool _expected)
        {
            Assert.AreEqual(_expected, SceneRenameTracking.IsCandidate(_path));
        }

        [Test]
        public void PathsContainingGuid_FindsTheFilesThatHoldIt_ExceptTheSceneItself()
        {
            Dictionary<string, string> files = new Dictionary<string, string>
            {
                { "Assets/level7Group.asset", "sceneGuid: " + SceneGuid },
                { "Assets/Other.asset", "sceneGuid: 0000" },
                { "Assets/Door.prefab", "sceneAsset: {fileID: 102900000, guid: " + SceneGuid + ", type: 3}" },
                { "Assets/Level 7.unity", "self " + SceneGuid },
            };

            List<string> found = SceneRenameTracking.PathsContainingGuid(files.Keys, SceneGuid, "Assets/Level 7.unity", _path => Encoding.UTF8.GetBytes(files[_path]));

            CollectionAssert.AreEquivalent(new[] { "Assets/level7Group.asset", "Assets/Door.prefab" }, found);
        }

        [Test]
        public void PathsContainingGuid_UnreadableFile_IsSkipped()
        {
            List<string> found = SceneRenameTracking.PathsContainingGuid(new[] { "Assets/Locked.asset" }, SceneGuid, string.Empty, _path => null);

            Assert.IsEmpty(found);
        }
    }
}
