using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A4: a scene is known by its asset (GUID, path, name). Two references match by GUID when both have one,
    /// otherwise by name.
    /// </summary>
    public class SceneReferenceIdentityTests
    {
        private const string TestFolder = "Assets/A4SceneIdentityTests";
        private const string FirstScenePath = TestFolder + "/IdentityLevel.unity";
        private const string SecondFolder = TestFolder + "/Other";
        private const string SecondScenePath = SecondFolder + "/IdentityLevel.unity";

        [SetUp]
        public void CreateTwoScenesWithTheSameName()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.CreateFolder("Assets", "A4SceneIdentityTests");
            AssetDatabase.CreateFolder(TestFolder, "Other");
            SaveEmptySceneAt(FirstScenePath);
            SaveEmptySceneAt(SecondScenePath);
        }

        [TearDown]
        public void DeleteTestScenes()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        private static void SaveEmptySceneAt(string _path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, _path);
        }

        private static SceneReference ReferenceToAsset(string _path)
        {
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_path);
            SceneReference reference = new SceneReference(string.Empty);
            reference.SceneFieldReference.SetSceneAsset(asset);
            return reference;
        }

        [Test]
        public void AssigningASceneAssetStoresGuidPathAndName()
        {
            SceneReference reference = ReferenceToAsset(FirstScenePath);

            Assert.AreEqual(AssetDatabase.AssetPathToGUID(FirstScenePath), reference.SceneGuid);
            Assert.AreEqual(FirstScenePath, reference.ScenePath);
            Assert.AreEqual("IdentityLevel", reference.SceneName);
            Assert.IsTrue(reference.HasGuid);
        }

        [Test]
        public void TwoScenesWithTheSameNameInDifferentFoldersAreNotTheSameScene()
        {
            SceneReference first = ReferenceToAsset(FirstScenePath);
            SceneReference second = ReferenceToAsset(SecondScenePath);

            Assert.AreEqual(first.SceneName, second.SceneName);
            Assert.IsFalse(first.Equals(second), "same name but different GUIDs must not match");
            HashSet<SceneReference> set = new HashSet<SceneReference> { first, second };
            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void ReferencesWithTheSameGuidMatchEvenWhenTheNameDiffers()
        {
            SceneReference current = new SceneReference("Level 5", "guid-a", "Assets/Level 5.unity");
            SceneReference renamedCopy = new SceneReference("Level Five", "guid-a", "Assets/Level Five.unity");

            Assert.IsTrue(current.Equals(renamedCopy));
        }

        [Test]
        public void ReferencesWithDifferentGuidsDoNotMatchEvenWhenTheNameIsTheSame()
        {
            SceneReference first = new SceneReference("Level 5", "guid-a", "Assets/A/Level 5.unity");
            SceneReference second = new SceneReference("Level 5", "guid-b", "Assets/B/Level 5.unity");

            Assert.IsFalse(first.Equals(second));
        }

        [Test]
        public void AReferenceWithoutAGuidMatchesByNameIgnoringCase()
        {
            SceneReference fromGroup = new SceneReference("Level 5", "guid-a", "Assets/Level 5.unity");
            SceneReference fromActiveSceneName = new SceneReference("level 5");

            Assert.IsTrue(fromGroup.Equals(fromActiveSceneName));
            Assert.IsTrue(fromActiveSceneName.Equals(fromGroup));
            HashSet<SceneReference> open = new HashSet<SceneReference> { fromGroup };
            Assert.IsTrue(open.Contains(fromActiveSceneName), "a plan that holds group references must find a name-only one");
        }

        [Test]
        public void ANameOnlyReferenceHasNoGuidOrPath()
        {
            SceneReference nameOnly = new SceneReference("Level 5");

            Assert.IsFalse(nameOnly.HasGuid);
            Assert.AreEqual(string.Empty, nameOnly.SceneGuid);
            Assert.AreEqual(string.Empty, nameOnly.ScenePath);
        }
    }
}
