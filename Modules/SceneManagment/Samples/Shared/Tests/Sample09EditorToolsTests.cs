using System.Collections.Generic;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Sample 09: the Workshop group is a sample group whose group check finds nothing, and Create Scene Group on a copy
    /// of the loose scene makes a sample scene group (both copies are deleted again).
    /// </summary>
    public class Sample09EditorToolsTests
    {
        private const string Folder = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/09 Editor Tools/";
        private const string CopyPath = Folder + "Scenes/Sample 09 Loose Scene Copy.unity";

        private readonly List<string> createdPaths = new List<string>();

        [Test]
        public void TheWorkshopGroupIsASampleGroupWithNoProblem()
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(Folder + "Scene Groups/Sample 09 WorkshopGroup.asset");

            Assert.IsNotNull(group);
            Assert.IsTrue(group.IsSample);
            List<SceneGroupProblem> problems = SceneGroupCheck.Find(group);
            Assert.IsEmpty(problems, "group check problems");
        }

        [Test]
        public void CreateSceneGroupMakesASampleGroupForTheLooseScene()
        {
            bool copied = AssetDatabase.CopyAsset(Folder + "Scenes/Sample 09 Loose Scene.unity", CopyPath);
            Assert.IsTrue(copied, "copied the loose scene");
            createdPaths.Add(CopyPath);

            SceneAsset copy = AssetDatabase.LoadAssetAtPath<SceneAsset>(CopyPath);
            SceneGroup group = CreateSceneGroupMenu.CreateFor(copy);
            string groupPath = AssetDatabase.GetAssetPath(group);
            createdPaths.Add(groupPath);

            Assert.IsNotNull(group);
            Assert.IsTrue(group.IsSample, "a scene in a Samples folder gets a sample scene group");
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(CopyPath), group.MainScene.SceneGuid);
        }

        [TearDown]
        public void DeleteWhatTheTestMade()
        {
            foreach (string path in createdPaths)
            {
                if (string.IsNullOrEmpty(path) == false)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            createdPaths.Clear();
            SampleSceneGroups.Forget();
        }
    }
}
