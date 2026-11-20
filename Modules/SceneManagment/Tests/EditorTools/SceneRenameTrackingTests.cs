using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Rename tracking: which files under Assets/ hold a renamed scene's GUID.</summary>
    public class SceneRenameTrackingTests
    {
        private const string SceneGuid = "937f7f0f10b87af4aa8a6c14e441ea1c";

        [Test]
        public void PathsContainingGuids_FindsEveryFileThatHoldsIt()
        {
            Dictionary<string, string> files = new Dictionary<string, string>
            {
                { "Assets/level7Group.asset", "sceneGuid: " + SceneGuid },
                { "Assets/Other.asset", "sceneGuid: 0000" },
                { "Assets/Door.prefab", "sceneAsset: {fileID: 102900000, guid: " + SceneGuid + ", type: 3}" },
                { "Assets/Level 7.unity", "self " + SceneGuid },
            };

            Dictionary<string, List<string>> found = SceneRenameTracking.PathsContainingGuids(files.Keys, new[] { SceneGuid }, _path => Encoding.UTF8.GetBytes(files[_path]));

            CollectionAssert.AreEquivalent(new[] { "Assets/level7Group.asset", "Assets/Door.prefab", "Assets/Level 7.unity" }, found[SceneGuid]);
        }

        [Test]
        public void PathsContainingGuids_UnreadableFile_IsSkipped()
        {
            Dictionary<string, List<string>> found = SceneRenameTracking.PathsContainingGuids(new[] { "Assets/Locked.asset" }, new[] { SceneGuid }, _path => null);

            Assert.IsEmpty(found[SceneGuid]);
        }
    }
}
