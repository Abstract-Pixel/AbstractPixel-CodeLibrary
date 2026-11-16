using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Tests
{
    // T1: the bridge inspector shows every saved script as its own SavedObjectCard.
    public class SavableBridgeEditorTests
    {
        class FieldScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        class FormerlyScript : MonoBehaviour
        {
            [Save(Formerly = "hp")] public int health = 100;
        }

        class SampleCaptureData
        {
            public int Score;
        }

        class SavableScript : MonoBehaviour, ISavable<SampleCaptureData>
        {
            public SampleCaptureData CaptureData() => new SampleCaptureData();
            public void RestoreData(SampleCaptureData loadedData)
            {
            }
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();
        readonly List<UnityEditor.Editor> editors = new List<UnityEditor.Editor>();
        const string TempPrefabFolder = "Assets/SavableBridgeEditorTestsTemp";

        [TearDown]
        public void TearDown()
        {
            SavedObjectCard.IsPlayingOverrideForTests = null;
            foreach (UnityEditor.Editor editor in editors)
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
            editors.Clear();
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }
            gameObjects.Clear();
            if (AssetDatabase.IsValidFolder(TempPrefabFolder))
            {
                AssetDatabase.DeleteAsset(TempPrefabFolder);
            }
        }

        [Test]
        public void ABridgeWithNoSavedScripts_ShowsOneLine_AndNoCards()
        {
            GameObject gameObject = new GameObject("Empty");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();

            VisualElement root = CreateInspectorFor(bridge);

            Assert.IsTrue(AllLabelTexts(root).Contains("Nothing on this GameObject is saved. You can remove this Savable Bridge."));
            Assert.AreEqual(0, CardsIn(root).Count);
        }

        [Test]
        public void ThreeSavedScripts_ShowThreeCards()
        {
            GameObject gameObject = new GameObject("ThreeScripts");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript fieldScript = gameObject.AddComponent<FieldScript>();
            SavableScript savableScript = gameObject.AddComponent<SavableScript>();
            FormerlyScript formerlyScript = gameObject.AddComponent<FormerlyScript>();
            SetSavableTargets(bridge, "bridge-guid",
                NewTarget(fieldScript, "FieldScript", "field-guid"),
                NewTarget(savableScript, "SavableScript", "savable-guid"),
                NewTarget(formerlyScript, "FormerlyScript", "formerly-guid"));

            VisualElement root = CreateInspectorFor(bridge);

            Assert.AreEqual(3, CardsIn(root).Count);
        }

        [Test]
        public void TheSameScriptTwice_NumbersTheHeaders()
        {
            GameObject gameObject = new GameObject("TwoFieldScripts");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript first = gameObject.AddComponent<FieldScript>();
            FieldScript second = gameObject.AddComponent<FieldScript>();
            SetSavableTargets(bridge, "bridge-guid",
                NewTarget(first, "FieldScript", "field-guid-1"),
                NewTarget(second, "FieldScript", "field-guid-2"));

            VisualElement root = CreateInspectorFor(bridge);

            // 2.1: each card also has its own (closed) Data foldout, which this query is not about.
            List<Foldout> foldouts = root.Query<Foldout>().ToList()
                .Where(foldout => !foldout.ClassListContains("saved-object-card__data-foldout")).ToList();
            Assert.AreEqual(2, foldouts.Count);
            Assert.IsTrue(foldouts[0].text.StartsWith("FieldScript (1)"));
            Assert.IsTrue(foldouts[1].text.StartsWith("FieldScript (2)"));
        }

        [Test]
        public void ARenamedField_ShowsItsFormerName()
        {
            GameObject gameObject = new GameObject("Formerly");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FormerlyScript script = gameObject.AddComponent<FormerlyScript>();
            SetSavableTargets(bridge, "bridge-guid", NewTarget(script, "FormerlyScript", "script-guid"));

            VisualElement root = CreateInspectorFor(bridge);

            Assert.IsTrue(AllLabelTexts(root).Contains("was: hp"));
        }

        [Test]
        public void AnISavableScript_ShowsItsDataTypeName()
        {
            GameObject gameObject = new GameObject("Savable");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            SavableScript script = gameObject.AddComponent<SavableScript>();
            SetSavableTargets(bridge, "bridge-guid", NewTarget(script, "SavableScript", "script-guid"));

            VisualElement root = CreateInspectorFor(bridge);

            Assert.IsTrue(AllLabelTexts(root).Contains("Custom data: SampleCaptureData"));
        }

        [Test]
        public void APrefabAsset_ShowsTheSetSaveIdNote_InsteadOfTheSaveIdButton()
        {
            // Writing a saved script's own type into a real prefab asset is covered by sample 05's Goblin prefab;
            // here only the bridge needs to be a genuine prefab asset. Its one SavableTarget can point at any
            // saved script instance, scene-placed or not, since the card only reads from the target it is given.
            GameObject looseScriptHolder = new GameObject("LooseFieldScriptHolder");
            gameObjects.Add(looseScriptHolder);
            FieldScript looseScript = looseScriptHolder.AddComponent<FieldScript>();

            GameObject sourceObject = new GameObject("PrefabBridge");
            gameObjects.Add(sourceObject);
            sourceObject.AddComponent<SavableBridge>();

            if (!AssetDatabase.IsValidFolder(TempPrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "SavableBridgeEditorTestsTemp");
            }
            string prefabPath = $"{TempPrefabFolder}/PrefabBridgeTest.prefab";
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(sourceObject, prefabPath, out bool saved);
            Assert.IsTrue(saved, "The test prefab asset failed to save.");
            SavableBridge prefabBridge = prefabAsset.GetComponent<SavableBridge>();
            SetSavableTargets(prefabBridge, "bridge-guid", NewTarget(looseScript, "FieldScript", "script-guid"));

            VisualElement root = CreateInspectorFor(prefabBridge);

            Assert.IsTrue(AllLabelTexts(root).Contains("Spawned copies get their save ID from SetSaveId"));
            Assert.IsFalse(root.Query<Button>().ToList().Any(button => button.text == "Show save ID"));
        }

        [Test]
        public void ShowSaveIdButton_RevealsTheBridgesOwnGetSaveId()
        {
            GameObject gameObject = new GameObject("Gold");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript script = gameObject.AddComponent<FieldScript>();
            SetField(bridge, "<UniqueId>k__BackingField", "Gold GameObject #[gold-guid]");
            SavableTarget savableTarget = NewTarget(script, "FieldScript", "script-guid");
            SetField(bridge, "savableTargets", new List<SavableTarget> { savableTarget });

            VisualElement root = CreateInspectorFor(bridge);
            SavedObjectCard card = CardsIn(root)[0];
            InvokePrivate(card, "ToggleSaveIdReveal");

            string expectedSaveId = bridge.GetSaveId(savableTarget);
            Assert.IsTrue(AllLabelTexts(root).Contains(expectedSaveId));
        }

        [Test]
        public void AWaitingSpawnedCopy_ShowsAWarning_InPlayMode()
        {
            GameObject gameObject = new GameObject("Waiting");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript script = gameObject.AddComponent<FieldScript>();
            SetSavableTargets(bridge, "bridge-guid", NewTarget(script, "FieldScript", "script-guid"));
            SetField(bridge, "isSpawnedCopy", true);

            SavedObjectCard.IsPlayingOverrideForTests = true;
            VisualElement root = CreateInspectorFor(bridge);
            InvokePrivate(CardsIn(root)[0], "RefreshLiveValues");

            HelpBox warning = root.Query<HelpBox>().ToList().FirstOrDefault(box => box.style.display.value == DisplayStyle.Flex);
            Assert.IsNotNull(warning, "No HelpBox is visible.");
            Assert.IsTrue(warning.text.Contains("SetSaveId"));
        }

        [Test]
        public void ALiveFieldValue_ShowsInPlayMode()
        {
            GameObject gameObject = new GameObject("LiveValue");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript script = gameObject.AddComponent<FieldScript>();
            script.gold = 42;
            SetSavableTargets(bridge, "bridge-guid", NewTarget(script, "FieldScript", "script-guid"));

            SavedObjectCard.IsPlayingOverrideForTests = true;
            VisualElement root = CreateInspectorFor(bridge);
            InvokePrivate(CardsIn(root)[0], "RefreshLiveValues");

            Label valueLabel = root.Query<Label>().Class("saved-object-card__field-value").ToList().First();
            Assert.AreEqual(DisplayStyle.Flex, valueLabel.style.display.value);
            Assert.IsTrue(valueLabel.text.Contains("42"));
        }

        [Test]
        public void InEditMode_ALiveFieldValue_StaysHidden()
        {
            GameObject gameObject = new GameObject("EditModeValue");
            gameObjects.Add(gameObject);
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript script = gameObject.AddComponent<FieldScript>();
            script.gold = 42;
            SetSavableTargets(bridge, "bridge-guid", NewTarget(script, "FieldScript", "script-guid"));

            VisualElement root = CreateInspectorFor(bridge);
            InvokePrivate(CardsIn(root)[0], "RefreshLiveValues");

            Label valueLabel = root.Query<Label>().Class("saved-object-card__field-value").ToList().First();
            Assert.AreEqual(DisplayStyle.None, valueLabel.style.display.value);
        }

        VisualElement CreateInspectorFor(SavableBridge bridge)
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(bridge);
            editors.Add(editor);
            return editor.CreateInspectorGUI();
        }

        static List<SavedObjectCard> CardsIn(VisualElement root)
        {
            return root.Query<SavedObjectCard>().ToList();
        }

        static SavableTarget NewTarget(MonoBehaviour script, string className, string guid)
        {
            return new SavableTarget(script, new SavableIdentification(className, guid));
        }

        static void SetSavableTargets(SavableBridge bridge, string uniqueId, params SavableTarget[] targets)
        {
            SetField(bridge, "<UniqueId>k__BackingField", $"{bridge.gameObject.name} GameObject #[{uniqueId}]");
            SetField(bridge, "savableTargets", new List<SavableTarget>(targets));
        }

        static List<string> AllLabelTexts(VisualElement root)
        {
            List<string> texts = new List<string>();
            root.Query<Label>().ForEach(label => texts.Add(label.text));
            return texts;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }

        static void InvokePrivate(object target, string methodName)
        {
            target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(target, null);
        }
    }
}
