using System;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    public class BuildProfileStatusTests
    {
        [Test]
        public void ASceneTheActiveBuildProfileLeavesOutIsNotInBuild()
        {
            EditorBuildSettingsScene listed = Array.Find(EditorBuildSettings.scenes, _scene => _scene.enabled && System.IO.File.Exists(_scene.path));
            Assert.IsNotNull(listed, "the project needs one scene in Build Settings for this test");
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(listed.path);
            Func<EditorBuildSettingsScene[]> profile = ActiveSceneList.ProfileOverride;
            ActiveSceneList.ProfileOverride = () => new EditorBuildSettingsScene[0];
            try
            {
                SceneStatusReport report = SceneStatusReader.Read(asset, asset.name, string.Empty, listed.path);

                Assert.AreNotEqual(SceneStatus.InBuild, report.Status);
            }
            finally
            {
                ActiveSceneList.ProfileOverride = profile;
            }
        }
    }
}
