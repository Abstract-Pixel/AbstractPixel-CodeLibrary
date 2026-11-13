using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// The Savable Bridge's inspector: each saved script's save file and saved fields, read only, and a button that
    /// copies its save ID to the clipboard.
    /// </summary>
    [CustomEditor(typeof(SavableBridge))]
    public class SavableBridgeEditor : UnityEditor.Editor
    {
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/SaveBridgeAndRegistration/SavableBridgeEditor.uss";

        public override VisualElement CreateInspectorGUI()
        {
            SavableBridge bridge = (SavableBridge)target;

            VisualElement root = new VisualElement();
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            root.Add(new PropertyField(serializedObject.FindProperty("<UniqueId>k__BackingField"), "Unique Id"));

            foreach (SavableTarget savableTarget in bridge.SavableTargets)
            {
                if (savableTarget == null || savableTarget.Script == null)
                {
                    continue;
                }
                root.Add(BuildTargetRow(bridge, savableTarget));
            }

            return root;
        }

        static VisualElement BuildTargetRow(SavableBridge bridge, SavableTarget savableTarget)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("savable-target");

            Label scriptNameLabel = new Label(savableTarget.Identification.ClassName);
            scriptNameLabel.AddToClassList("savable-target__script-name");
            row.Add(scriptNameLabel);

            row.Add(new Label($"Save file: {DescribeSaveFile(savableTarget)}"));
            row.Add(new Label($"Saved fields: {DescribeFields(savableTarget)}"));

            VisualElement idRow = new VisualElement();
            idRow.AddToClassList("savable-target__id-row");
            string saveId = bridge.GetSaveId(savableTarget);
            Label idLabel = new Label(saveId);
            idLabel.AddToClassList("savable-target__id-label");
            Button copyButton = new Button(() => EditorGUIUtility.systemCopyBuffer = saveId) { text = "Copy save ID" };
            idRow.Add(idLabel);
            idRow.Add(copyButton);
            row.Add(idRow);

            return row;
        }

        static string DescribeSaveFile(SavableTarget savableTarget)
        {
            Type saveFileClass = SavedScript.SaveFileClassOf(savableTarget.Script.GetType());
            return saveFileClass != null ? saveFileClass.Name : "GameProfile";
        }

        static string DescribeFields(SavableTarget savableTarget)
        {
            Type scriptClass = savableTarget.Script.GetType();
            if (SavedScript.FindSavableInterface(scriptClass) != null)
            {
                return "custom data (ISavable)";
            }

            string[] fieldNames = new SavedFieldSet(scriptClass).FieldNames.ToArray();
            return fieldNames.Length > 0 ? string.Join(", ", fieldNames) : "none";
        }
    }
}
