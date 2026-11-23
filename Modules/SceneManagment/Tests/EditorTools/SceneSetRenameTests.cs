using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// Renaming a scene set on the Scene System page (session 5A2) renames its asset file and keeps its GUID; an empty,
    /// invalid or taken name is refused and changes nothing.
    /// </summary>
    public class SceneSetRenameTests
    {
        private const string Folder = "Assets/_TempSceneSetRename";

        private SceneSet first;
        private SceneSet second;

        [SetUp]
        public void CreateSets()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets", "_TempSceneSetRename");
            }
            first = Create("New Scene Set");
            second = Create("Boss Levels");
        }

        [TearDown]
        public void DeleteSets()
        {
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void ARenameChangesTheFileAndKeepsTheGuid()
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(first));

            bool renamed = SceneSetRename.Rename(first, "Racing Levels", out string error);

            Assert.IsTrue(renamed, error);
            Assert.AreEqual(Folder + "/Racing Levels.asset", AssetDatabase.GetAssetPath(first));
            Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(first)));
            Assert.AreEqual("Racing Levels", first.name);
        }

        [TestCase("", "name")]
        [TestCase("Boss Levels", "already named")]
        [TestCase("boss levels", "already named")]
        [TestCase("Racing/Levels", "contain")]
        public void AnEmptyInvalidOrTakenNameIsRefusedAndChangesNothing(string _name, string _expectedWord)
        {
            bool renamed = SceneSetRename.Rename(first, _name, out string error);

            Assert.IsFalse(renamed);
            StringAssert.Contains(_expectedWord, error);
            Assert.AreEqual(Folder + "/New Scene Set.asset", AssetDatabase.GetAssetPath(first));
        }

        [Test]
        public void KeepingTheSameNameIsFine()
        {
            Assert.AreEqual(string.Empty, SceneSetRename.Problem(first, "New Scene Set"));
            Assert.IsTrue(SceneSetRename.Rename(first, "New Scene Set", out string _));
        }

        private static SceneSet Create(string _name)
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            AssetDatabase.CreateAsset(set, Folder + "/" + _name + ".asset");
            return set;
        }
    }
}
