using System.Collections.Generic;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// Setup's words and scene set names (session 5A2): each Terminology line is one sentence of 8 to 15 words, a step
    /// explains at most three words and never one an earlier step explained, and a scene set name must be a free,
    /// valid file name.
    /// </summary>
    public class SceneSetupWordsTests
    {
        public static IEnumerable<string> Words()
        {
            return SceneSystemTerms.AllWords;
        }

        [TestCaseSource(nameof(Words))]
        public void EachDefinitionIsOneSentenceOfEightToFifteenWords(string _word)
        {
            string definition = SceneSystemTerms.Definition(_word);
            int words = SceneSystemTerms.WordCount(definition);

            Assert.That(words, Is.InRange(SceneSystemTerms.FewestWordsInDefinition, SceneSystemTerms.MostWordsInDefinition), _word + ": " + definition);
            StringAssert.EndsWith(".", definition);
            Assert.IsFalse(definition.TrimEnd('.').Contains(". "), "one sentence");
        }

        [Test]
        public void AStepExplainsAtMostThreeWordsAndEachWordOnce()
        {
            HashSet<string> explained = new HashSet<string>();
            foreach (string[] step in SceneSystemTerms.WordsPerStep)
            {
                Assert.LessOrEqual(step.Length, SceneSystemTerms.MostWordsPerStep);
                foreach (string word in step)
                {
                    Assert.IsTrue(explained.Add(word), word + " is explained twice");
                    Assert.IsNotEmpty(SceneSystemTerms.Definition(word), word + " has a definition");
                }
            }
        }

        [Test]
        public void SetupStepsAreInTheDecidedOrder()
        {
            Assert.AreEqual(7, SceneSystemTerms.WordsPerStep.Length, "1 Scene System to 7 Opening Scenes");
            CollectionAssert.AreEqual(new[] { SceneSystemTerms.SceneSet, SceneSystemTerms.ManagerScene, SceneSystemTerms.SharedScene }, SceneSystemTerms.WordsPerStep[5]);
            CollectionAssert.AreEqual(new[] { SceneSystemTerms.MainScene, SceneSystemTerms.SceneInNoGroup }, SceneSystemTerms.WordsPerStep[6]);
        }

        [TestCase("Boss Levels", "")]
        [TestCase("Main Menu Set", "")]
        [TestCase("", "name")]
        [TestCase("   ", "name")]
        [TestCase("Levels/Boss", "contain")]
        [TestCase("Boss?", "contain")]
        [TestCase(" Level", "start or end")]
        [TestCase("Level.", "start or end")]
        [TestCase("level", "already named")]
        [TestCase("Main Menu", "already named")]
        public void ASceneSetNameMustBeAFreeValidFileName(string _name, string _expectedWord)
        {
            string problem = SceneSetNames.Problem(_name, new[] { "Level", "Main Menu" });

            if (_expectedWord.Length == 0)
            {
                Assert.AreEqual(string.Empty, problem);
                return;
            }
            StringAssert.Contains(_expectedWord, problem);
        }

        [Test]
        public void AnotherSceneSetGetsTheFirstFreeName()
        {
            List<SceneSetDraft> drafts = new List<SceneSetDraft> { SceneSetDraft.Default() };
            drafts.Add(SceneSetDraft.Another(drafts));
            drafts.Add(SceneSetDraft.Another(drafts));

            Assert.AreEqual("Default Scene Set", drafts[0].Name);
            Assert.IsTrue(drafts[0].IncludeStarterManagers, "the default set lists the starter Managers scene");
            Assert.AreEqual("Scene Set", drafts[1].Name);
            Assert.AreEqual("Scene Set 2", drafts[2].Name);
            Assert.IsFalse(drafts[1].IncludeStarterManagers);
        }

        [Test]
        public void SetupNamesTheSceneSetProblem()
        {
            SceneSystemSetupChoices choices = new SceneSystemSetupChoices();
            choices.SetUpSceneSetsNow = true;
            choices.SceneSets.Add(SceneSetDraft.Another(choices.SceneSets));
            choices.SceneSets[1].Name = "default scene set";

            StringAssert.Contains("already named", SceneSystemSetup.SceneSetsProblem(choices));

            choices.SetUpSceneSetsNow = false;
            Assert.AreEqual(string.Empty, SceneSystemSetup.SceneSetsProblem(choices), "Not now makes only the Default Scene Set");
        }
    }
}
