using System.Collections.Generic;
using System.Linq;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Tests
{
    // T2: mistakes show on the saved-object card itself, from the same checks the Console uses. None of these
    // fixtures has a broken [Save] field (SavedFieldScan would block play mode for the whole project); the
    // field-level mistakes are handed to the card as text instead.
    public class SavedScriptMistakeTests
    {
        sealed class UnlistedSaveFile : ISaveFile { }

        sealed class ListedSaveFile : ISaveFile { }

        [SaveFileDefaults("Defaults File", SaveLocation.GameProfile)]
        sealed class DefaultsSaveFile : ISaveFile { }

        class PlainClassInUnlistedFile : ISaveIn<UnlistedSaveFile>
        {
            [Save] public int gold;
        }

        class PlainClassInListedFile : ISaveIn<ListedSaveFile>
        {
            [Save] public int gold;
        }

        class PlainClassInDefaultsFile : ISaveIn<DefaultsSaveFile>
        {
            [Save] public int gold;
        }

        class PlainClassWithNoFileChoice
        {
            [Save] public int gold;
        }

        class SampleData
        {
            public int Score;
        }

        class SavableInUnlistedFile : MonoBehaviour, ISavable<SampleData, UnlistedSaveFile>
        {
            public SampleData CaptureData() => new SampleData();
            public void RestoreData(SampleData loadedData)
            {
            }
        }

        class FieldScriptInUnlistedFile : MonoBehaviour, ISaveIn<UnlistedSaveFile>
        {
            [Save] public int gold;
        }

        class FieldScriptWithNoFileChoice : MonoBehaviour
        {
            [Save] public int gold;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();
        readonly List<UnityEditor.Editor> editors = new List<UnityEditor.Editor>();
        SaveSystemConfigSO config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new[]
            {
                SaveFileDefinition.ForClass(typeof(ListedSaveFile), "Listed", SaveLocation.GameProfile)
            });
        }

        [TearDown]
        public void TearDown()
        {
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
            UnityEngine.Object.DestroyImmediate(config);
        }

        // B14: the old check read only ISavable classes, so a plain [Save] class picking its file with ISaveIn slipped through.
        [Test]
        public void APlainClassInASaveFileWithNoRow_IsReported()
        {
            string problem = SaveFileRowCheck.ProblemWith(typeof(PlainClassInUnlistedFile), config);

            Assert.IsNotNull(problem);
            Assert.IsTrue(problem.Contains(typeof(UnlistedSaveFile).FullName));
            Assert.IsTrue(problem.Contains("Update save file classes"));
        }

        [Test]
        public void AnISavableWithATFileWithNoRow_IsReported()
        {
            string problem = SaveFileRowCheck.ProblemWith(typeof(SavableInUnlistedFile), config);

            Assert.IsNotNull(problem);
            Assert.IsTrue(problem.Contains(typeof(UnlistedSaveFile).FullName));
        }

        [Test]
        public void ASaveFileWithARow_IsNotReported()
        {
            Assert.IsNull(SaveFileRowCheck.ProblemWith(typeof(PlainClassInListedFile), config));
        }

        [Test]
        public void ASaveFileWithDefaults_IsNotReported_BecauseItsRowIsAddedForYou()
        {
            Assert.IsNull(SaveFileRowCheck.ProblemWith(typeof(PlainClassInDefaultsFile), config));
        }

        [Test]
        public void AClassWithNoFileChoice_IsNotReported()
        {
            Assert.IsNull(SaveFileRowCheck.ProblemWith(typeof(PlainClassWithNoFileChoice), config));
        }

        [Test]
        public void WithNoConfig_NothingIsReported()
        {
            Assert.IsNull(SaveFileRowCheck.ProblemWith(typeof(PlainClassInUnlistedFile), null));
        }

        [Test]
        public void BlockingMistakes_ListEveryProblemTheFieldSetFound()
        {
            SavedFieldSet fields = new SavedFieldSet(typeof(PlainClassInListedFile));
            fields.Problems.Add("First problem.");
            fields.Problems.Add("Second problem.");

            IReadOnlyList<string> mistakes = SavedScriptMistakes.BlockingMistakesOf(typeof(PlainClassInListedFile), fields);

            CollectionAssert.IsSubsetOf(new[] { "First problem.", "Second problem." }, mistakes.ToList());
        }

        [Test]
        public void AValidClass_HasNoBlockingMistakes()
        {
            SavedFieldSet fields = new SavedFieldSet(typeof(PlainClassInListedFile));

            Assert.AreEqual(0, SavedScriptMistakes.BlockingMistakesOf(typeof(PlainClassInListedFile), fields).Count);
        }

        [Test]
        public void AGameObjectWithAScriptInAnUnlistedSaveFile_HasAMistake()
        {
            GameObject gameObject = NewGameObject("Unlisted");
            gameObject.AddComponent<FieldScriptInUnlistedFile>();

            Assert.IsTrue(SavedScriptMistakes.AnyMistakeOn(gameObject));
        }

        [Test]
        public void AGameObjectWithAnOrdinarySavedScript_HasNoMistake()
        {
            GameObject gameObject = NewGameObject("Ordinary");
            gameObject.AddComponent<FieldScriptWithNoFileChoice>();

            Assert.IsFalse(SavedScriptMistakes.AnyMistakeOn(gameObject));
        }

        [Test]
        public void TheCard_ShowsEachBlockingMistakeAsAnErrorHelpBox()
        {
            SavedObjectDescription description = NewDescription();
            description.BlockingMistakes = new[] { "Mistake one.", "Mistake two." };

            List<HelpBox> errors = HelpBoxesOf(BoundCard(description), HelpBoxMessageType.Error);

            CollectionAssert.AreEqual(new[] { "Mistake one.", "Mistake two." }, errors.Select(box => box.text).ToList());
        }

        [Test]
        public void TheCard_ShowsAMissingRowAsAWarningHelpBox()
        {
            SavedObjectDescription description = NewDescription();
            description.MissingConfigRowWarning = "No row for this file.";

            List<HelpBox> warnings = HelpBoxesOf(BoundCard(description), HelpBoxMessageType.Warning)
                .Where(box => box.text == "No row for this file.").ToList();

            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void TheCard_WithNoMistakes_ShowsNoErrorOrMissingRowHelpBox()
        {
            VisualElement card = BoundCard(NewDescription());

            Assert.AreEqual(0, HelpBoxesOf(card, HelpBoxMessageType.Error).Count);
            Assert.AreEqual(0, HelpBoxesOf(card, HelpBoxMessageType.Warning).Count);
        }

        [Test]
        public void TheBridgeInspector_ShowsTheMissingRowWarning_OnTheScriptsCard()
        {
            GameObject gameObject = NewGameObject("Bridge");
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScriptInUnlistedFile script = gameObject.AddComponent<FieldScriptInUnlistedFile>();
            SetSavableTargets(bridge, new SavableTarget(script, new SavableIdentification("FieldScriptInUnlistedFile", "script-guid")));
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(bridge);
            editors.Add(editor);

            VisualElement root = editor.CreateInspectorGUI();

            bool warningShown = HelpBoxesOf(root, HelpBoxMessageType.Warning)
                .Any(box => box.text.Contains(typeof(UnlistedSaveFile).FullName));
            Assert.IsTrue(warningShown);
        }

        GameObject NewGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObjects.Add(gameObject);
            return gameObject;
        }

        static SavedObjectDescription NewDescription()
        {
            return new SavedObjectDescription
            {
                HeaderText = "Fixture",
                ViewDataKey = "fixture",
                FieldRows = new List<SavedObjectDescription.FieldRow>(),
                GetSaveId = () => "fixture-id",
                BlockingMistakes = new List<string>()
            };
        }

        static SavedObjectCard BoundCard(SavedObjectDescription description)
        {
            SavedObjectCard card = new SavedObjectCard();
            card.Bind(description, true);
            return card;
        }

        // The waiting-for-SetSaveId box is a hidden Warning, so only boxes that are showing count.
        static List<HelpBox> HelpBoxesOf(VisualElement root, HelpBoxMessageType type)
        {
            return root.Query<HelpBox>().ToList()
                .Where(box => box.messageType == type && box.style.display.value != DisplayStyle.None)
                .ToList();
        }

        static void SetSavableTargets(SavableBridge bridge, params SavableTarget[] targets)
        {
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            typeof(SavableBridge).GetField("<UniqueId>k__BackingField", flags)
                .SetValue(bridge, $"{bridge.gameObject.name} GameObject #[bridge-guid]");
            typeof(SavableBridge).GetField("savableTargets", flags).SetValue(bridge, new List<SavableTarget>(targets));
        }
    }
}
