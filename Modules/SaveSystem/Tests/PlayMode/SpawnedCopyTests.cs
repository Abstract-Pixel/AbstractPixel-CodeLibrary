using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // S5: a spawned copy needs its own save ID from SetSaveId. Uses a fake ISceneInfo (PlayMode tests can't see the
    // EditMode assembly's FakeSceneInfo) so this test's own scene can be marked "ready" without really reloading
    // it, the same way a spawned copy's scene is already ready by the time Instantiate creates it.
    public class SpawnedCopyTests
    {
        // The real scene this test runs in, since every GameObject created here reports this as its own scene.
        static string SceneName => SceneManager.GetActiveScene().name;

        sealed class FakeSceneInfo : ISceneInfo
        {
            public string ActiveSceneName => SceneName;
            public IReadOnlyList<string> LoadedSceneNames { get; } = new List<string> { SceneName };
            public event Action<string> SceneReady;
            public event Action<string> SceneUnloaded;
            public void SignalReady(string sceneName) => SceneReady?.Invoke(sceneName);
            public void SignalUnloaded(string sceneName) => SceneUnloaded?.Invoke(sceneName);
        }

        readonly List<GameObject> createdObjects = new List<GameObject>();
        FakeSceneInfo sceneInfo;

        [SetUp]
        public void SetUp()
        {
            sceneInfo = new FakeSceneInfo();
            SaveStartup.Start(CreateConfig(), new MemoryStorageService(), sceneInfo, () => Time.realtimeSinceStartupAsDouble);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                UnityEngine.Object.Destroy(createdObject);
            }
            createdObjects.Clear();
            SaveStartup.StartFromResources();
        }

        [UnityTest]
        public IEnumerator TwoSpawnedCopies_WithDistinctSaveIds_KeepTheirOwnData()
        {
            sceneInfo.SignalReady(SceneName);

            // Awake runs as soon as each copy is active; SetSaveId right after, still before Start next frame.
            InventoryForTests first = CreateSpawnedCopy();
            first.GetComponent<SavableBridge>().SetSaveId("goblin-1");
            InventoryForTests second = CreateSpawnedCopy();
            second.GetComponent<SavableBridge>().SetSaveId("goblin-2");
            yield return null;

            first.gold = 10;
            second.gold = 20;
            SaveActions.SaveAll();

            first.gold = 0;
            second.gold = 0;
            SaveActions.Reload<SaveFiles.GameProfile>();

            Assert.AreEqual(10, first.gold);
            Assert.AreEqual(20, second.gold);
        }

        [UnityTest]
        public IEnumerator CopySpawnedWithNoSaveId_LogsOneWarning_AndIsNotSaved()
        {
            sceneInfo.SignalReady(SceneName);

            InventoryForTests copy = CreateSpawnedCopy();
            LogAssert.Expect(LogType.Warning, new Regex("Inventory.*spawned after its scene was already loaded.*SetSaveId"));
            yield return null;

            copy.gold = 999;
            SaveActions.SaveAll();
            copy.gold = 0;
            SaveActions.Reload<SaveFiles.GameProfile>();

            Assert.AreEqual(0, copy.gold, "An unregistered copy's field is never captured, so reloading can't bring 999 back.");
        }

        [UnityTest]
        public IEnumerator ForgetOnASpawnedCopy_RemovesItsData_InsteadOfAGoneMarker()
        {
            sceneInfo.SignalReady(SceneName);

            InventoryForTests original = CreateSpawnedCopy();
            original.GetComponent<SavableBridge>().SetSaveId("goblin-1");
            yield return null;
            original.gold = 50;
            SaveActions.SaveAll();

            SaveActions.Forget(original.gameObject);
            UnityEngine.Object.Destroy(original.gameObject);
            SaveActions.SaveAll();
            yield return null;

            InventoryForTests respawned = CreateSpawnedCopy();
            respawned.GetComponent<SavableBridge>().SetSaveId("goblin-1");
            yield return null;

            Assert.IsTrue(respawned.gameObject.activeSelf, "Forgetting a spawned copy removes its data; it is not a gone marker, so a new copy under the same ID is not turned off.");
            Assert.AreEqual(10, respawned.gold, "No data remains, so the new copy starts with its own new-game value.");
        }

        // B3: an additive reload of the same scene while the old copy is still loaded must still refuse the
        // duplicate, not silently take over. The takeover rule in SaveRegistryTests only applies once the old
        // object is destroyed or its scene has unloaded, neither of which has happened here.
        [UnityTest]
        public IEnumerator AdditiveReloadOfTheSameScene_WhileTheOldCopyIsStillLoaded_RefusesTheDuplicate()
        {
            InventoryForTests original = CreateSceneObject("Chest GameObject #[same-guid]");
            yield return null;
            original.gold = 7;

            LogAssert.Expect(LogType.Error, new Regex(@"Two saved objects use the save ID 'same-guid/script-guid'.*'Chest'.*'Chest'"));
            InventoryForTests reloadedCopy = CreateSceneObject("Chest GameObject #[same-guid]");
            yield return null;

            Assert.AreEqual(7, original.gold, "The first object keeps its save ID and its data.");
            reloadedCopy.gold = 999;
            SaveActions.SaveAll();
            SaveActions.Reload<SaveFiles.GameProfile>();
            Assert.AreEqual(7, original.gold, "The refused duplicate was never tracked, so its change was never captured.");
        }

        // A scene object switched off and on again after its scene is ready is still a scene object, not a copy.
        [UnityTest]
        public IEnumerator SceneObjectTurnedOffAndOnAfterItsSceneIsReady_IsStillSaved()
        {
            InventoryForTests chest = CreateSceneObject("Chest GameObject #[chest-guid]");
            yield return null;
            sceneInfo.SignalReady(SceneName);

            chest.gameObject.SetActive(false);
            chest.gameObject.SetActive(true);
            chest.gold = 77;
            SaveActions.SaveAll();
            chest.gold = 0;
            SaveActions.Reload<SaveFiles.GameProfile>();

            LogAssert.NoUnexpectedReceived();
            Assert.AreEqual(77, chest.gold, "Re-enabling a scene object registers it again, so its change is still saved.");
        }

        // Mirrors Instantiate: by the time this returns, Awake has already run (so the bridge's reflection setup
        // is ready for SetSaveId to use), but Start is still a frame away, same as the real Instantiate timing.
        InventoryForTests CreateSpawnedCopy()
        {
            GameObject copyObject = new GameObject("Inventory(Clone)");
            createdObjects.Add(copyObject);
            copyObject.SetActive(false);

            SavableBridge bridge = copyObject.AddComponent<SavableBridge>();
            InventoryForTests inventory = copyObject.AddComponent<InventoryForTests>();
            SetField(bridge, "<UniqueId>k__BackingField", "Inventory(Clone) GameObject #[prefab-shared-guid]");
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(inventory, new SavableIdentification(nameof(InventoryForTests), "script-guid"))
            });

            copyObject.SetActive(true);
            return inventory;
        }

        InventoryForTests CreateSceneObject(string bridgeId)
        {
            GameObject sceneObject = new GameObject("Chest");
            createdObjects.Add(sceneObject);
            sceneObject.SetActive(false);

            SavableBridge bridge = sceneObject.AddComponent<SavableBridge>();
            InventoryForTests inventory = sceneObject.AddComponent<InventoryForTests>();
            SetField(bridge, "<UniqueId>k__BackingField", bridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(inventory, new SavableIdentification(nameof(InventoryForTests), "script-guid"))
            });
            sceneObject.SetActive(true);
            return inventory;
        }

        static SaveSystemConfigSO CreateConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetField(config, "<StartupPolicy>k__BackingField", SaveStartupPolicy.AutoSetUp);
            config.ReplaceSaveFileList(new List<SaveFileDefinition>());
            return config;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
