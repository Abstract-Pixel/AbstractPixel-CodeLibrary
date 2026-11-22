using System.Collections.Generic;
using System.Linq;
using AbstractPixel.SaveSystem.Editor;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Tests
{
    // T5: the Saved Objects window. Entries are built by hand, the way SaveSession.Register would, so the
    // grouping, the card and the window's content are all tested without entering play mode.
    public class SavedObjectsWindowTests
    {
        class PlainSaved
        {
            [Save] public int gold;
        }

        class SavedAsset : ScriptableObject
        {
            [Save] public int level;
        }

        class FieldScript : MonoBehaviour
        {
            [Save] public int health = 7;
        }

        class SampleData
        {
            public int Score;
        }

        class SavableScript : MonoBehaviour, ISavable<SampleData>
        {
            public SampleData CaptureData() => new SampleData { Score = 3 };
            public void RestoreData(SampleData loadedData)
            {
            }
        }

        readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object unityObject in created)
            {
                if (unityObject != null)
                {
                    Object.DestroyImmediate(unityObject);
                }
            }
            created.Clear();
        }

        [Test]
        public void Grouping_PutsEntriesUnderTheirSaveFile_WithCounts_InNameOrder()
        {
            List<SavedObjectEntry> entries = new List<SavedObjectEntry>
            {
                EntryFor(new PlainSaved(), "b", "Settings", SaveLocation.Global, null),
                EntryFor(new PlainSaved(), "a", "Game", SaveLocation.GameProfile, null),
                EntryFor(new PlainSaved(), "c", "Game", SaveLocation.GameProfile, null)
            };

            List<SavedObjectGroup> groups = SavedObjectGrouping.Group(entries);

            CollectionAssert.AreEqual(new[] { "Game", "Settings" }, groups.Select(group => group.Title).ToList());
            CollectionAssert.AreEqual(new[] { 2, 1 }, groups.Select(group => group.Count).ToList());
        }

        [Test]
        public void Grouping_SplitsASceneSaveFileByScene()
        {
            List<SavedObjectEntry> entries = new List<SavedObjectEntry>
            {
                EntryFor(new PlainSaved(), "a", "Scene", SaveLocation.Scene, "Forest"),
                EntryFor(new PlainSaved(), "b", "Scene", SaveLocation.Scene, "Forest"),
                EntryFor(new PlainSaved(), "c", "Scene", SaveLocation.Scene, "Cave")
            };

            SavedObjectGroup sceneGroup = SavedObjectGrouping.Group(entries).Single();

            Assert.AreEqual(3, sceneGroup.Count);
            Assert.AreEqual(0, sceneGroup.Entries.Count, "Every entry of a Scene save file sits under its scene.");
            CollectionAssert.AreEqual(new[] { "Cave", "Forest" }, sceneGroup.Children.Select(child => child.Title).ToList());
            CollectionAssert.AreEqual(new[] { 1, 2 }, sceneGroup.Children.Select(child => child.Count).ToList());
        }

        [Test]
        public void Grouping_IgnoresTheSceneName_ForASaveFileThatIsNotPerScene()
        {
            List<SavedObjectEntry> entries = new List<SavedObjectEntry>
            {
                EntryFor(new PlainSaved(), "a", "Game", SaveLocation.GameProfile, "Forest")
            };

            SavedObjectGroup group = SavedObjectGrouping.Group(entries).Single();

            Assert.AreEqual(0, group.Children.Count);
            Assert.AreEqual(1, group.Entries.Count);
        }

        [Test]
        public void Grouping_NoEntries_NoGroups()
        {
            Assert.AreEqual(0, SavedObjectGrouping.Group(new List<SavedObjectEntry>()).Count);
        }

        [Test]
        public void TheSignature_ChangesWhenAnEntryIsAdded_AndStaysTheSameOtherwise()
        {
            SavedObjectEntry first = EntryFor(new PlainSaved(), "a", "Game", SaveLocation.GameProfile, null);
            SavedObjectEntry second = EntryFor(new PlainSaved(), "b", "Game", SaveLocation.GameProfile, null);

            string one = SavedObjectGrouping.Signature(new[] { first });
            string oneAgain = SavedObjectGrouping.Signature(new[] { first });
            string two = SavedObjectGrouping.Signature(new[] { first, second });

            Assert.AreEqual(one, oneAgain);
            Assert.AreNotEqual(one, two);
        }

        [Test]
        public void ABridgesScript_ReadsAsTheScriptOnTheGameObject()
        {
            GameObject chest = NewGameObject("Chest 2");
            FieldScript script = chest.AddComponent<FieldScript>();
            SavedObjectEntry entry = EntryFor(script, "chest-2", "Scene", SaveLocation.Scene, "Forest");

            SavedObjectDescription description = SavedObjectDescription.ForEntry(entry, new SaveJson());

            Assert.IsTrue(description.HeaderText.StartsWith("FieldScript on Chest 2"), description.HeaderText);
            Assert.IsTrue(description.HeaderText.Contains("Scene"));
            Assert.AreEqual(chest, description.PingTarget);
        }

        [Test]
        public void APlainClass_ReadsAsItsClassName_AndCannotBePinged()
        {
            SavedObjectEntry entry = EntryFor(new PlainSaved(), "plain", "Game", SaveLocation.GameProfile, null);

            SavedObjectDescription description = SavedObjectDescription.ForEntry(entry, new SaveJson());

            Assert.IsTrue(description.HeaderText.StartsWith("PlainSaved ·"), description.HeaderText);
            Assert.IsNull(description.PingTarget);
        }

        [Test]
        public void AScriptableObject_CanBePinged()
        {
            SavedAsset asset = ScriptableObject.CreateInstance<SavedAsset>();
            created.Add(asset);
            SavedObjectEntry entry = EntryFor(asset, "asset", "Game", SaveLocation.GameProfile, null);

            SavedObjectDescription description = SavedObjectDescription.ForEntry(entry, new SaveJson());

            Assert.AreEqual(asset, description.PingTarget);
        }

        [Test]
        public void AnISavable_ShowsItsDataTypeName()
        {
            GameObject owner = NewGameObject("Owner");
            SavableScript script = owner.AddComponent<SavableScript>();
            SavedObjectEntry entry = EntryFor(script, "savable", "Game", SaveLocation.GameProfile, null);

            SavedObjectDescription description = SavedObjectDescription.ForEntry(entry, new SaveJson());

            Assert.AreEqual("SampleData", description.CustomDataTypeName);
        }

        [Test]
        public void TheSaveId_IsTheEntrys()
        {
            SavedObjectEntry entry = EntryFor(new PlainSaved(), "the-id", "Game", SaveLocation.GameProfile, null);

            Assert.AreEqual("the-id", SavedObjectDescription.ForEntry(entry, new SaveJson()).GetSaveId());
        }

        [Test]
        public void TheCard_ShowsAPingButton_OnlyWhenThereIsSomethingToPing()
        {
            GameObject chest = NewGameObject("Chest");
            SavedObjectEntry withTarget = EntryFor(chest.AddComponent<FieldScript>(), "a", "Game", SaveLocation.GameProfile, null);
            SavedObjectEntry withoutTarget = EntryFor(new PlainSaved(), "b", "Game", SaveLocation.GameProfile, null);

            Assert.IsTrue(HasButton(CardFor(withTarget), "Ping"));
            Assert.IsFalse(HasButton(CardFor(withoutTarget), "Ping"));
        }

        [Test]
        public void TheWindowContent_ShowsACardForEachObject_UnderCountedGroupHeaders()
        {
            GameObject chest = NewGameObject("Chest");
            List<SavedObjectEntry> entries = new List<SavedObjectEntry>
            {
                EntryFor(new PlainSaved(), "plain", "Game", SaveLocation.GameProfile, null),
                EntryFor(chest.AddComponent<FieldScript>(), "chest", "Game", SaveLocation.GameProfile, null)
            };

            VisualElement content = SavedObjectsWindow.BuildContent(entries);

            Assert.AreEqual(2, content.Query<SavedObjectCard>().ToList().Count);
            List<string> foldoutTexts = content.Query<Foldout>().ToList().Select(foldout => foldout.text).ToList();
            Assert.IsTrue(foldoutTexts.Contains("Game (2)"), string.Join(" / ", foldoutTexts));
        }

        [Test]
        public void TheWindowContent_WithNothingRegistered_SaysSo()
        {
            VisualElement content = SavedObjectsWindow.BuildContent(new List<SavedObjectEntry>());

            Assert.IsTrue(content.Query<Label>().ToList().Any(label => label.text == SavedObjectsWindow.NothingRegisteredMessage));
        }

        [Test]
        public void TheEditModeMessage_PointsToPlayMode()
        {
            Assert.AreEqual("Enter play mode to see every saved object that registered.", SavedObjectsWindow.EditModeMessage);
        }

        GameObject NewGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            created.Add(gameObject);
            return gameObject;
        }

        // Built the way SaveSession.RegisterScript builds one: a bridge owns it, the script is the saved object.
        static SavedObjectEntry EntryFor(object savedObject, string saveId, string saveFileName, SaveLocation location, string sceneName)
        {
            SavedObjectEntry entry = new SavedObjectEntry(savedObject, saveId, saveFileName, location, sceneName,
                $"{sceneName}/{saveId}", typeof(JToken), () => new JObject(), _ => { });
            entry.SavedObject = savedObject;
            return entry;
        }

        static SavedObjectCard CardFor(SavedObjectEntry entry)
        {
            SavedObjectCard card = new SavedObjectCard();
            card.Bind(SavedObjectDescription.ForEntry(entry, new SaveJson()), true);
            return card;
        }

        static bool HasButton(VisualElement root, string text)
        {
            return root.Query<Button>().ToList().Any(button => button.text == text);
        }
    }
}
