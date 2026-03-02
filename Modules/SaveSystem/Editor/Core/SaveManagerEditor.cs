using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace AbstractPixel.SaveSystem.Editor
{
    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : UnityEditor.Editor
    {
        [SerializeField] StyleSheet saveManagerUSSStyleSheet;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            saveManagerUSSStyleSheet=AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/AbstractPixel/Core/Modules/SaveSystem/Editor/Core/SaveManagerEditorUSS.uss");
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
                    saveManager.SaveALL();
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
                    saveManager.LoadALL();
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

        private void InitializePathsInEditMode()
        {
            SerializedProperty configProperty = serializedObject.FindProperty("saveConfig");
            SaveSystemConfigSO configSO = configProperty.objectReferenceValue as SaveSystemConfigSO;
            if (configSO != null)
            {
                SavePathGenerator.Initialize(configSO);
            }
        }

        private void OpenReleaseDirectory()
        {
            InitializePathsInEditMode();
            string releasePath = SavePathGenerator.ShipRootPath;
            if (!Directory.Exists(releasePath))
            {
                Directory.CreateDirectory(releasePath);
            }
            EditorUtility.RevealInFinder(releasePath);
        }

        private void OpenDebugDirectory()
        {
            InitializePathsInEditMode();
            string debugPath = SavePathGenerator.DebugRootPath;
            if (!Directory.Exists(debugPath))
            {
                Directory.CreateDirectory(debugPath);
            }
            EditorUtility.RevealInFinder(debugPath);

        }

        private void DeleteAllSaves()
        {
            InitializePathsInEditMode();
            string releasePath = SavePathGenerator.ShipRootPath;
            string debugPath = SavePathGenerator.DebugRootPath;
            if (Directory.Exists(releasePath))
            {
                Directory.Delete(releasePath, true);
            }
            if (Directory.Exists(debugPath))
            {
                Directory.Delete(debugPath, true);
            }
            Debug.Log("All save files for both debug and release have been deleted.");
        }
    }
}
