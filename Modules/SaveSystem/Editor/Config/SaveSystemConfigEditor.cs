using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// The save system config's inspector. When the save file list changes, the built-in and module rows are put
    /// back if they were removed. GameSaveFiles.cs is written only when Update save file classes is pressed.
    /// </summary>
    [CustomEditor(typeof(SaveSystemConfigSO))]
    public class SaveSystemConfigEditor : UnityEditor.Editor
    {
        const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/Config/SaveSystemConfigEditor.uss";

        public static StyleSheet LoadStyleSheet()
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
        }

        public override VisualElement CreateInspectorGUI()
        {
            SaveSystemConfigSO config = (SaveSystemConfigSO)target;

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

            root.Add(new HelpBox("The save file list. Each row is one save file. GameProfile, Scene, Settings, Input and Achievements are built in: their classes ship with the library, such as SaveFiles.Settings. Press Update save file classes to write a class for each row you add into GameSaveFiles.cs, so game code writes SaveActions.Save<GameSaveFiles.Ghosts>(). Library modules add their own rows.", HelpBoxMessageType.Info));

            SerializedProperty listProperty = serializedObject.FindProperty("saveFileList");
            // The list is what people come here for, so it starts open.
            listProperty.isExpanded = true;
            PropertyField listField = new PropertyField(listProperty, "Save File List");
            root.Add(listField);

            VisualElement updateArea = BuildUpdateArea(config);
            root.Add(updateArea);
            // TrackSerializedObjectValue works for most changes, but a plain interval is unconditionally
            // correct regardless of what any future list control change does internally, matching the same
            // defensive polling SavedObjectCard already uses for its own similarly externally-driven refresh.
            root.schedule.Execute(() => RefreshUpdateArea(config, updateArea)).Every(200);

            return root;
        }

        VisualElement BuildUpdateArea(SaveSystemConfigSO config)
        {
            VisualElement area = new VisualElement();

            Label waitingLabel = new Label("The save file list has changes that GameSaveFiles.cs doesn't have yet.");
            waitingLabel.AddToClassList("save-file-list__waiting");
            area.Add(waitingLabel);

            Button updateButton = new Button(() => WriteNow(config, area)) { text = "Update save file classes" };
            updateButton.AddToClassList("save-file-list__actions");
            area.Add(updateButton);

            RefreshUpdateArea(config, area);
            return area;
        }

        // The button and its line disappear together once GameSaveFiles.cs matches the list.
        void RefreshUpdateArea(SaveSystemConfigSO config, VisualElement area)
        {
            // A row just added or removed through the list's own buttons is still only on the SerializedObject
            // side until this runs, so config.SaveFileList (the actual object) would otherwise read stale data.
            serializedObject.ApplyModifiedProperties();
            bool isOutOfDate = SaveFileListStatus.IsOutOfDate(config.SaveFileList, config.GameSaveFilesScriptPath);
            area.style.display = isOutOfDate ? DisplayStyle.Flex : DisplayStyle.None;
            // Disabled, not hidden, while a clash blocks the write: the row above already says why.
            area.Q<Button>().SetEnabled(SaveFileClash.FindAll(config.SaveFileList).Count == 0);
        }

        void WriteNow(SaveSystemConfigSO config, VisualElement area)
        {
            SaveFilesCodeWriter.WriteFile(config);
            serializedObject.Update();
            RefreshUpdateArea(config, area);
        }
    }
}
