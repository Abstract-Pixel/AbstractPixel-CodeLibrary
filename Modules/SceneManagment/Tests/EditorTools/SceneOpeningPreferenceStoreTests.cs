using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// The two scene opening preferences (session 5A2) replace "Open the group with its main scene": per project, both on
    /// by default, and the old value carries over to both the first time each is read.
    /// </summary>
    public class SceneOpeningPreferenceStoreTests
    {
        private const string ProjectId = "project-a";
        private const string OtherProjectId = "project-b";

        private FakeBoolPreferenceStore prefs;
        private SceneOpeningPreferenceStore store;

        [SetUp]
        public void CreateStore()
        {
            prefs = new FakeBoolPreferenceStore();
            store = new SceneOpeningPreferenceStore(prefs, ProjectId);
        }

        [Test]
        public void BothAreOnByDefault()
        {
            Assert.IsTrue(store.MainSceneWithGroup, "main scene with its group");
            Assert.IsTrue(store.NoGroupSceneWithDefaultSet, "scene in no group with the default set");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TheOldValueCarriesOverToBoth(bool _oldValue)
        {
            prefs.SetBool(SceneOpeningPreferenceStore.OldKeyPrefix + ProjectId, _oldValue);

            Assert.AreEqual(_oldValue, store.MainSceneWithGroup, "main scene with its group");
            Assert.AreEqual(_oldValue, store.NoGroupSceneWithDefaultSet, "scene in no group with the default set");
        }

        [Test]
        public void TheCarriedOverValueIsWrittenSoALaterOldValueChangesNothing()
        {
            prefs.SetBool(SceneOpeningPreferenceStore.OldKeyPrefix + ProjectId, false);
            bool firstRead = store.MainSceneWithGroup;

            prefs.SetBool(SceneOpeningPreferenceStore.OldKeyPrefix + ProjectId, true);

            Assert.IsFalse(firstRead);
            Assert.IsFalse(store.MainSceneWithGroup);
            Assert.IsTrue(prefs.HasKey(SceneOpeningPreferenceStore.MainSceneWithGroupKeyPrefix + ProjectId), "written on first read");
        }

        [Test]
        public void ANewValueWinsOverTheOldOne()
        {
            prefs.SetBool(SceneOpeningPreferenceStore.OldKeyPrefix + ProjectId, true);
            prefs.SetBool(SceneOpeningPreferenceStore.NoGroupSceneWithDefaultSetKeyPrefix + ProjectId, false);

            Assert.IsTrue(store.MainSceneWithGroup);
            Assert.IsFalse(store.NoGroupSceneWithDefaultSet);
        }

        [Test]
        public void EachPreferenceChangesOnItsOwn()
        {
            store.MainSceneWithGroup = false;

            Assert.IsFalse(store.MainSceneWithGroup);
            Assert.IsTrue(store.NoGroupSceneWithDefaultSet);

            store.NoGroupSceneWithDefaultSet = false;
            store.MainSceneWithGroup = true;

            Assert.IsTrue(store.MainSceneWithGroup);
            Assert.IsFalse(store.NoGroupSceneWithDefaultSet);
        }

        [Test]
        public void EachProjectKeepsItsOwnValues()
        {
            store.MainSceneWithGroup = false;
            SceneOpeningPreferenceStore otherProject = new SceneOpeningPreferenceStore(prefs, OtherProjectId);

            Assert.IsTrue(otherProject.MainSceneWithGroup);
        }
    }
}
