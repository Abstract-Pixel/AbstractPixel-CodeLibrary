using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateTableColumnsTests
    {
        // Time, Cursor, HUD, Input: what each column's longest content needs, in pixels.
        private static readonly float[] Needs = { 54f, 124f, 70f, 64f };

        [Test]
        public void EveryValueColumnGetsAllItsLongestContentNeeds()
        {
            CollectionAssert.AreEqual(Needs, StateTableColumns.ValueWidths(Needs));
        }

        [Test]
        public void AColumnNeverGetsNarrowerThanItsHeading()
        {
            float[] widths = StateTableColumns.ValueWidths(new[] { 10f, 124f, 12f, 8f });

            Assert.AreEqual(StateTableColumns.LeastValue, widths[0]);
            Assert.AreEqual(124f, widths[1]);
        }

        [Test]
        public void TheTableStaysWhileEveryValueFitsAndParentKeepsARoomyName()
        {
            float least = 54f + 124f + 70f + 64f + StateTableColumns.ParentLeast + StateTableColumns.RowChrome;

            Assert.IsTrue(StateTableColumns.Fits(least, StateTableColumns.ValueWidths(Needs), 500f));
            Assert.IsFalse(StateTableColumns.Fits(least - 1f, StateTableColumns.ValueWidths(Needs), 500f));
        }

        [Test]
        public void ShortParentNamesNeedLessRoomSoTheTableStaysLonger()
        {
            float width = 54f + 124f + 70f + 64f + 70f + StateTableColumns.RowChrome;

            Assert.IsTrue(StateTableColumns.Fits(width, StateTableColumns.ValueWidths(Needs), 70f));
        }
    }
}
