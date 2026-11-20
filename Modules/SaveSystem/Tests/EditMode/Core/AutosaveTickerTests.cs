using NUnit.Framework;
using UnityEngine;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // S7: autosave every N minutes. Ticks are driven by hand here, never by a real frame, so the test is instant.
    public class AutosaveTickerTests
    {
        AutosaveTicker ticker;
        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new SaveJson(), new FakeSceneInfo(), () => 0d));
            SaveActions.GameProfiles.CreateNew().Open();
            SaveActions.Register(new PlayerHealth { Current = 1 }, "player-health");

            ticker = new GameObject("Ticker").AddComponent<AutosaveTicker>();
            ticker.IntervalSeconds = 10f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(ticker.gameObject);
            SaveActions.Stop();
        }

        [Test]
        public void BeforeTheInterval_DoesNotSave()
        {
            ticker.Tick(9f);

            Assert.IsFalse(storage.Exists(GameKey));
        }

        [Test]
        public void AtTheInterval_Saves()
        {
            ticker.Tick(10f);

            Assert.IsTrue(storage.Exists(GameKey));
        }

        [Test]
        public void AfterSaving_TheTimerRestarts()
        {
            ticker.Tick(10f);
            storage.Delete(GameKey);

            ticker.Tick(5f);

            Assert.IsFalse(storage.Exists(GameKey), "Only 5 of the next 10 seconds have passed.");
        }

        static string GameKey => $"GameProfiles/{SaveActions.GameProfiles.Current.Id}/{GameFile}";
    }
}
