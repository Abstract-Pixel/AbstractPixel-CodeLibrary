using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    public class SavableBridgeTimingTests
    {
        const string BridgeId = "Health GameObject #[bridge-guid]";
        const string ScriptId = "script-guid";

        readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            // Memory storage keeps these tests away from real save files.
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
            // Put back the save system the game itself started, for the tests that run after these.
            SaveStartup.StartFromResources();
        }

        [UnityTest]
        public IEnumerator SavedValue_IsRestoredAfterAwake_AndBeforeStart()
        {
            yield return StoreSavedHealth(42);

            // The bridge comes first on purpose: it must still restore after the health script's Awake.
            HealthForTests health = CreateHealthObject(bridgeFirst: true);
            yield return null;

            Assert.AreEqual(42, health.ValueSeenInStart);
        }

        [UnityTest]
        public IEnumerator ObjectInstantiatedAfterTheSceneStarts_GetsItsDataBeforeItsFirstStart()
        {
            yield return StoreSavedHealth(42);
            HealthForTests template = CreateHealthObject(bridgeFirst: true, activate: false);

            GameObject spawned = Object.Instantiate(template.gameObject);
            createdObjects.Add(spawned);
            spawned.SetActive(true);
            yield return null;

            Assert.AreEqual(42, spawned.GetComponent<HealthForTests>().ValueSeenInStart);
        }

        [UnityTest]
        public IEnumerator DisablingAndReEnablingASavedObject_KeepsItsChangedValues()
        {
            yield return StoreSavedHealth(42);
            HealthForTests health = CreateHealthObject(bridgeFirst: false);
            yield return null;
            health.Current = 7;

            health.gameObject.SetActive(false);
            health.gameObject.SetActive(true);
            yield return null;

            Assert.AreEqual(7, health.Current, "Re-enabling must restore the value captured on disable, not the older saved one.");
        }

        [Test]
        public void SavableBridge_RunsBeforeOtherScripts()
        {
            DefaultExecutionOrder order = typeof(SavableBridge).GetCustomAttribute<DefaultExecutionOrder>();

            Assert.IsNotNull(order, "SavableBridge needs [DefaultExecutionOrder(-10000)].");
            Assert.AreEqual(-10000, order.order);
        }

        // Saves a health value under the test save ID, as if a previous play session had saved it.
        IEnumerator StoreSavedHealth(int value)
        {
            HealthForTests earlier = CreateHealthObject(bridgeFirst: false);
            yield return null;
            earlier.Current = value;
            SaveActions.SaveAll();
            Object.Destroy(earlier.gameObject);
            yield return null;
        }

        HealthForTests CreateHealthObject(bool bridgeFirst, bool activate = true)
        {
            GameObject healthObject = new GameObject("Health");
            createdObjects.Add(healthObject);
            healthObject.SetActive(false);

            SavableBridge bridge = bridgeFirst ? healthObject.AddComponent<SavableBridge>() : null;
            HealthForTests health = healthObject.AddComponent<HealthForTests>();
            if (bridge == null)
            {
                bridge = healthObject.AddComponent<SavableBridge>();
            }

            // The editor fills these in OnValidate; an object built in play mode sets them itself.
            SetField(bridge, "<UniqueId>k__BackingField", BridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(health, new SavableIdentification(nameof(HealthForTests), ScriptId))
            });

            healthObject.SetActive(activate);
            return health;
        }

        static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetField(config, "<StartupPolicy>k__BackingField", SaveStartupPolicy.AutoSetUp);
            SetField(config, "categoryDefinitionList", new List<SaveCatgeoryDefinition>
            {
                new SaveCatgeoryDefinition { Category = SaveCategory.Game, Location = SaveLocation.GameProfile }
            });
            return config;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
