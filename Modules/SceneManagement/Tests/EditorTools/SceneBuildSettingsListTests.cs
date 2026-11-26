using System.Linq;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>The scene field's fixes on the Build Settings list: add or switch on one scene, or take it out.</summary>
    public class SceneBuildSettingsListTests
    {
        private const string LevelPath = "Assets/Levels/Level 1.unity";
        private const string ManagerPath = "Assets/Boot/Managers.unity";

        private static string[] PathsOf(EditorBuildSettingsScene[] _scenes)
        {
            return _scenes.Select(scene => scene.path).ToArray();
        }

        [Test]
        public void WithSceneSwitchedOn_SceneNotListed_IsAddedAtTheEndSwitchedOn()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(ManagerPath, true) };

            EditorBuildSettingsScene[] result = SceneBuildSettingsList.WithSceneSwitchedOn(current, LevelPath);

            CollectionAssert.AreEqual(new[] { ManagerPath, LevelPath }, PathsOf(result));
            Assert.IsTrue(result[1].enabled);
        }

        [Test]
        public void WithSceneSwitchedOn_SceneListedButOff_IsSwitchedOnInPlace()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(LevelPath, false), new EditorBuildSettingsScene(ManagerPath, true) };

            EditorBuildSettingsScene[] result = SceneBuildSettingsList.WithSceneSwitchedOn(current, LevelPath);

            CollectionAssert.AreEqual(new[] { LevelPath, ManagerPath }, PathsOf(result));
            Assert.IsTrue(result[0].enabled);
            Assert.IsFalse(current[0].enabled, "the array it was given must not change");
        }

        [Test]
        public void WithoutScene_RemovesEveryEntryOfThatScene_KeepsTheOthers()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(LevelPath, true), new EditorBuildSettingsScene(ManagerPath, false), new EditorBuildSettingsScene(LevelPath, false) };

            EditorBuildSettingsScene[] result = SceneBuildSettingsList.WithoutScene(current, LevelPath);

            CollectionAssert.AreEqual(new[] { ManagerPath }, PathsOf(result));
            Assert.IsFalse(result[0].enabled);
        }

        [Test]
        public void IsSwitchedOn_TrueOnlyForAListedSceneThatIsOn()
        {
            EditorBuildSettingsScene[] current = { new EditorBuildSettingsScene(LevelPath, true), new EditorBuildSettingsScene(ManagerPath, false) };

            Assert.IsTrue(SceneBuildSettingsList.IsSwitchedOn(current, LevelPath));
            Assert.IsFalse(SceneBuildSettingsList.IsSwitchedOn(current, ManagerPath));
            Assert.IsFalse(SceneBuildSettingsList.IsSwitchedOn(current, "Assets/Other.unity"));
        }
    }
}
