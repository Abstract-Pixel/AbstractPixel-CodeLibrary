using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // R5 (bug 1): an object that stays alive across a game profile switch gets its new-game values in a new game,
    // and from then on saves into the new game profile.
    public class NewGameValuesTests
    {
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
        public void PlainClassMadeAtStartup_GetsItsNewGameValues_InANewGameProfile_AndEachSlotKeepsItsOwn()
        {
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health);
            GameProfile slot1 = SaveActions.GameProfiles.CreateNew();
            slot1.Open();
            health.Current = 40;
            SaveActions.SaveAll();

            GameProfile slot2 = SaveActions.GameProfiles.CreateNew();
            slot2.Open();
            Assert.AreEqual(100, health.Current, "Slot 2 has no data for this object, so it starts a new game.");
            health.Current = 70;
            SaveActions.SaveAll();

            slot1.Open();
            Assert.AreEqual(40, health.Current);
            slot2.Open();
            Assert.AreEqual(70, health.Current, "Slot 2's values were saved into slot 2.");
        }

        [Test]
        public void GlobalObject_KeepsItsValues_WhenTheGameProfileChanges()
        {
            VolumeSetting volume = new VolumeSetting();
            SaveActions.Register(volume, "volume");
            SaveActions.GameProfiles.CreateNew().Open();
            volume.Volume = 2;

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.AreEqual(2, volume.Volume);
        }
    }
}
