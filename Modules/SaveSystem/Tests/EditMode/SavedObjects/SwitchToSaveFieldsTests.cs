using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // A developer switches a script from ISavable<T> to [Save] fields under the same save ID. Players' files still
    // hold the old shape, so the switch must read it, or say clearly why it can't.
    public class SwitchToSaveFieldsTests
    {
        class HealthAsSavable : ISavable<int>
        {
            public int Current = 100;
            public int CaptureData() => Current;
            public void RestoreData(int _loadedData) => Current = _loadedData;
        }

        class HealthAsOneField
        {
            [Save] public int current = 100;
        }

        class HealthAsTwoFields
        {
            [Save] public int current = 100;
            [Save] public int maximum = 100;
        }

        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            Relaunch();
            SaveActions.GameProfiles.CreateNew().Open();
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        void Relaunch()
        {
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new SaveJson(), new FakeSceneInfo(), () => 0d));
        }

        void SaveHealthTheOldWay(int value)
        {
            HealthAsSavable oldHealth = new HealthAsSavable();
            SaveActions.Register(oldHealth, "player-health");
            oldHealth.Current = value;
            SaveActions.SaveAll();
            int slotNumber = SaveActions.GameProfiles.Current.SlotNumber;
            Relaunch();
            SaveActions.GameProfiles.Get(slotNumber).Open();
        }

        [Test]
        public void OneSaveField_ReadsTheSingleValue_ISavableSaved()
        {
            SaveHealthTheOldWay(42);

            HealthAsOneField health = new HealthAsOneField();
            SaveActions.Register(health, "player-health");

            Assert.AreEqual(42, health.current, "A class with one [Save] field reads the one value its ISavable<int> used to save.");
        }

        [Test]
        public void SeveralSaveFields_LogOneErrorNamingTheFix_AndKeepNewGameValues()
        {
            SaveHealthTheOldWay(42);

            SaveLogAssert.Expect(LogType.Error, new Regex("single value.*ISavable.*exactly one \\[Save\\] field"));
            HealthAsTwoFields health = new HealthAsTwoFields();
            SaveActions.Register(health, "player-health");

            Assert.AreEqual(100, health.current);
            Assert.AreEqual(100, health.maximum);
        }
    }
}
