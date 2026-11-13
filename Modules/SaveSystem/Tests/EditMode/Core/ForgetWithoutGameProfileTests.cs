using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // R3: Forget needs to know which game profile the data belongs to.
    public class ForgetWithoutGameProfileTests
    {
        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            StartSession();
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

        [Test]
        public void ForgetWithNoGameProfileOpen_LogsOneError_AndChangesNothing()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            SaveActions.Register(new PlayerHealth { Current = 40 }, "player-health");
            SaveActions.SaveAll();
            StartSession();
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health, "player-health");
            LogAssert.Expect(LogType.Error, new Regex("Forget.*PlayerHealth.*no game profile is open.*Open one"));

            SaveActions.Forget(health);
            SaveActions.GameProfiles.GetById(profile.Id).Open();

            Assert.AreEqual(40, health.Current, "The object is still registered and its data is still there, so opening the game profile restores it.");
        }

        [Test]
        public void ForgetOfAGlobalObject_WorksWithNoGameProfileOpen()
        {
            VolumeSetting volume = new VolumeSetting { Volume = 2 };
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();

            SaveActions.Forget(volume);
            SaveActions.SaveAll();
            StartSession();
            VolumeSetting nextTime = new VolumeSetting();
            SaveActions.Register(nextTime, "volume");

            Assert.AreEqual(5, nextTime.Volume, "A forgotten plain class has no data, so it keeps its new-game value.");
        }
    }
}
