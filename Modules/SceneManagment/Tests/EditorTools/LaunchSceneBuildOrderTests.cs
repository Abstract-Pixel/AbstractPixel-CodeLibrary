using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>The launch scene at build index 0, and what the setup's optional launch scene step accepts.</summary>
    public class LaunchSceneBuildOrderTests
    {
        private const string Splash = "Assets/Splash.unity";

        [TestCase("Assets/Menu.unity,Assets/Splash.unity,Assets/Level.unity", "Assets/Splash.unity,Assets/Menu.unity,Assets/Level.unity")]
        [TestCase("Assets/Menu.unity,Assets/Level.unity", "Assets/Splash.unity,Assets/Menu.unity,Assets/Level.unity")]
        [TestCase("Assets/Splash.unity,Assets/Menu.unity", "Assets/Splash.unity,Assets/Menu.unity")]
        [TestCase("", "Assets/Splash.unity")]
        public void PutsTheLaunchSceneFirstAndKeepsTheOthersInOrder(string _before, string _expected)
        {
            EditorBuildSettingsScene[] result = LaunchSceneBuildOrder.WithLaunchSceneFirst(Scenes(_before, true), Splash);

            Assert.AreEqual(_expected, Paths(result));
            Assert.IsTrue(result[0].enabled, "the launch scene is switched on");
        }

        [Test]
        public void KeepsOtherScenesSwitchedOffWhenTheyWere()
        {
            EditorBuildSettingsScene[] result = LaunchSceneBuildOrder.WithLaunchSceneFirst(Scenes("Assets/Menu.unity", false), Splash);

            Assert.IsFalse(result[1].enabled);
        }

        [TestCase("Assets/Splash.unity,Assets/Menu.unity", true, true)]
        [TestCase("Assets/Menu.unity,Assets/Splash.unity", true, false)]
        [TestCase("Assets/Splash.unity", false, false)]
        [TestCase("", true, false)]
        public void IsFirstOnlyWhenIndexZeroAndSwitchedOn(string _scenes, bool _enabled, bool _expected)
        {
            Assert.AreEqual(_expected, LaunchSceneBuildOrder.IsFirst(Scenes(_scenes, _enabled), Splash));
        }

        [TestCase(LaunchSceneChoice.None, "", true)]
        [TestCase(LaunchSceneChoice.Existing, "", false)]
        [TestCase(LaunchSceneChoice.CreateNew, "Assets/Scenes/Never Made 4B.unity", true)]
        [TestCase(LaunchSceneChoice.CreateNew, "Scenes/Outside.unity", false)]
        [TestCase(LaunchSceneChoice.CreateNew, "Assets/Scenes/NoExtension", false)]
        public void SetupLaunchSceneStepAcceptsOnlyAnswersThatWork(LaunchSceneChoice _choice, string _newPath, bool _expectedOk)
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
            choices.LaunchScene = _choice;
            choices.NewLaunchScenePath = _newPath;

            string problem = SceneSystemSetup.LaunchSceneProblem(choices);

            Assert.AreEqual(_expectedOk, string.IsNullOrEmpty(problem), problem);
        }

        private static EditorBuildSettingsScene[] Scenes(string _paths, bool _enabled)
        {
            if (string.IsNullOrEmpty(_paths))
            {
                return new EditorBuildSettingsScene[0];
            }
            string[] paths = _paths.Split(',');
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[paths.Length];
            for (int index = 0; index < paths.Length; index++)
            {
                scenes[index] = new EditorBuildSettingsScene(paths[index], _enabled);
            }
            return scenes;
        }

        private static string Paths(EditorBuildSettingsScene[] _scenes)
        {
            string[] paths = new string[_scenes.Length];
            for (int index = 0; index < _scenes.Length; index++)
            {
                paths[index] = _scenes[index].path;
            }
            return string.Join(",", paths);
        }
    }
}
