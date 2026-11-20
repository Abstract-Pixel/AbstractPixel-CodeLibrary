using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // S1: [Save] fields on plain classes registered with SaveActions.Register(this), and rename safety.
    public class SaveAttributeTests
    {
        class Base
        {
            [Save] protected int inheritedGold = 10;
            public int InheritedGold => inheritedGold;
        }

        class Inventory : Base
        {
            [Save] int privatePotions = 1;
            [Save] public string publicName = "Bag";
            public int Potions => privatePotions;
        }

        class HealthBeforeRename
        {
            [Save] public int hp = 1;
        }

        class HealthAfterRename
        {
            [Save(Formerly = "hp")] public int health = 1;
        }

        [SaveId("Inventory")]
        class PlayerBagBeforeRename
        {
            [Save] public int gold = 5;
        }

        [SaveId("Inventory")]
        class PlayerBagAfterRename
        {
            [Save] public int gold = 5;
        }

        class SceneStash : ISaveIn<TestChestsFile>
        {
            [Save] public int coins = 3;
        }

        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new SaveJson(), new FakeSceneInfo(), () => 0d));
            SaveActions.GameProfiles.CreateNew().Open();
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        [Test]
        public void PrivatePublicAndInheritedFields_SaveAndReload()
        {
            Inventory inventory = new Inventory();
            SaveActions.Register(inventory);
            SetField(inventory, "privatePotions", 9);
            SetField(inventory, "inheritedGold", 99);
            inventory.publicName = "Satchel";
            SaveActions.SaveAll();

            SetField(inventory, "privatePotions", 0);
            SetField(inventory, "inheritedGold", 0);
            inventory.publicName = "Bag";
            SaveActions.Reload<SaveFiles.GameProfile>();

            Assert.AreEqual(9, inventory.Potions, "A private field saves and restores.");
            Assert.AreEqual(99, inventory.InheritedGold, "A field inherited from a base class saves and restores.");
            Assert.AreEqual("Satchel", inventory.publicName, "A public field saves and restores.");
        }

        [Test]
        public void RenamedField_WithFormerly_ReadsTheOldKey()
        {
            HealthBeforeRename before = new HealthBeforeRename();
            SaveActions.Register(before, "player-health");
            before.hp = 42;
            SaveActions.SaveAll();
            SaveActions.Unregister(before);

            HealthAfterRename after = new HealthAfterRename();
            SaveActions.Register(after, "player-health");

            Assert.AreEqual(42, after.health, "[Save(Formerly = \"hp\")] reads the data the old field name wrote.");
        }

        [Test]
        public void RenamedClass_WithMatchingSaveId_KeepsItsData()
        {
            PlayerBagBeforeRename before = new PlayerBagBeforeRename();
            SaveActions.Register(before);
            before.gold = 77;
            SaveActions.SaveAll();
            SaveActions.Unregister(before);

            PlayerBagAfterRename after = new PlayerBagAfterRename();
            SaveActions.Register(after);

            Assert.AreEqual(77, after.gold, "Both classes share [SaveId(\"Inventory\")], so the renamed class finds the same data.");
        }

        [Test]
        public void ISaveIn_PicksTheNamedSaveFile_InsteadOfGameProfile()
        {
            SceneStash stash = new SceneStash();
            SaveActions.Register(stash);
            stash.coins = 8;
            SaveActions.Save<TestChestsFile>();

            stash.coins = 0;
            SaveActions.Reload<TestChestsFile>();
            Assert.AreEqual(8, stash.coins);
        }

        [Test]
        public void PlainFieldClass_WithNoSaveFileChoice_DefaultsToGameProfile()
        {
            Inventory inventory = new Inventory();
            SaveActions.Register(inventory);
            inventory.publicName = "Pouch";
            SaveActions.SaveAll();

            inventory.publicName = "Bag";
            SaveActions.Reload<SaveFiles.GameProfile>();
            Assert.AreEqual("Pouch", inventory.publicName, "With no ISaveIn, the data lives in GameProfile.");
        }

        [Test]
        public void PlainFieldClass_GetsNewGameValues_InANewGameProfile()
        {
            Inventory inventory = new Inventory();
            SaveActions.Register(inventory);
            inventory.publicName = "Loot";
            SaveActions.SaveAll();

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.AreEqual("Bag", inventory.publicName, "Slot 2 has no data for this object, so it starts with its new-game values.");
        }
    }
}
