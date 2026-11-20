using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // S3: the Savable Bridge recognises [Save] fields on a MonoBehaviour with no ISavable.
    public class SavableBridgeFieldTests
    {
        const string BridgeId = "Inventory GameObject #[bridge-guid]";
        const string ScriptId = "script-guid";

        readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            // Memory storage keeps this test away from real save files.
            SaveStartup.Start(CreateConfig(), new MemoryStorageService(), new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                Object.Destroy(createdObject);
            }
            createdObjects.Clear();
            // Put back the save system the game itself started, for the tests that run after this one.
            SaveStartup.StartFromResources();
        }

        [UnityTest]
        public IEnumerator SaveAttributeField_SavesAndReloads_WithNoISavable()
        {
            InventoryForTests inventory = CreateInventoryObject();
            yield return null;
            inventory.gold = 77;
            SaveActions.SaveAll();

            inventory.gold = 0;
            SaveActions.Reload<SaveFiles.GameProfile>();

            Assert.AreEqual(77, inventory.gold);
        }

        InventoryForTests CreateInventoryObject()
        {
            GameObject inventoryObject = new GameObject("Inventory");
            createdObjects.Add(inventoryObject);
            inventoryObject.SetActive(false);

            SavableBridge bridge = inventoryObject.AddComponent<SavableBridge>();
            InventoryForTests inventory = inventoryObject.AddComponent<InventoryForTests>();

            // The editor fills these in OnValidate; an object built in play mode sets them itself.
            SetField(bridge, "<UniqueId>k__BackingField", BridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(inventory, new SavableIdentification(nameof(InventoryForTests), ScriptId))
            });

            inventoryObject.SetActive(true);
            return inventory;
        }

        static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetField(config, "<StartupPolicy>k__BackingField", SaveStartupPolicy.AutoSetUp);
            // No rows: InventoryForTests has no ISaveIn, so it saves into the built-in GameProfile file.
            config.ReplaceSaveFileList(new List<SaveFileDefinition>());
            return config;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
