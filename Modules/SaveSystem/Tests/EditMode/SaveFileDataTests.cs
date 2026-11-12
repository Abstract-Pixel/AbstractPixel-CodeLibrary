using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveFileDataTests
    {
        [Test]
        public void Constructor_KeepsTheFileNameItWasGiven()
        {
            // Bug 6: the v1 constructor ignored its file name argument.
            SaveFileData saveFile = new SaveFileData("Inventory", "a1b2c3");

            Assert.AreEqual("Inventory", saveFile.FileName);
            Assert.AreEqual("a1b2c3", saveFile.ProfileID);
        }

        [Test]
        public void NewSaveFile_HasTheCurrentFormatVersion()
        {
            Assert.AreEqual(2, new SaveFileData("Inventory").FormatVersion);
            Assert.AreEqual(2, SaveFileStore.CurrentFormatVersion);
        }
    }
}
