using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // R3 (bug 7): forgetting an object placed in a scene keeps it gone after its scene loads again.
    public class ForgetTests
    {
        const string BridgeId = "Coin GameObject #[coin-bridge-guid]";
        const string ScriptId = "coin-script-guid";

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

        // A new play session over the same save files. AutoSetUp opens the last game profile again.
        void Relaunch()
        {
            SaveStartup.Start(CreateConfig(), storage, new UnitySceneInfo(), () => Time.realtimeSinceStartupAsDouble);
        }

        [UnityTest]
        public IEnumerator ForgottenSceneObject_TurnsItselfOff_AfterARelaunch()
        {
            HealthForTests coin = CreateSceneObject();
            yield return null;

            SaveActions.Forget(coin.gameObject);
            Object.Destroy(coin.gameObject);
            SaveActions.SaveAll();
            yield return null;
            Relaunch();
            HealthForTests coinNextTime = CreateSceneObject();
            yield return null;

            Assert.IsFalse(coinNextTime.gameObject.activeSelf, "A forgotten coin must not come back when its scene loads again.");
            Assert.AreEqual(-1, coinNextTime.ValueSeenInStart, "The bridge turns the object off in its own Start, so the other scripts never start.");
        }

        [UnityTest]
        public IEnumerator ForgettingTheSavedScript_WritesAGoneMarkerInTheSaveFile()
        {
            HealthForTests coin = CreateSceneObject();
            yield return null;

            SaveActions.Forget(coin);
            Object.Destroy(coin.gameObject);
            SaveActions.SaveAll();
            yield return null;

            string key = $"GameProfiles/{SaveActions.GameProfiles.Current.Id}/GameProfile";
            Assert.IsTrue(storage.TryRead(key, out string json), "The GameProfile save file was never written.");
            StringAssert.Contains("\"gone\": true", json);
        }

        [UnityTest]
        public IEnumerator ForgottenSceneObject_TurnsItselfOff_InTheSameSession()
        {
            HealthForTests coin = CreateSceneObject();
            yield return null;

            SaveActions.Forget(coin.gameObject);
            Object.Destroy(coin.gameObject);
            yield return null;
            HealthForTests coinAfterSceneReload = CreateSceneObject();
            yield return null;

            Assert.IsFalse(coinAfterSceneReload.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator ANewGameProfile_HasNoGoneMarkers()
        {
            HealthForTests coin = CreateSceneObject();
            yield return null;
            SaveActions.Forget(coin.gameObject);
            Object.Destroy(coin.gameObject);
            SaveActions.SaveAll();
            yield return null;

            SaveActions.GameProfiles.CreateNew().Open();
            HealthForTests coinInTheNewGame = CreateSceneObject();
            yield return null;

            Assert.IsTrue(coinInTheNewGame.gameObject.activeSelf);
            Assert.AreEqual(100, coinInTheNewGame.ValueSeenInStart);
        }

        // An object placed in a scene: its bridge already has the save IDs the editor gave it.
        HealthForTests CreateSceneObject()
        {
            GameObject coinObject = new GameObject("Coin");
            createdObjects.Add(coinObject);
            coinObject.SetActive(false);
            HealthForTests coin = coinObject.AddComponent<HealthForTests>();
            SavableBridge bridge = coinObject.AddComponent<SavableBridge>();
            SetField(bridge, "<UniqueId>k__BackingField", BridgeId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(coin, new SavableIdentification(nameof(HealthForTests), ScriptId))
            });
            coinObject.SetActive(true);
            return coin;
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
