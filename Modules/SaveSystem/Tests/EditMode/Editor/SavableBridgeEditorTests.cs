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
    // S7: the Copy save ID button, plus the read-only save file and saved fields list, on the bridge inspector.
    public class SavableBridgeEditorTests
    {
        class FieldScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        class SavableScript : MonoBehaviour, ISavable<int>
        {
            public int CaptureData() => 0;
            public void RestoreData(int loadedData)
            {
            }
        }

        GameObject gameObject;
        UnityEditor.Editor editor;

        [TearDown]
        public void TearDown()
        {
            if (editor != null)
            {
                Object.DestroyImmediate(editor);
            }
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ShowsTheSaveFileAndTheFieldNames_ForASaveAttributeScript()
        {
            List<string> labels = InspectorLabelsFor<FieldScript>("Gold GameObject #[gold-guid]");

            Assert.IsTrue(labels.Any(text => text.Contains("Save file: GameProfile")));
            Assert.IsTrue(labels.Any(text => text.Contains("Saved fields: gold")));
        }

        [Test]
        public void ShowsCustomData_ForAnISavableScript()
        {
            List<string> labels = InspectorLabelsFor<SavableScript>("Savable GameObject #[savable-guid]");

            Assert.IsTrue(labels.Any(text => text.Contains("Saved fields: custom data (ISavable)")));
        }

        [Test]
        public void ShowsTheSaveId_MatchingTheBridgesOwnGetSaveId()
        {
            gameObject = new GameObject("Gold");
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            FieldScript script = gameObject.AddComponent<FieldScript>();
            SetField(bridge, "<UniqueId>k__BackingField", "Gold GameObject #[gold-guid]");
            SavableTarget savableTarget = new SavableTarget(script, new SavableIdentification(nameof(FieldScript), "script-guid"));
            SetField(bridge, "savableTargets", new List<SavableTarget> { savableTarget });

            editor = UnityEditor.Editor.CreateEditor(bridge);
            VisualElement root = editor.CreateInspectorGUI();
            List<string> labels = AllLabelTexts(root);

            string expectedSaveId = bridge.GetSaveId(savableTarget);
            Assert.IsTrue(labels.Contains(expectedSaveId));
        }

        List<string> InspectorLabelsFor<TScript>(string uniqueId) where TScript : MonoBehaviour
        {
            gameObject = new GameObject("Target");
            SavableBridge bridge = gameObject.AddComponent<SavableBridge>();
            TScript script = gameObject.AddComponent<TScript>();
            SetField(bridge, "<UniqueId>k__BackingField", uniqueId);
            SetField(bridge, "savableTargets", new List<SavableTarget>
            {
                new SavableTarget(script, new SavableIdentification(typeof(TScript).Name, "script-guid"))
            });

            editor = UnityEditor.Editor.CreateEditor(bridge);
            VisualElement root = editor.CreateInspectorGUI();
            return AllLabelTexts(root);
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
    }
}
