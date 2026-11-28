using AbstractPixel.GameManagement.Debugging;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class StackNameFitTests
    {
        [Test]
        public void NamesThatFitKeepTheirWidth()
        {
            CollectionAssert.AreEqual(new[] { 30f, 40f }, StackNameFit.Widths(new[] { 30f, 40f }, 100f));
        }

        [Test]
        public void OnlyTheLongNameIsCutWhenShortOnesFit()
        {
            CollectionAssert.AreEqual(new[] { 30f, 40f, 130f }, StackNameFit.Widths(new[] { 30f, 40f, 300f }, 200f));
        }

        [Test]
        public void SeveralLongNamesShareTheRoomEvenly()
        {
            CollectionAssert.AreEqual(new[] { 85f, 30f, 85f }, StackNameFit.Widths(new[] { 300f, 30f, 200f }, 200f));
        }

        [Test]
        public void NoRoomGivesNoNegativeWidths()
        {
            CollectionAssert.AreEqual(new[] { 0f, 0f }, StackNameFit.Widths(new[] { 30f, 40f }, -5f));
        }
    }
}
