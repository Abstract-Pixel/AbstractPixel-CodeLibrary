using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // T3: a Savable Bridge is added while editing, never at play time or build time.
    public class SavableBridgeAdderTests
    {
        class FieldScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();
        readonly List<string> createdFolders = new List<string>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
            gameObjects.Clear();
            SavableBridgeAdder.IsPlayingOverrideForTests = null;

            foreach (string folder in createdFolders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.DeleteAsset(folder);
                }
            }
            createdFolders.Clear();
        }

        // A bridge added in play mode never gets its ID (OnValidate skips play mode) and is thrown away at Stop.
        // The scene check reports the scene instead.
        [Test]
        public void InPlayMode_NoBridgeIsAdded()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();
            SavableBridgeAdder.IsPlayingOverrideForTests = true;

            SavableBridgeAdder.EnsureBridges(new[] { gameObject });

            Assert.IsNull(gameObject.GetComponent<SavableBridge>());
        }

        [Test]
        public void AddingASavedScript_AddsTheBridge_InTheSameUndoGroup()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Add Component");
            int group = Undo.GetCurrentGroup();
            Undo.AddComponent<FieldScript>(gameObject);
            SavableBridgeAdder.EnsureBridges(new[] { gameObject });

            Assert.IsNotNull(gameObject.GetComponent<FieldScript>());
            Assert.IsNotNull(gameObject.GetComponent<SavableBridge>());

            Undo.RevertAllDownToGroup(group);

            Assert.IsNull(gameObject.GetComponent<FieldScript>(), "One Undo should remove the script too.");
            Assert.IsNull(gameObject.GetComponent<SavableBridge>(), "One Undo should remove the bridge it added in the same step.");
        }

        [Test]
        public void AScriptWithItsFirstSavedField_GetsABridgeWhenScanned()
        {
            GameObject gameObject = new GameObject("Lamp");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();

            SavableBridgeAdder.EnsureBridges(new[] { gameObject });

            Assert.IsNotNull(gameObject.GetComponent<SavableBridge>());
        }

        [Test]
        public void ASavedScriptOnAPrefabAsset_GetsItsBridgeInTheAsset()
        {
            // A top-level class, not one nested in this test fixture: a saved script nested in a test class doesn't
            // survive PrefabUtility.SaveAsPrefabAsset's round trip in this Unity version, which is a test-only quirk,
            // not something a real saved script hits. SaveTransform is a ready-made saver built on [Save].
            GameObject source = new GameObject("ChestPrefabSource");
            gameObjects.Add(source);
            source.AddComponent<SaveTransform>();
            GameObject prefabAsset = SaveAsTempPrefab(source, "ChestPrefab");

            SavableBridgeAdder.EnsureBridges(new[] { prefabAsset });

            Assert.IsNotNull(prefabAsset.GetComponent<SavableBridge>());
        }

        [Test]
        public void AScriptAddedAsAnInstanceOverride_GetsItsBridgeOnTheInstance_NotTheAsset()
        {
            GameObject source = new GameObject("EmptyPrefabSource");
            gameObjects.Add(source);
            GameObject prefabAsset = SaveAsTempPrefab(source, "EmptyPrefab");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            gameObjects.Add(instance);
            instance.AddComponent<SaveTransform>();

            SavableBridgeAdder.EnsureBridges(new[] { instance });

            Assert.IsNotNull(instance.GetComponent<SavableBridge>(), "The instance needs its own bridge: the asset has nothing to save.");
            Assert.IsNull(prefabAsset.GetComponent<SavableBridge>(), "The asset itself still has nothing to save.");
        }

        [Test]
        public void ARemovedBridge_ComesBackWithAWarning_InsteadOfAPlainAddedLog()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();
            SavableBridgeAdder.EnsureBridges(new[] { gameObject });
            Object.DestroyImmediate(gameObject.GetComponent<SavableBridge>());

            SaveLogAssert.Expect(LogType.Warning, new Regex("Chest.*still saves.*Savable Bridge"));
            SavableBridgeAdder.EnsureBridges(new[] { gameObject });

            Assert.IsNotNull(gameObject.GetComponent<SavableBridge>());
        }

        GameObject SaveAsTempPrefab(GameObject source, string prefabName)
        {
            const string folder = "Assets/SavableBridgeAdderTestsTemp";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets", "SavableBridgeAdderTestsTemp");
            }
            createdFolders.Add(folder);
            string prefabPath = $"{folder}/{prefabName}.prefab";
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(source, prefabPath, out bool saved);
            Assert.IsTrue(saved, "The test prefab asset failed to save.");
            return prefabAsset;
        }
    }
}
