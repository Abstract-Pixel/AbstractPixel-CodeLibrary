using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // S8: Save Transform, Save Active State and Save Destroyed, each a small component built on [Save].
    public class ReadyMadeSaversTests
    {
        const string BridgeId = "Crate GameObject #[crate-bridge-guid]";
        const string ScriptId = "crate-script-guid";

        readonly List<GameObject> createdObjects = new List<GameObject>();
        MemoryStorageService storage;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            Relaunch();
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

        void Relaunch()
        {
            SaveStartup.Start(CreateConfig(), storage, new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        [UnityTest]
        public IEnumerator SaveTransform_RestoresPositionRotationAndScale_AfterARelaunch()
        {
            SaveTransform crate = CreateSceneObject<SaveTransform>();
            yield return null;
            crate.transform.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 90f, 0f));
            crate.transform.localScale = new Vector3(2f, 2f, 2f);
            yield return null;
            SaveActions.SaveAll();

            Relaunch();
            SaveTransform crateNextTime = CreateSceneObject<SaveTransform>();
            yield return null;

            Assert.AreEqual(new Vector3(1f, 2f, 3f), crateNextTime.transform.position);
            Assert.Less(Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f), crateNextTime.transform.rotation), 0.01f);
            Assert.AreEqual(new Vector3(2f, 2f, 2f), crateNextTime.transform.localScale);
        }

        [UnityTest]
        public IEnumerator SaveActiveState_RestoresBeingTurnedOff_AfterARelaunch()
        {
            SaveActiveState crate = CreateSceneObject<SaveActiveState>();
            yield return null;

            crate.gameObject.SetActive(false);
            SaveActions.SaveAll();
            yield return null;

            Relaunch();
            SaveActiveState crateNextTime = CreateSceneObject<SaveActiveState>();
            yield return null;

            Assert.IsFalse(crateNextTime.gameObject.activeSelf, "The crate was turned off before the save, so it starts off next time too.");
        }

        [UnityTest]
        public IEnumerator SaveActiveState_CanBeTurnedBackOn_AndRemembersBeingOn()
        {
            SaveActiveState lamp = CreateSceneObject<SaveActiveState>();
            yield return null;
            lamp.gameObject.SetActive(false);
            yield return null;

            lamp.gameObject.SetActive(true);
            yield return null;
            Assert.IsTrue(lamp.gameObject.activeSelf, "Turning it back on must not be undone by the value saved when it was turned off.");
            SaveActions.SaveAll();

            Relaunch();
            SaveActiveState lampNextTime = CreateSceneObject<SaveActiveState>();
            yield return null;
            Assert.IsTrue(lampNextTime.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SaveActiveState_StaysOn_WhenItWasNeverTurnedOff()
        {
            SaveActiveState crate = CreateSceneObject<SaveActiveState>();
            yield return null;
            SaveActions.SaveAll();

            Relaunch();
            SaveActiveState crateNextTime = CreateSceneObject<SaveActiveState>();
            yield return null;

            Assert.IsTrue(crateNextTime.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SaveDestroyed_ForgetsTheObject_WithNoExplicitForgetCall()
        {
            GameObject crateObject = CreateSceneGameObject<InventoryForTests>();
            crateObject.AddComponent<SaveDestroyed>();
            crateObject.SetActive(true);
            yield return null;
            crateObject.GetComponent<InventoryForTests>().gold = 99;
            SaveActions.SaveAll();

            crateObject.GetComponent<SaveDestroyed>().DestroyAndForget();
            SaveActions.SaveAll();
            yield return null;

            Relaunch();
            GameObject crateNextTime = CreateSceneGameObject<InventoryForTests>();
            crateNextTime.SetActive(true);
            yield return null;

            Assert.IsFalse(crateNextTime.activeSelf, "DestroyAndForget forgot the crate, so it stays gone like any other forgotten scene object.");
        }

        [UnityTest]
        public IEnumerator SaveDestroyed_StaysGone_OnAnObjectWithNothingElseSaved()
        {
            // This test builds the bridge by hand, so check the rule the real bridge and the auto-adder use to pick scripts.
            Assert.IsTrue(SavedScript.IsSaved(typeof(SaveDestroyed)), "Save Destroyed must count as a saved script, or a coin with nothing else saved never gets a Savable Bridge.");
            SaveDestroyed coin = CreateSceneObject<SaveDestroyed>();
            yield return null;

            coin.DestroyAndForget();
            SaveActions.SaveAll();
            yield return null;

            Relaunch();
            SaveDestroyed coinNextTime = CreateSceneObject<SaveDestroyed>();
            yield return null;

            Assert.IsFalse(coinNextTime.gameObject.activeSelf, "A coin with only Save Destroyed on it is the usual designer case, so it must stay gone too.");
        }

        T CreateSceneObject<T>() where T : MonoBehaviour
        {
            GameObject gameObject = CreateSceneGameObject<T>();
            gameObject.SetActive(true);
            return gameObject.GetComponent<T>();
        }

        GameObject CreateSceneGameObject<T>() where T : MonoBehaviour
        {
            GameObject gameObject = new GameObject("Crate");
            createdObjects.Add(gameObject);
            gameObject.SetActive(false);
            T script = gameObject.AddComponent<T>();
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            SetField(bridge, "<UniqueId>k__BackingField", BridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(script, new SavableIdentification(typeof(T).Name, ScriptId))
            });
            return gameObject;
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
