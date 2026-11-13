using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// The save system config's inspector. When the save file list changes, the built-in and module rows are put
    /// back if they were removed, and SaveFiles.cs is written again, so the classes always match the list.
    /// </summary>
    [CustomEditor(typeof(SaveSystemConfigSO))]
    public class SaveSystemConfigEditor : UnityEditor.Editor
    {
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/Config/SaveSystemConfigEditor.uss";

        bool isWriteScheduled;

        public static StyleSheet LoadStyleSheet()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.styleSheets.Add(LoadStyleSheet());

            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                bool isSaveFileList = property.name == "saveFileList";
                if (!isSaveFileList)
                {
                    root.Add(new PropertyField(property.Copy()));
                }
            }

            root.Add(new HelpBox("The save file list. Each row is one save file. The editor writes a class per row into SaveFiles.cs, so game code writes SaveActions.Save<SaveFiles.Ghosts>(). Global, GameProfile and Scene are built in, and library modules add their own rows.", HelpBoxMessageType.Info));

            SerializedProperty listProperty = serializedObject.FindProperty("saveFileList");
            // The list is what people come here for, so it starts open.
            listProperty.isExpanded = true;
            PropertyField listField = new PropertyField(listProperty, "Save File List");
            listField.TrackPropertyValue(listProperty, changed => ScheduleWrite());
            root.Add(listField);

            Button writeButton = new Button(WriteNow);
            writeButton.text = "Write SaveFiles.cs";
            writeButton.tooltip = "Write SaveFiles.cs from the list now. It is also written whenever the list changes.";
            writeButton.AddToClassList("save-file-list__actions");
            root.Add(writeButton);
            return root;
        }

        // Typing a name changes the list on every key, so the file is written once the editor is idle again.
        void ScheduleWrite()
        {
            if (isWriteScheduled)
            {
                return;
            }
            isWriteScheduled = true;
            EditorApplication.delayCall += WriteNow;
        }

        void WriteNow()
        {
            isWriteScheduled = false;
            SaveSystemConfigSO config = target as SaveSystemConfigSO;
            if (config == null)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
            SaveFilesCodeWriter.WriteFile(config);
            serializedObject.Update();
        }
    }
}
