using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // S4: IOnRestored fires after every restore: on register, for new-game values, and when a game profile opens.
    public class OnRestoredTests
    {
        class TrackedRestore : ISavable<int, TestGameFile>, IOnRestored
        {
            public int Value;
            public int RestoredCount;
            public int CaptureData() => Value;
            public void RestoreData(int loadedData) => Value = loadedData;
            public void OnRestored() => RestoredCount++;
        }

        class TrackedFieldRestore : IOnRestored
        {
            [Save] public int gold = 1;
            public int RestoredCount;
            public void OnRestored() => RestoredCount++;
        }

        class ExplodingOnRestored : ISavable<int, TestGameFile>, IOnRestored
        {
            public int CaptureData() => 1;
            public void RestoreData(int loadedData)
            {
            }
            public void OnRestored() => throw new System.Exception("OnRestored exploded");
        }

        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new SaveJson(), new FakeSceneInfo(), () => 0d));
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        [Test]
        public void RegisteringWithExistingData_FiresOnRestored()
        {
            TrackedRestore first = new TrackedRestore();
            SaveActions.Register(first, "tracked");
            SaveActions.GameProfiles.CreateNew().Open();
            first.Value = 9;
            SaveActions.SaveAll();
            SaveActions.Unregister(first);

            TrackedRestore second = new TrackedRestore();
            SaveActions.Register(second, "tracked");

            Assert.AreEqual(1, second.RestoredCount);
            Assert.AreEqual(9, second.Value);
        }

        [Test]
        public void RegisteringWithNoData_DoesNotFireOnRestored()
        {
            TrackedRestore first = new TrackedRestore();
            SaveActions.Register(first, "tracked");

            Assert.AreEqual(0, first.RestoredCount);
        }

        [Test]
        public void SaveAttributeField_AlsoFiresOnRestored()
        {
            TrackedFieldRestore first = new TrackedFieldRestore();
            SaveActions.Register(first, "tracked-field");
            SaveActions.GameProfiles.CreateNew().Open();
            first.gold = 55;
            SaveActions.SaveAll();
            SaveActions.Unregister(first);

            TrackedFieldRestore second = new TrackedFieldRestore();
            SaveActions.Register(second, "tracked-field");

            Assert.AreEqual(1, second.RestoredCount);
            Assert.AreEqual(55, second.gold);
        }

        [Test]
        public void NewGameValues_AlsoFireOnRestored()
        {
            TrackedRestore tracked = new TrackedRestore();
            SaveActions.Register(tracked, "tracked");
            SaveActions.GameProfiles.CreateNew().Open();
            tracked.RestoredCount = 0;

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.AreEqual(1, tracked.RestoredCount, "Slot 2 has no data for this object, so its new-game values are restored.");
        }

        [Test]
        public void ReopeningAGameProfileWithData_FiresOnRestored()
        {
            TrackedRestore tracked = new TrackedRestore();
            SaveActions.Register(tracked, "tracked");
            GameProfile slot1 = SaveActions.GameProfiles.CreateNew();
            slot1.Open();
            tracked.Value = 3;
            SaveActions.SaveAll();

            SaveActions.GameProfiles.CreateNew().Open();
            tracked.RestoredCount = 0;

            slot1.Open();

            Assert.AreEqual(1, tracked.RestoredCount);
            Assert.AreEqual(3, tracked.Value);
        }

        [Test]
        public void AThrowingOnRestored_IsLogged_AndTheRestoredDataStays()
        {
            ExplodingOnRestored first = new ExplodingOnRestored();
            SaveActions.Register(first, "exploding");
            SaveActions.GameProfiles.CreateNew().Open();
            SaveActions.SaveAll();
            SaveActions.Unregister(first);

            ExplodingOnRestored second = new ExplodingOnRestored();
            SaveLogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("OnRestored.*ExplodingOnRestored.*exploding.*OnRestored exploded"));
            Assert.DoesNotThrow(() => SaveActions.Register(second, "exploding"));
        }
    }
}
