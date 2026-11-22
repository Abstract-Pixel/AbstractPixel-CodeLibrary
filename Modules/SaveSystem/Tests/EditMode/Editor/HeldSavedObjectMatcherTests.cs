using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // 2.1/2.2-2.6: which registered entries a GameObject's components hold, one level deep.
    public class HeldSavedObjectMatcherTests
    {
        class HeldByPrivateField : MonoBehaviour
        {
            object held;
            public void Hold(object value) => held = value;
        }

        class HeldByAutoProperty : MonoBehaviour
        {
            public object Held { get; set; }
        }

        class HeldInAList : MonoBehaviour
        {
            public List<object> Items = new List<object>();
        }

        class HoldsNothingRegistered : MonoBehaviour
        {
            public object Unregistered = new object();
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();

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
        public void APrivateFieldHoldingARegisteredObject_IsFound()
        {
            object held = new object();
            GameObject gameObject = NewGameObject();
            gameObject.AddComponent<HeldByPrivateField>().Hold(held);
            SavedObjectEntry entry = NewEntryFor(held);

            List<SavedObjectEntry> found = HeldSavedObjectMatcher.HeldBy(gameObject, new[] { entry });

            Assert.AreEqual(1, found.Count);
            Assert.AreSame(entry, found[0]);
        }

        [Test]
        public void AnAutoPropertyHoldingARegisteredObject_IsFound()
        {
            object held = new object();
            GameObject gameObject = NewGameObject();
            gameObject.AddComponent<HeldByAutoProperty>().Held = held;
            SavedObjectEntry entry = NewEntryFor(held);

            List<SavedObjectEntry> found = HeldSavedObjectMatcher.HeldBy(gameObject, new[] { entry });

            Assert.AreEqual(1, found.Count);
            Assert.AreSame(entry, found[0]);
        }

        [Test]
        public void AListElementHoldingARegisteredObject_IsFound()
        {
            object held = new object();
            GameObject gameObject = NewGameObject();
            gameObject.AddComponent<HeldInAList>().Items.Add(held);
            SavedObjectEntry entry = NewEntryFor(held);

            List<SavedObjectEntry> found = HeldSavedObjectMatcher.HeldBy(gameObject, new[] { entry });

            Assert.AreEqual(1, found.Count);
            Assert.AreSame(entry, found[0]);
        }

        [Test]
        public void AnUnregisteredObject_IsIgnored()
        {
            GameObject gameObject = NewGameObject();
            gameObject.AddComponent<HoldsNothingRegistered>();
            SavedObjectEntry entry = NewEntryFor(new object());

            Assert.AreEqual(0, HeldSavedObjectMatcher.HeldBy(gameObject, new[] { entry }).Count);
        }

        GameObject NewGameObject()
        {
            GameObject gameObject = new GameObject("Holder");
            gameObjects.Add(gameObject);
            return gameObject;
        }

        static SavedObjectEntry NewEntryFor(object savedObject)
        {
            SavedObjectEntry entry = new SavedObjectEntry(savedObject, "save-id", "Game", SaveLocation.GameProfile, null, "Entry", typeof(object), () => savedObject, _ => { })
            {
                SavedObject = savedObject
            };
            return entry;
        }
    }
}
