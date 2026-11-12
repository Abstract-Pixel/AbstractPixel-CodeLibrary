using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : UnityEditor.Editor
    {
        StyleSheet saveManagerUSSStyleSheet;
        [SerializeField] string uSSStyleSheetPath = "Packages/com.abstractpixel.library/Modules/SaveSystem/Editor/Core/SaveManagerEditorUSS.uss";
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            saveManagerUSSStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(uSSStyleSheetPath);
            if (saveManagerUSSStyleSheet != null)
            {
                root.styleSheets.Add(saveManagerUSSStyleSheet);
            }

            VisualElement debugContainer = new VisualElement();
            debugContainer.AddToClassList("debug-container");

            Foldout debugFoldOut = new Foldout { text = "Debug Controls" };
            debugFoldOut.AddToClassList("debug-foldout");
            debugContainer.Add(debugFoldOut);

            VisualElement emptySpace = new VisualElement();
            emptySpace.style.height = 15;
            debugFoldOut.Add(emptySpace);

            Label editorControlsLabel = new Label("Editor Controls");
            editorControlsLabel.AddToClassList("control-label");

            VisualElement editorControlsContainer = new VisualElement();
            editorControlsContainer.AddToClassList("editor-controls-container");
            Button deleteAllSavesButton = new Button();
            deleteAllSavesButton.AddToClassList("delete-saves-button");
            deleteAllSavesButton.text = "Delete All Saves";
            Button openReleaseSaveDirectory = new Button();
            openReleaseSaveDirectory.AddToClassList("editor-control-button");
            openReleaseSaveDirectory.text = "Open Release Save Directory";
            Button openDebugSaveDirectory = new Button();
            openDebugSaveDirectory.AddToClassList("editor-control-button");
            openDebugSaveDirectory.text = "Open Debug Save Directory";

            deleteAllSavesButton.clicked += DeleteAllSaves;
            openReleaseSaveDirectory.clicked += OpenReleaseDirectory;
            openDebugSaveDirectory.clicked += OpenDebugDirectory;

            debugFoldOut.Add(editorControlsLabel);
            editorControlsContainer.Add(deleteAllSavesButton);
            editorControlsContainer.Add(openReleaseSaveDirectory);
            editorControlsContainer.Add(openDebugSaveDirectory);
            debugFoldOut.Add(editorControlsContainer);


            Label runtimeControlsLabel = new Label("Runtime Controls");
            runtimeControlsLabel.AddToClassList("control-label");
            VisualElement runtimeControlsContainer = new VisualElement();
            runtimeControlsContainer.AddToClassList("runtime-controls-container");
            Button saveRuntimeDataButton = new Button();
            saveRuntimeDataButton.AddToClassList("runtime-control-button");
            saveRuntimeDataButton.text = "SAVE ALL DATA !";
            Button loadRuntimeDataButton = new Button();
            loadRuntimeDataButton.AddToClassList("runtime-control-button");
            loadRuntimeDataButton.text = "LOAD ALL DATA !";



            if (!EditorApplication.isPlaying)
            {
                runtimeControlsLabel.SetEnabled(false);
                saveRuntimeDataButton.SetEnabled(false);
                loadRuntimeDataButton.SetEnabled(false);
            }

            saveRuntimeDataButton.clicked += () =>
        {
            if (Application.isPlaying)
            {
                SaveManager saveManager = (SaveManager)target;
                SaveActions.SaveAll();
                Debug.Log("[Save System: FORCED] : Save all data executed.");
            }
            else
            {
                Debug.LogWarning("Save and Load buttons only work in Play mode.");
            }
        };

            loadRuntimeDataButton.clicked += () =>
            {
                if (Application.isPlaying)
                {
                    SaveManager saveManager = (SaveManager)target;
                    SaveActions.LoadAll();
                    Debug.Log("[Save System: FORCED] : Load all data executed.");
                }
                else
                {
                    Debug.LogWarning("Save and Load buttons only work in Play mode.");
                }
            };

            debugFoldOut.Add(runtimeControlsLabel);
            runtimeControlsContainer.Add(saveRuntimeDataButton);
            runtimeControlsContainer.Add(loadRuntimeDataButton);
            debugFoldOut.Add(runtimeControlsContainer);

            root.Add(debugContainer);
            return root;
        }

        // The folder logic lives in SaveSystemToolsMenu, so the menu and this inspector always agree.
        private void OpenReleaseDirectory() => SaveSystemToolsMenu.OpenReleaseFolder();

        private void OpenDebugDirectory() => SaveSystemToolsMenu.OpenDebugFolder();

        private void DeleteAllSaves() => SaveSystemToolsMenu.DeleteAllSaveFiles();
    }
}
