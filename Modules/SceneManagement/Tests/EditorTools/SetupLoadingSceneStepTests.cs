using NUnit.Framework;
using UnityEditor;
using AbstractPixel.SceneManagement.Editor;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Session 4D (user request): the setup's optional Loading Scene step, beside the optional launch scene step.</summary>
    public class SetupLoadingSceneStepTests
    {
        [Test]
        public void TheReadyMadeLoadingSceneIsTheDefaultChoice()
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();

            Assert.AreEqual(LoadingSceneChoice.ReadyMade, choices.LoadingScene, "setup behaves as before this step existed");
        }

        [Test]
        public void UsingASceneIHaveNeedsAScene()
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
            choices.LoadingScene = LoadingSceneChoice.Existing;

            StringAssert.Contains("Pick the loading scene", SceneSystemSetup.LoadingSceneProblem(choices));
        }

        [Test]
        public void SkippingTheLoadingSceneHasNoProblemAndNoReadyLine()
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
            choices.LoadingScene = LoadingSceneChoice.None;

            Assert.AreEqual(string.Empty, SceneSystemSetup.LoadingSceneProblem(choices));
            Assert.AreEqual(string.Empty, SceneSystemSetup.LoadingSceneLine(choices));
        }

        [Test]
        public void APickedSceneIsNamedOnTheReadyPage()
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
            choices.LoadingScene = LoadingSceneChoice.Existing;
            string anyScenePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:SceneAsset")[0]);
            choices.ExistingLoadingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(anyScenePath);

            StringAssert.StartsWith("Loading scene '" + choices.ExistingLoadingScene.name + "'", SceneSystemSetup.LoadingSceneLine(choices));
            Assert.AreEqual(string.Empty, SceneSystemSetup.LoadingSceneProblem(choices));
        }
    }
}
