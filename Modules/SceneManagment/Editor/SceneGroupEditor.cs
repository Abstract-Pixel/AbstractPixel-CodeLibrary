using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Main scene, manager scenes, shared scenes, reset shared scenes and Open group, with Unity's own controls.
    /// Each scene is drawn by the scene field drawer, which shows the scene's status. A problems list at the top
    /// appears only when the group check finds something wrong.
    /// </summary>
    [CustomEditor(typeof(SceneGroup))]
    public class SceneGroupEditor : UnityEditor.Editor
    {
        private Button addToBuildButton;
        private SceneGroupProblemsView problemsView;
        private const string MainSceneTooltip = "The one scene that makes this place, such as 'Level 7'. It becomes Unity's active scene.";
        private const string ManagerScenesTooltip = "Scenes that open once and stay open across scene changes, such as the scene holding audio and save managers.";
        private const string SharedScenesTooltip = "Scenes several scene groups use next to their main scene, such as the scene holding the player's car.";
        private const string ResetSharedScenesTooltip = "Closes and reopens every shared scene on each scene change, so state kept in them starts fresh.";
        private const string AddToBuildTooltip = "Adds this group's manager scenes, shared scenes and main scene to Build Settings. Scenes already there are not added again; a scene that is switched off is switched on.";
        private const string OpenGroupTooltip = "Opens the main scene with its manager scenes and shared scenes, as they are in Play.";
        private const string CheckAllGroupsTooltip = "Opens a window listing every scene group in the project with its problems and their fixes.";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            problemsView = new SceneGroupProblemsView(true);
            problemsView.style.marginBottom = 6;
            root.Add(problemsView);

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

            addToBuildButton = new Button(AddScenesToBuildSettings);
            addToBuildButton.text = "Add scenes to Build Settings";
            addToBuildButton.tooltip = AddToBuildTooltip;
            addToBuildButton.style.marginTop = 10;
            root.Add(addToBuildButton);
            RefreshGroupState();
            root.TrackSerializedObjectValue(serializedObject, _changedObject => RefreshGroupState());
            root.RegisterCallback<AttachToPanelEvent>(_event => SubscribeToProjectChanges());
            root.RegisterCallback<DetachFromPanelEvent>(_event => UnsubscribeFromProjectChanges());

            Button openGroup = new Button(OpenGroup);
            openGroup.text = "Open group";
            openGroup.tooltip = OpenGroupTooltip;
            openGroup.style.marginTop = 4;
            root.Add(openGroup);

            Button checkAllGroups = new Button(CheckAllSceneGroupsWindow.Open);
            checkAllGroups.text = "Check all groups";
            checkAllGroups.tooltip = CheckAllGroupsTooltip;
            checkAllGroups.style.marginTop = 4;
            root.Add(checkAllGroups);
            return root;
        }

        private void SubscribeToProjectChanges()
        {
            EditorBuildSettings.sceneListChanged += RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed += RefreshGroupState;
            }
        }

        private void UnsubscribeFromProjectChanges()
        {
            EditorBuildSettings.sceneListChanged -= RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed -= RefreshGroupState;
            }
        }

        // The problems list and the Add scenes button both follow the group and the project.
        private void RefreshGroupState()
        {
            RefreshAddToBuildButton();
            RefreshProblems();
        }

        private void RefreshProblems()
        {
            if (problemsView == null || target == null)
            {
                return;
            }
            SceneGroup group = (SceneGroup)target;
            problemsView.Refresh(group, AfterProblemFix);
        }

        private void AfterProblemFix()
        {
            serializedObject.Update();
            RefreshGroupState();
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

        private void AddScenesToBuildSettings()
        {
            SceneGroup group = (SceneGroup)target;
            SceneGroupBuildSettings.ApplyToProject(group);
            RefreshGroupState();
        }

        // The button is clickable only when Build Settings would change; the scene field shows which scenes are missing.
        private void RefreshAddToBuildButton()
        {
            if (addToBuildButton == null || target == null)
            {
                return;
            }
            SceneGroup group = (SceneGroup)target;
            BuildSettingsChange change = SceneGroupBuildSettings.PreviewForProject(group);
            addToBuildButton.SetEnabled(change.HasChanges);
        }

        private void OpenGroup()
        {
            SceneGroup group = (SceneGroup)target;
            SceneGroupOpener.Open(group);
        }
    }
}
