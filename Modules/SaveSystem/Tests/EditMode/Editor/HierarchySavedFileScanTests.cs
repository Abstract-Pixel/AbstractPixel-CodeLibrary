using System.Collections.Generic;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // 2.2-2.6: whether a saved script's data is already in its save file on disk, read directly without a running
    // save system (edit mode has none). MemoryStorageService stands in for disk, the same way the save
    // system's own tests use it.
    public class HierarchySavedFileScanTests
    {
        class PlainScript : MonoBehaviour
        {
            [Save] public int value;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();
        MemoryStorageService storage;
        SaveFileStore store;
        SaveFileLookup lookup;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            store = new SaveFileStore(storage, new SaveJson());
            lookup = new SaveFileLookup(null);
        }

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
        }

        [Test]
        public void ATargetWithNoFileOnDisk_IsNotSaved()
        {
            SavableBridge bridge = NewBridgeWithTarget<PlainScript>(out SavableTarget target);

            Assert.IsFalse(HierarchySavedFileScan.AnyTargetSavedOnDisk(bridge, new[] { target }, lookup, store, storage));
        }

        [Test]
        public void ATargetWithAnEntryInAGameProfile_IsSaved()
        {
            SavableBridge bridge = NewBridgeWithTarget<PlainScript>(out SavableTarget target);
            string saveId = bridge.GetSaveId(target);
            WriteEntry("profile-1", saveId, gone: false);

            Assert.IsTrue(HierarchySavedFileScan.AnyTargetSavedOnDisk(bridge, new[] { target }, lookup, store, storage));
        }

        [Test]
        public void ATargetWhoseOnlyEntryIsAGoneMarker_IsNotSaved()
        {
            SavableBridge bridge = NewBridgeWithTarget<PlainScript>(out SavableTarget target);
            string saveId = bridge.GetSaveId(target);
            WriteEntry("profile-1", saveId, gone: true);

            Assert.IsFalse(HierarchySavedFileScan.AnyTargetSavedOnDisk(bridge, new[] { target }, lookup, store, storage));
        }

        [Test]
        public void ATargetSavedInADifferentGameProfile_IsStillFound()
        {
            SavableBridge bridge = NewBridgeWithTarget<PlainScript>(out SavableTarget target);
            string saveId = bridge.GetSaveId(target);
            WriteEntry("some-other-profile", saveId, gone: false);

            Assert.IsTrue(HierarchySavedFileScan.AnyTargetSavedOnDisk(bridge, new[] { target }, lookup, store, storage));
        }

        void WriteEntry(string gameProfileId, string saveId, bool gone)
        {
            string manifestKey = SavePathGenerator.GetGameProfileManifestKey(gameProfileId);
            storage.Write(manifestKey, "{}");

            SaveFileData data = new SaveFileData("GameProfile", gameProfileId);
            data.Entries[saveId] = new SaveFileEntry { Name = "Entry", Gone = gone, Data = gone ? null : JToken.FromObject(1) };
            string fileKey = SavePathGenerator.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, gameProfileId, null);
            store.Write(fileKey, data);
        }

        SavableBridge NewBridgeWithTarget<TScript>(out SavableTarget target) where TScript : MonoBehaviour
        {
            GameObject gameObject = new GameObject("Holder");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            MonoBehaviour script = gameObject.AddComponent<TScript>();
            SavableTarget createdTarget = new SavableTarget(script, new SavableIdentification(typeof(TScript).Name, "guid-0"));

            List<SavableTarget> targets = new List<SavableTarget> { createdTarget };
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            typeof(SavableBridge).GetField("savableTargets", flags).SetValue(bridge, targets);

            target = createdTarget;
            return bridge;
        }
    }
}
