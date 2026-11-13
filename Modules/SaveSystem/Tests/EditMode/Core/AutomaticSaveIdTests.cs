using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // R2: a plain class registers with SaveActions.Register(this) and never types a save ID.
    public class AutomaticSaveIdTests
    {
        MemoryStorageService storage;
        SaveFileStore store;
        string gameProfileId;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            store = new SaveFileStore(storage, new SaveJson());
            StartSession();
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            gameProfileId = profile.Id;
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        void StartSession()
        {
            SaveActions.Stop();
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new SaveJson(), new FakeSceneInfo(), () => 0d));
        }

        void RelaunchAndOpenTheSameGameProfile()
        {
            StartSession();
            SaveActions.GameProfiles.GetById(gameProfileId).Open();
        }

        SaveFileData StoredGameFile()
        {
            Assert.IsTrue(store.TryRead($"GameProfiles/{gameProfileId}/Game", out SaveFileData file), "The Game save file was never written.");
            return file;
        }

        [Test]
        public void TwoObjectsWithTheSameCustomSaveId_OnlyTheFirstIsSaved()
        {
            SaveActions.Register(new PlayerHealth { Current = 40 }, "hero");
            LogAssert.Expect(LogType.Error, new Regex("Two saved objects use the save ID 'hero'"));
            SaveActions.Register(new PlayerHealth { Current = 70 }, "hero");

            SaveActions.SaveAll();

            Assert.AreEqual(40, StoredGameFile().Entries["hero"].Data.ToObject<int>());
        }

        [Test]
        public void RegisterWithNoSaveId_UsesTheClassName()
        {
            SaveActions.Register(new PlayerHealth { Current = 40 });

            SaveActions.SaveAll();

            Assert.AreEqual(40, StoredGameFile().Entries["PlayerHealth"].Data.ToObject<int>());
        }

        [Test]
        public void LaterInstancesOfTheSameClass_GetNumbersInTheOrderTheyRegister()
        {
            SaveActions.Register(new PlayerHealth { Current = 1 });
            SaveActions.Register(new PlayerHealth { Current = 2 });
            SaveActions.Register(new PlayerHealth { Current = 3 });

            SaveActions.SaveAll();

            SaveFileData file = StoredGameFile();
            Assert.AreEqual(1, file.Entries["PlayerHealth"].Data.ToObject<int>());
            Assert.AreEqual(2, file.Entries["PlayerHealth 2"].Data.ToObject<int>());
            Assert.AreEqual(3, file.Entries["PlayerHealth 3"].Data.ToObject<int>());
        }

        [Test]
        public void NumberedInstances_GetTheirOwnDataBackAfterARelaunch()
        {
            SaveActions.Register(new PlayerHealth { Current = 10 });
            SaveActions.Register(new PlayerHealth { Current = 20 });
            SaveActions.SaveAll();
            RelaunchAndOpenTheSameGameProfile();
            PlayerHealth first = new PlayerHealth();
            PlayerHealth second = new PlayerHealth();

            SaveActions.Register(first);
            SaveActions.Register(second);

            Assert.AreEqual(10, first.Current);
            Assert.AreEqual(20, second.Current);
        }

        [Test]
        public void AnUnregisteredInstancesNumber_IsFreeForTheNextNewInstance()
        {
            PlayerHealth first = new PlayerHealth { Current = 1 };
            PlayerHealth second = new PlayerHealth { Current = 2 };
            SaveActions.Register(first);
            SaveActions.Register(second);
            SaveActions.Unregister(second);

            // Like a reloaded scene: the new object takes the number the old one had, and with it the old one's data.
            PlayerHealth replacement = new PlayerHealth();
            SaveActions.Register(replacement);
            Assert.AreEqual(2, replacement.Current);
            replacement.Current = 22;
            SaveActions.SaveAll();

            Assert.AreEqual(22, StoredGameFile().Entries["PlayerHealth 2"].Data.ToObject<int>());
            Assert.IsFalse(StoredGameFile().Entries.ContainsKey("PlayerHealth 3"));
        }

        [Test]
        public void TheSameObjectRegisteringAgain_KeepsItsSaveId()
        {
            PlayerHealth first = new PlayerHealth { Current = 1 };
            PlayerHealth second = new PlayerHealth { Current = 2 };
            SaveActions.Register(first);
            SaveActions.Register(second);
            SaveActions.Unregister(second);

            SaveActions.Register(second);
            second.Current = 5;
            SaveActions.SaveAll();

            Assert.AreEqual(5, StoredGameFile().Entries["PlayerHealth 2"].Data.ToObject<int>());
        }

        [Test]
        public void TwoClassesWithTheSameName_TheSecondUsesItsFullName()
        {
            SaveActions.Register(new FirstShop.Wallet { Coins = 7 });
            SaveActions.Register(new SecondShop.Wallet { Coins = 9 });

            SaveActions.SaveAll();

            SaveFileData file = StoredGameFile();
            Assert.AreEqual(7, file.Entries["Wallet"].Data.ToObject<int>());
            Assert.AreEqual(9, file.Entries[typeof(SecondShop.Wallet).FullName].Data.ToObject<int>());
        }

        [Test]
        public void ACustomSaveId_AlwaysWins()
        {
            SaveActions.Register(new PlayerHealth { Current = 8 }, "player-health");
            SaveActions.Register(new PlayerHealth { Current = 9 });

            SaveActions.SaveAll();

            SaveFileData file = StoredGameFile();
            Assert.AreEqual(8, file.Entries["player-health"].Data.ToObject<int>());
            Assert.AreEqual(9, file.Entries["PlayerHealth"].Data.ToObject<int>());
        }

        static class FirstShop
        {
            public sealed class Wallet : ISavable<int, TestGameFile>
            {
                public int Coins;
                public int CaptureData() => Coins;
                public void RestoreData(int _loadedData) => Coins = _loadedData;
            }
        }

        static class SecondShop
        {
            public sealed class Wallet : ISavable<int, TestGameFile>
            {
                public int Coins;
                public int CaptureData() => Coins;
                public void RestoreData(int _loadedData) => Coins = _loadedData;
            }
        }
    }
}
