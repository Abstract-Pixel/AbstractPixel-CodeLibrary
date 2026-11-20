// PROTOTYPE for scene system session 3C: the short scene group inspector. Replaced by the real inspector after review.
using AbstractPixel.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Main scene, manager scenes, shared scenes, reset shared scenes and Open group, with Unity's own controls.
    /// Each scene is drawn by the scene field drawer, which is the one place that shows whether a scene is in Build Settings.
    /// </summary>
    [CustomEditor(typeof(SceneGroup))]
    public class SceneGroupEditorPrototype : UnityEditor.Editor
    {
        private const string MainSceneTooltip = "The one scene that makes this place, such as 'Level 7'. It becomes Unity's active scene.";
        private const string ManagerScenesTooltip = "Scenes that open once and stay open across scene changes, such as the scene holding audio and save managers.";
        private const string SharedScenesTooltip = "Scenes several scene groups use next to their main scene, such as the scene holding the player's car.";
        private const string ResetSharedScenesTooltip = "Closes and reopens every shared scene on each scene change, so state kept in them starts fresh.";
        private const string OpenGroupTooltip = "Opens the main scene with its manager scenes and shared scenes, as they are in Play.";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            SerializedProperty mainSceneProperty = serializedObject.FindProperty("MainScene");
            PropertyField mainScene = MakeSceneField(mainSceneProperty, "Main scene", MainSceneTooltip);
            root.Add(mainScene);

            root.Add(MakeSceneList("ManagerialBootScenesList", "Manager scenes", ManagerScenesTooltip));
            root.Add(MakeSceneList("ContextualBootScenesList", "Shared scenes", SharedScenesTooltip));

            SerializedProperty resetProperty = serializedObject.FindProperty("ForceReloadContextualScenes");
            PropertyField resetSharedScenes = new PropertyField(resetProperty, "Reset shared scenes");
            resetSharedScenes.tooltip = ResetSharedScenesTooltip;
            resetSharedScenes.style.marginTop = 6;
            root.Add(resetSharedScenes);

            Button openGroup = new Button(OpenGroup);
            openGroup.text = "Open group";
            openGroup.tooltip = OpenGroupTooltip;
            openGroup.style.marginTop = 10;
            root.Add(openGroup);
            return root;
        }

        // A scene reference wraps one SceneField; binding to it uses the scene field drawer as it is.
        private static PropertyField MakeSceneField(SerializedProperty _sceneReferenceProperty, string _label, string _tooltip)
        {
            SerializedProperty sceneFieldProperty = _sceneReferenceProperty.FindPropertyRelative("scene");
            PropertyField field = new PropertyField(sceneFieldProperty, _label);
            field.tooltip = _tooltip;
            return field;
        }

        private ListView MakeSceneList(string _propertyName, string _title, string _tooltip)
        {
            SerializedProperty listProperty = serializedObject.FindProperty(_propertyName);
            ListView list = new ListView();
            list.headerTitle = _title;
            list.tooltip = _tooltip;
            list.showFoldoutHeader = true;
            list.showAddRemoveFooter = true;
            list.reorderable = true;
            list.reorderMode = ListViewReorderMode.Animated;
            list.showBoundCollectionSize = false;
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            list.style.marginTop = 6;
            list.makeItem = () => new PropertyField();
            list.bindItem = (_element, _index) =>
            {
                PropertyField field = (PropertyField)_element;
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(_index);
                SerializedProperty sceneFieldProperty = elementProperty.FindPropertyRelative("scene");
                field.label = "Scene " + _index;
                field.tooltip = _tooltip;
                field.BindProperty(sceneFieldProperty);
            };
            list.BindProperty(listProperty);
            return list;
        }

        private void OpenGroup()
        {
            SceneGroup group = (SceneGroup)target;
            bool userKeptGoing = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!userKeptGoing)
            {
                return;
            }
            bool isFirstScene = true;
            foreach (AbstractPixel.Core.SceneReference reference in group.ManagerialBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            foreach (AbstractPixel.Core.SceneReference reference in group.ContextualBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            string mainScenePath = group.MainScene.SceneFieldReference.ScenePath;
            OpenScene(mainScenePath, ref isFirstScene);
            UnityEngine.SceneManagement.Scene mainScene = EditorSceneManager.GetSceneByPath(mainScenePath);
            if (mainScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(mainScene);
            }
        }

        private static void OpenScene(string _scenePath, ref bool _isFirstScene)
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                return;
            }
            OpenSceneMode mode = OpenSceneMode.Additive;
            if (_isFirstScene)
            {
                mode = OpenSceneMode.Single;
            }
            EditorSceneManager.OpenScene(_scenePath, mode);
            _isFirstScene = false;
        }
    }
}
