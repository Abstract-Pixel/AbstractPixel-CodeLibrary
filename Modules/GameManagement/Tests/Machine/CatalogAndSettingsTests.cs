using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    public class CatalogAndSettingsTests
    {
        private MachineTestRig rig;

        [SetUp]
        public void Build()
        {
            rig = new MachineTestRig();
        }

        [TearDown]
        public void CleanUp()
        {
            rig.Dispose();
        }

        [Test]
        public void HigherInTheListOutranks()
        {
            Assert.IsTrue(rig.Catalog.Outranks(rig.GameOver, rig.Play));
            Assert.IsFalse(rig.Catalog.Outranks(rig.Play, rig.MainMenu));
            Assert.IsFalse(rig.Catalog.Outranks(rig.Play, rig.Play));
        }

        [Test]
        public void ContainsOnlyListedStates()
        {
            StateSO stranger = rig.MakeState("Stranger", 1f);

            Assert.IsTrue(rig.Catalog.Contains(rig.Pause));
            Assert.IsFalse(rig.Catalog.Contains(stranger));
            Assert.IsFalse(rig.Catalog.Contains(null));
        }

        [Test]
        public void ACleanCatalogReportsNoProblems()
        {
            Assert.IsEmpty(rig.Catalog.FindEmptySlots());
            Assert.IsEmpty(rig.Catalog.FindDuplicates());
            Assert.IsEmpty(rig.Catalog.FindParentsMissingFromCatalog());
        }

        [Test]
        public void EmptySlotsAndDuplicatesAreFound()
        {
            rig.Catalog.States.Insert(1, null);
            rig.Catalog.States.Add(rig.Pause);
            rig.Catalog.States.Add(rig.Pause);

            CollectionAssert.AreEqual(new[] { 1 }, rig.Catalog.FindEmptySlots());
            CollectionAssert.AreEqual(new[] { rig.Pause }, rig.Catalog.FindDuplicates());
        }

        [Test]
        public void ParentsOutsideTheCatalogAreFound()
        {
            StateSO stranger = rig.MakeState("Stranger", 1f);
            rig.Pause.Parents.Add(stranger);

            var missing = rig.Catalog.FindParentsMissingFromCatalog();

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(rig.Pause, missing[0].subState);
            Assert.AreEqual(stranger, missing[0].parent);
        }

        [Test]
        public void NewSettingsKeepTheLastTimeScaleWhenNoStateIsActive()
        {
            GameManagementSettings fresh = ScriptableObject.CreateInstance<GameManagementSettings>();

            Assert.AreEqual(NoStateTimeScale.KeepLast, fresh.NoStateTimeScale);
            Object.DestroyImmediate(fresh);
        }

        [Test]
        public void NewStateFieldsLeaveTheOldFieldsAlone()
        {
            StateSO state = ScriptableObject.CreateInstance<StateSO>();

            Assert.AreEqual(0, state.Parents.Count);
            Assert.AreEqual(StateEffectSource.SameAsParent, state.Effects.TimeScaleSource);
            Object.DestroyImmediate(state);
        }

        [Test]
        public void SettingsSourcePrefersTheEditorLookup()
        {
            System.Func<GameManagementSettings> previous = GameManagementSettingsSource.EditorLookup;
            GameManagementSettingsSource.EditorLookup = () => rig.Settings;

            Assert.AreEqual(rig.Settings, GameManagementSettingsSource.Find());
            GameManagementSettingsSource.EditorLookup = previous;
        }

        [Test]
        public void PreloadedListGainsTheSettingsOnceAndDropsEmptyEntries()
        {
            Object other = rig.Catalog;
            Object[] start = { null, other, rig.Settings };

            Object[] result = Editor.GameManagementSettingsAssets.WithSettingsPreloaded(start, rig.Settings);

            CollectionAssert.AreEqual(new Object[] { other, rig.Settings }, result);
        }

        [Test]
        public void FacadeResetsToNoMachine()
        {
            GameStates.Use(rig.Machine);
            GameStates.Request(rig.Play);
            Assert.IsTrue(GameStates.IsActive(rig.Play));

            GameStates.ResetForNewPlaySession();

            Assert.IsFalse(GameStates.HasMachine);
        }
    }
}
