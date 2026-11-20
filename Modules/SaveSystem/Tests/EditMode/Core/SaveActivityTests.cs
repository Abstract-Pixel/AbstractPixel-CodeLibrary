using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // 2.0: when each saved object was last written and last restored, for the Hierarchy badge and the runtime previewer.
    public class SaveActivityTests
    {
        [TearDown]
        public void TearDown()
        {
            SaveActivity.Reset();
        }

        [Test]
        public void RecordWrite_IsReadBackForEverySaveIdGiven()
        {
            SaveActivity.RecordWrite("Game", new[] { "chest-1", "chest-2" });

            Assert.IsTrue(SaveActivity.TryGet("Game", "chest-1", out SaveActivity.Record first));
            Assert.IsNotNull(first.LastWrittenUtc);
            Assert.IsTrue(SaveActivity.TryGet("Game", "chest-2", out SaveActivity.Record second));
            Assert.IsNotNull(second.LastWrittenUtc);
        }

        [Test]
        public void RecordRestore_KeepsTheRestoredJson()
        {
            SaveActivity.RecordRestore("Game", "chest-1", "{\"gold\":5}");

            Assert.IsTrue(SaveActivity.TryGet("Game", "chest-1", out SaveActivity.Record record));
            Assert.IsNotNull(record.LastRestoredUtc);
            Assert.AreEqual("{\"gold\":5}", record.LastRestoredJson);
        }

        [Test]
        public void TryGet_WithNothingRecorded_ReturnsFalse()
        {
            Assert.IsFalse(SaveActivity.TryGet("Game", "nothing-here", out _));
        }

        [Test]
        public void Reset_ForgetsEveryRecord()
        {
            SaveActivity.RecordWrite("Game", new[] { "chest-1" });

            SaveActivity.Reset();

            Assert.IsFalse(SaveActivity.TryGet("Game", "chest-1", out _));
        }

        [Test]
        public void Changed_FiresOnARecordedWriteAndOnReset()
        {
            int fired = 0;
            SaveActivity.Changed += OnChanged;
            try
            {
                SaveActivity.RecordWrite("Game", new[] { "chest-1" });
                Assert.AreEqual(1, fired);

                SaveActivity.Reset();
                Assert.AreEqual(2, fired);
            }
            finally
            {
                SaveActivity.Changed -= OnChanged;
            }

            void OnChanged() => fired++;
        }
    }
}
