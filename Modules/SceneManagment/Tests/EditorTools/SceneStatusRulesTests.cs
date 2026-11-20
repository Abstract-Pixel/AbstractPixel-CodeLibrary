using AbstractPixel.Core.Editor;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Scene status: what the scene field shows, which fix it offers, and what its message says.</summary>
    public class SceneStatusRulesTests
    {
        private static SceneStatusFacts Facts(bool _hasSceneAsset, bool _hasStoredScene, bool _isInBuild, bool _isAddressable)
        {
            SceneStatusFacts facts = new SceneStatusFacts();
            facts.HasSceneAsset = _hasSceneAsset;
            facts.HasStoredScene = _hasStoredScene;
            facts.IsInBuild = _isInBuild;
            facts.IsAddressable = _isAddressable;
            return facts;
        }

        [TestCase(false, false, false, false, SceneStatus.NoScene)]
        [TestCase(false, true, false, false, SceneStatus.Missing)]
        [TestCase(false, true, true, true, SceneStatus.Missing)]
        [TestCase(true, true, true, false, SceneStatus.InBuild)]
        [TestCase(true, true, false, true, SceneStatus.Addressable)]
        [TestCase(true, true, false, false, SceneStatus.NotInBuild)]
        [TestCase(true, false, false, false, SceneStatus.NotInBuild)]
        public void Decide_ReturnsTheSceneStatus(bool _hasSceneAsset, bool _hasStoredScene, bool _isInBuild, bool _isAddressable, SceneStatus _expected)
        {
            SceneStatus status = SceneStatusRules.Decide(Facts(_hasSceneAsset, _hasStoredScene, _isInBuild, _isAddressable));

            Assert.AreEqual(_expected, status);
        }

        [TestCase(SceneStatus.NotInBuild, ScenePlace.BuildSettings, true, SceneFix.AddToBuild)]
        [TestCase(SceneStatus.NotInBuild, ScenePlace.BuildSettings, false, SceneFix.AddToBuild)]
        [TestCase(SceneStatus.NotInBuild, ScenePlace.Addressables, true, SceneFix.MakeAddressable)]
        [TestCase(SceneStatus.NotInBuild, ScenePlace.Addressables, false, SceneFix.AddToBuild)]
        [TestCase(SceneStatus.InBuild, ScenePlace.BuildSettings, true, SceneFix.None)]
        [TestCase(SceneStatus.Addressable, ScenePlace.BuildSettings, true, SceneFix.None)]
        [TestCase(SceneStatus.Missing, ScenePlace.BuildSettings, true, SceneFix.None)]
        [TestCase(SceneStatus.NoScene, ScenePlace.BuildSettings, true, SceneFix.None)]
        public void FixFor_OffersOneFixOnlyForNotInBuild(SceneStatus _status, ScenePlace _defaultPlace, bool _addressablesInstalled, SceneFix _expected)
        {
            SceneFix fix = SceneStatusRules.FixFor(_status, _defaultPlace, _addressablesInstalled);

            Assert.AreEqual(_expected, fix);
        }

        [TestCase(SceneFix.AddToBuild, "Add to build")]
        [TestCase(SceneFix.MakeAddressable, "Make Addressable")]
        [TestCase(SceneFix.None, "")]
        public void FixLabel_UsesTheGlossaryWords(SceneFix _fix, string _expected)
        {
            Assert.AreEqual(_expected, SceneStatusRules.FixLabel(_fix));
        }

        [TestCase(SceneStatus.InBuild, "in build")]
        [TestCase(SceneStatus.Addressable, "Addressable")]
        [TestCase(SceneStatus.NotInBuild, "not in build")]
        [TestCase(SceneStatus.Missing, "missing")]
        public void Message_NamesTheSceneAndItsStatus(SceneStatus _status, string _statusWords)
        {
            string message = SceneStatusRules.Message(_status, "Level 7", "Assets/Levels/Level 7.unity", true);

            StringAssert.Contains("Level 7", message);
            StringAssert.Contains(_statusWords, message);
        }

        [Test]
        public void Message_Missing_ShowsTheOldPath()
        {
            string message = SceneStatusRules.Message(SceneStatus.Missing, "Old Level", "Assets/Levels/Old Level.unity", true);

            StringAssert.Contains("Assets/Levels/Old Level.unity", message);
        }

        [Test]
        public void Message_NotInBuild_WithAddressables_NamesBothPlaces()
        {
            string message = SceneStatusRules.Message(SceneStatus.NotInBuild, "Level 7", "Assets/Level 7.unity", true);

            StringAssert.Contains("Build Settings", message);
            StringAssert.Contains("Addressables", message);
        }

        [Test]
        public void Message_NotInBuild_WithoutAddressables_NamesOnlyBuildSettings()
        {
            string message = SceneStatusRules.Message(SceneStatus.NotInBuild, "Level 7", "Assets/Level 7.unity", false);

            StringAssert.Contains("Build Settings", message);
            StringAssert.DoesNotContain("Addressables", message);
        }

        [Test]
        public void Message_NoScene_SaysNoSceneIsAssigned()
        {
            string message = SceneStatusRules.Message(SceneStatus.NoScene, string.Empty, string.Empty, true);

            StringAssert.Contains("No scene", message);
        }
    }
}
