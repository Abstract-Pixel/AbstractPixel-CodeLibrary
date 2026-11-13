using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // S7: the UnityEvents designers wire with no code, plus Reload Chosen File, Open Slot N and New Game.
    // PlayMode, not EditMode: OnEnable is not guaranteed to run synchronously from AddComponent in edit mode,
    // but this component relies on it to subscribe to SaveEventBus.
    public class MonoSaveActionsTests
    {
        readonly List<GameObject> createdObjects = new List<GameObject>();
        MonoSaveActions actions;

        [SetUp]
        public void SetUp()
        {
            SaveStartup.Start(CreateConfig(), new MemoryStorageService(), new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
            GameObject gameObject = new GameObject("Save Actions");
            createdObjects.Add(gameObject);
            actions = gameObject.AddComponent<MonoSaveActions>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                Object.Destroy(createdObject);
            }
            createdObjects.Clear();
            SaveStartup.StartFromResources();
        }

        void ChooseFile<TFile>() where TFile : ISaveFile
        {
            SetField(actions, "chosenSaveFile", SaveFileReference.To<TFile>());
        }

        [UnityTest]
        public IEnumerator SaveChosenFile_FiresOnSaved_OnlyForTheChosenFile()
        {
            yield return null;
            ChooseFile<SaveFiles.GameProfile>();
            SaveActions.GameProfiles.CreateNew().Open();
            bool savedFired = false;
            actions.OnSaved.AddListener(() => savedFired = true);

            SaveActions.Save<SaveFiles.Settings>();
            Assert.IsFalse(savedFired, "Settings saved, not the chosen file.");

            actions.SaveChosenFile();
            Assert.IsTrue(savedFired);
        }

        [UnityTest]
        public IEnumerator OnLoaded_Fires_WhenTheChosenFileLoads()
        {
            yield return null;
            ChooseFile<SaveFiles.GameProfile>();
            bool loadedFired = false;
            actions.OnLoaded.AddListener(() => loadedFired = true);

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.IsTrue(loadedFired);
        }

        [UnityTest]
        public IEnumerator OnGameProfileOpened_Fires_WhenAGameProfileOpens()
        {
            yield return null;
            bool opened = false;
            actions.OnGameProfileOpened.AddListener(() => opened = true);

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.IsTrue(opened);
        }

        [Test]
        public void SaveChosenFile_WithNoFilePicked_LogsOneError()
        {
            LogAssert.Expect(LogType.Error, new Regex("no save file is picked"));

            actions.SaveChosenFile();
        }

        [UnityTest]
        public IEnumerator ReloadChosenFile_ThrowsAwayUnsavedChanges()
        {
            ChooseFile<SaveFiles.GameProfile>();
            SaveActions.GameProfiles.CreateNew().Open();
            InventoryForTests inventory = CreateInventoryObject();
            yield return null;
            inventory.gold = 40;
            SaveActions.SaveAll();
            inventory.gold = 999;

            actions.ReloadChosenFile();

            Assert.AreEqual(40, inventory.gold);
        }

        [Test]
        public void ReloadChosenFile_WithNoFilePicked_LogsOneError()
        {
            LogAssert.Expect(LogType.Error, new Regex("no save file is picked"));

            actions.ReloadChosenFile();
        }

        [Test]
        public void OpenSlot_OpensTheGameProfileInThatSlot()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();

            actions.OpenSlot(profile.SlotNumber);

            Assert.AreSame(profile, SaveActions.GameProfiles.Current);
        }

        [Test]
        public void OpenSlot_OnAnEmptySlot_LogsOneWarning_AndOpensNothing()
        {
            LogAssert.Expect(LogType.Warning, new Regex("slot 3.*empty"));

            actions.OpenSlot(3);

            Assert.IsNull(SaveActions.GameProfiles.Current);
        }

        [Test]
        public void NewGame_CreatesAndOpensAGameProfile()
        {
            actions.NewGame();

            Assert.IsNotNull(SaveActions.GameProfiles.Current);
            Assert.AreEqual(1, SaveActions.GameProfiles.Count);
        }

        InventoryForTests CreateInventoryObject()
        {
            GameObject inventoryObject = new GameObject("Inventory");
            createdObjects.Add(inventoryObject);
            inventoryObject.SetActive(false);

            SavableBridge bridge = inventoryObject.AddComponent<SavableBridge>();
            InventoryForTests inventory = inventoryObject.AddComponent<InventoryForTests>();
            SetField(bridge, "<UniqueId>k__BackingField", "Inventory GameObject #[inventory-guid]");
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(inventory, new SavableIdentification(nameof(InventoryForTests), "script-guid"))
            });

            inventoryObject.SetActive(true);
            return inventory;
        }

        static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetField(config, "<StartupPolicy>k__BackingField", SaveStartupPolicy.Manual);
            config.ReplaceSaveFileList(new List<SaveFileDefinition>());
            return config;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
