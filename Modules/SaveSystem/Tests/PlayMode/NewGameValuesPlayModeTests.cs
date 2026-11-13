using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // R5 (bug 1): a DontDestroyOnLoad manager gets its new-game values when a new game profile opens.
    public class NewGameValuesPlayModeTests
    {
        GameObject manager;

        [SetUp]
        public void SetUp()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new List<SaveFileDefinition>());
            SaveStartup.Start(config, new MemoryStorageService(), new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(manager);
            SaveStartup.StartFromResources();
        }

        [UnityTest]
        public IEnumerator DontDestroyOnLoadManager_GetsItsNewGameValues_InSlot2_AndSlot1KeepsItsOwn()
        {
            HealthForTests health = CreateManager();
            yield return null;
            GameProfile slot1 = SaveActions.GameProfiles.CreateNew();
            slot1.Open();
            health.Current = 40;
            SaveActions.SaveAll();

            GameProfile slot2 = SaveActions.GameProfiles.CreateNew();
            slot2.Open();
            Assert.AreEqual(100, health.Current, "Slot 2 has no data for the manager, so it starts a new game.");
            health.Current = 70;
            SaveActions.SaveAll();

            slot1.Open();
            Assert.AreEqual(40, health.Current);
            slot2.Open();
            Assert.AreEqual(70, health.Current);
        }

        HealthForTests CreateManager()
        {
            manager = new GameObject("Health Manager");
            manager.SetActive(false);
            Object.DontDestroyOnLoad(manager);
            HealthForTests health = manager.AddComponent<HealthForTests>();
            SavableBridge bridge = manager.AddComponent<SavableBridge>();
            SetField(bridge, "<UniqueId>k__BackingField", "Health Manager GameObject #[manager-bridge-guid]");
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(health, new SavableIdentification(nameof(HealthForTests), "manager-script-guid"))
            });
            manager.SetActive(true);
            return health;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
