using System.Collections.Generic;
using AbstractPixel.GameManagement.Debugging;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateColorsTests
    {
        private MachineTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Dispose();
        }

        [Test]
        public void SixCatalogStatesAllDiffer()
        {
            HashSet<int> colors = new HashSet<int>();
            foreach (StateSO state in rig.Catalog.States)
            {
                int color = StateColors.IndexFor(state, rig.Catalog);
                Assert.That(color, Is.InRange(0, StateColors.Count - 1));
                colors.Add(color);
            }
            Assert.AreEqual(6, colors.Count);
        }

        [Test]
        public void EmptySlotsAndDuplicatesDoNotMakeTwoOfSixStatesShareAColor()
        {
            rig.Catalog.States.Insert(0, null);
            rig.Catalog.States.Insert(2, rig.GameOver);
            HashSet<int> colors = new HashSet<int>();
            foreach (StateSO state in new[] { rig.GameOver, rig.GameWin, rig.Settings_, rig.Pause, rig.MainMenu, rig.Play })
            {
                colors.Add(StateColors.IndexFor(state, rig.Catalog));
            }
            Assert.AreEqual(6, colors.Count);
        }

        [Test]
        public void TheSeventhStateRepeatsAColor()
        {
            StateSO seventh = rig.MakeState("Seventh", 1f);
            rig.Catalog.States.Add(seventh);
            Assert.AreEqual(StateColors.IndexFor(rig.GameOver, rig.Catalog), StateColors.IndexFor(seventh, rig.Catalog));
        }

        [Test]
        public void AStateOutsideTheCatalogGetsAColorFromItsNameAlone()
        {
            StateSO first = rig.MakeState("Stranger", 1f);
            StateSO second = rig.MakeState("Stranger", 1f);
            int color = StateColors.IndexFor(first, rig.Catalog);
            Assert.That(color, Is.InRange(0, StateColors.Count - 1));
            Assert.AreEqual(color, StateColors.IndexFor(second, null));
        }

        [Test]
        public void NoStateHasNoColor()
        {
            Assert.AreEqual(StateColors.None, StateColors.IndexFor(null, rig.Catalog));
        }
    }
}
