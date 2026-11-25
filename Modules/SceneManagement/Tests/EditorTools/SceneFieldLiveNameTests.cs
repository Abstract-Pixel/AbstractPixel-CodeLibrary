using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>A scene field reads the scene asset's current name and path in the Editor, so a renamed scene loads before a re-save.</summary>
    public class SceneFieldLiveNameTests
    {
        private const string Folder = "Assets/_SceneFieldLiveNameTests";

        [TearDown]
        public void CleanUp()
        {
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void SceneNameAndPath_AfterTheSceneIsRenamed_FollowTheAsset()
        {
            AssetDatabase.CreateFolder("Assets", "_SceneFieldLiveNameTests");
            // A scene file written directly, so the test does not depend on which scenes the test runner has open.
            System.IO.File.WriteAllText(Folder + "/Before.unity", "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(Folder + "/Before.unity");
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Folder + "/Before.unity");
            SceneField field = new SceneField("Before");
            field.SetSceneAsset(asset);

            AssetDatabase.RenameAsset(Folder + "/Before.unity", "After");

            Assert.AreEqual("After", field.SceneName);
            Assert.AreEqual(Folder + "/After.unity", field.ScenePath);
        }
    }
}
