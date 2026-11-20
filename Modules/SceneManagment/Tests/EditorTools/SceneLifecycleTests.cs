using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Bugs found in the 4A sweep: stale names after a rename, and the save-time fix ignoring where new scenes go.</summary>
    public class SceneLifecycleTests
    {
        private const string Folder = "Assets/_SceneLifecycleTests";
        private ScenePlace placeBefore;

        [SetUp]
        public void MakeFolder()
        {
            placeBefore = SceneStatusReader.ProjectDefaultPlace;
            AssetDatabase.CreateFolder("Assets", "_SceneLifecycleTests");
        }

        [TearDown]
        public void CleanUp()
        {
            SceneStatusReader.ProjectDefaultPlace = placeBefore;
            string scenePath = Folder + "/Lifecycle.unity";
            EditorBuildSettings.scenes = SceneBuildSettingsList.WithoutScene(EditorBuildSettings.scenes, scenePath);
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.RemoveFromAddressables(AssetDatabase.AssetPathToGUID(scenePath));
            }
            AssetDatabase.DeleteAsset(Folder);
        }

        private static SceneAsset MakeScene(string _name)
        {
            string path = Folder + "/" + _name + ".unity";
            System.IO.File.WriteAllText(path, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        [Test]
        public void HasStaleIdentity_AfterARename_TrueUntilSaved()
        {
            SceneAsset scene = MakeScene("Before");
            SceneField field = new SceneField("Before");
            field.SetSceneAsset(scene);
            Assert.IsFalse(field.HasStaleIdentity);

            AssetDatabase.RenameAsset(Folder + "/Before.unity", "After");
            Assert.IsTrue(field.HasStaleIdentity);

            field.OnBeforeSerialize();
            Assert.IsFalse(field.HasStaleIdentity);
        }

        [Test]
        public void PlaceScenesOf_NewScenesGoToAddressables_MakesTheSceneAddressableNotInBuild()
        {
            if (!AddressableScenes.IsInstalled)
            {
                Assert.Ignore("Needs the Addressables package.");
            }
            SceneAsset scene = MakeScene("Lifecycle");
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            SceneReference main = new SceneReference(scene.name);
            main.SceneFieldReference.SetSceneAsset(scene);
            group.Initialize(new SceneReference[0], new SceneReference[0], main);
            SceneStatusReader.ProjectDefaultPlace = ScenePlace.Addressables;

            SceneGroupSaveFix.PlaceScenesOf(group);
            SceneStatus status = SceneStatusReader.Read(main.SceneFieldReference).Status;
            bool listed = SceneBuildSettingsList.IsListed(EditorBuildSettings.scenes, Folder + "/Lifecycle.unity");
            Object.DestroyImmediate(group);

            Assert.AreEqual(SceneStatus.Addressable, status);
            Assert.IsFalse(listed);
        }
    }
}
