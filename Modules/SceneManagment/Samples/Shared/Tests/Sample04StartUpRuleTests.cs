using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 04: Play started from the shared scene alone opens the Sample 04 scene group.</summary>
    public class Sample04StartUpRuleTests
    {
        private const string Folder = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/04 Play From Any Scene/";

        [Test]
        public void TheSharedSceneAlonePicksTheSample04Group()
        {
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(Folder + "Scene Groups/Sample 04 LevelGroup.asset");
            string sharedPath = Folder + "Scenes/Sample 04 Shared.unity";
            SceneReference shared = new SceneReference("Sample 04 Shared", AssetDatabase.AssetPathToGUID(sharedPath), sharedPath);

            StartUpChoice choice = StartUpRule.Choose(settings, new List<SceneReference> { shared });

            Assert.IsNotNull(settings, "the project has scene system settings");
            Assert.AreEqual(StartUpChoiceKind.SampleSceneGroupOfOpenScene, choice.Kind);
            Assert.AreSame(group, choice.Group);
        }
    }
}
