using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The short scene group inspector: problems at the top (only when the group check finds some), the main scene, the
    /// scene set, Show the loading scene with its loading scene picker, then an Advanced foldout with manager scenes, shared scenes and Reset shared scenes. Each of those says
    /// whether it follows the set or is an override, with Revert to set. Editing a part makes it an override; editing it
    /// back to the set's value ends the override. Every scene is drawn by the scene field drawer, which shows its status.
    /// </summary>
    [CustomEditor(typeof(SceneGroup))]
    public class SceneGroupEditor : UnityEditor.Editor
    {
        private const string AdvancedOpenKey = "AbstractPixel.SceneSystem.GroupInspectorAdvancedOpen";
        private const string AddToBuildTooltip = "Adds this group's manager scenes, shared scenes and main scene to Build Settings. Scenes already there are not added again; a scene that is switched off is switched on.";
        private const string OpenSceneSystemTooltip = "Opens Project Settings > Abstract Pixel > Scene System at the Scene Groups Debugger, which lists every scene group with its problems and their fixes.";
        private const string MoveToAddressablesTooltip = "Gives each of this group's scenes an Addressables entry and removes it from Build Settings, so it loads from one place.";

        private static readonly SceneSetPart[] Parts = { SceneSetPart.ManagerScenes, SceneSetPart.SharedScenes, SceneSetPart.ResetSharedScenes };

        private Button addToBuildButton;
        private SceneGroupProblemsView problemsView;
        private HelpBox bootGroupBox;
        private PopupField<SceneSet> sceneSetPopup;
        private PopupField<string> showLoadingScenePopup;
        private PopupField<string> loadingScenePopup;
        private readonly Dictionary<SceneSetPart, VisualElement> partHeaders = new Dictionary<SceneSetPart, VisualElement>();

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SceneGroup group = (SceneGroup)target;

            root.Add(SceneSystemHelp.HelpRow("One place the player can be: a main scene, its scene set and when it shows a loading scene.", SceneSystemDocTopics.SceneGroupInspector));
            problemsView = new SceneGroupProblemsView(true);
            problemsView.style.marginBottom = 6;
            root.Add(problemsView);
            bootGroupBox = new HelpBox("This scene group has no main scene and no scene set: a boot group from before scene sets. It is not a place the player can go, and scene sets now hold its scenes.", HelpBoxMessageType.Info);
            root.Add(bootGroupBox);

            root.Add(SceneSystemUI.SceneField(serializedObject.FindProperty("MainScene"), "Main scene", SceneSystemUI.MainSceneTooltip));
            root.Add(MakeSceneSetField(group));
            root.Add(MakeLoadingSceneFields(group));
            root.Add(MakeSampleToggle());
            root.Add(MakeAdvanced());
            root.Add(MakeButtons(group));

            RefreshGroupState();
            root.TrackSerializedObjectValue(serializedObject, _changedObject => RefreshGroupState());
            root.TrackPropertyValue(serializedObject.FindProperty("ManagerialBootScenesList"), _property => OnPartEdited(SceneSetPart.ManagerScenes));
            root.TrackPropertyValue(serializedObject.FindProperty("ContextualBootScenesList"), _property => OnPartEdited(SceneSetPart.SharedScenes));
            root.TrackPropertyValue(serializedObject.FindProperty("ForceReloadContextualScenes"), _property => OnPartEdited(SceneSetPart.ResetSharedScenes));
            root.RegisterCallback<AttachToPanelEvent>(_event => SubscribeToProjectChanges());
            root.RegisterCallback<DetachFromPanelEvent>(_event => UnsubscribeFromProjectChanges());
            return root;
        }

        private VisualElement MakeSceneSetField(SceneGroup _group)
        {
            VisualElement container = new VisualElement();
            container.style.marginTop = 4;
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings == null)
            {
                container.Add(SceneSystemUI.HelpBoxWithButton("Scene sets need the scene system to be set up.", HelpBoxMessageType.Info, "Open Scene System", "Opens Project Settings > Abstract Pixel > Scene System, where the setup is.", SceneSystemSettingsProvider.Open));
                return container;
            }

            PopupField<SceneSet> field = SceneSystemUI.SceneSetPopup("Scene set", settings, _group.SceneSet, OnSetPicked);
            field.AddToClassList(PopupField<SceneSet>.alignedFieldUssClassName);
            sceneSetPopup = field;
            container.Add(field);
            return container;
        }

        private static readonly List<string> ShowLoadingSceneChoices = new List<string> { "Never", "When arriving from another scene group", "Every time, Retry included" };
        private const string ShowLoadingSceneTooltip = "Show the loading scene: Never; When arriving from another scene group (a Retry goes straight back in without it); or Every time, Retry included.";
        private const string LoadingScenePickTooltip = "Which loading scene this group shows. The default comes from Project Settings > Abstract Pixel > Scene System > Loading scenes.";
        private const string DefaultLoadingSceneChoice = "Default";
        private VisualElement loadingScenePickRow;

        // "Show the loading scene" and, when it is not Never, the loading scene picker with the default preselected.
        private VisualElement MakeLoadingSceneFields(SceneGroup _group)
        {
            VisualElement container = new VisualElement();
            container.style.marginTop = 4;
            int currentChoice = (int)_group.ShowLoadingScene;
            PopupField<string> showField = new PopupField<string>("Show the loading scene", ShowLoadingSceneChoices, currentChoice);
            showField.tooltip = ShowLoadingSceneTooltip;
            showField.AddToClassList(PopupField<string>.alignedFieldUssClassName);
            showField.RegisterValueChangedCallback(_change => OnShowLoadingScenePicked(ShowLoadingSceneChoices.IndexOf(_change.newValue)));
            container.Add(showField);
            showLoadingScenePopup = showField;

            loadingScenePickRow = new VisualElement();
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings != null)
            {
                PopupField<string> pick = MakeLoadingScenePicker(_group, settings);
                pick.AddToClassList(PopupField<string>.alignedFieldUssClassName);
                loadingScenePickRow.Add(pick);
                loadingScenePopup = pick;
            }
            loadingScenePickRow.style.display = _group.ShowLoadingScene == ShowLoadingScene.Never ? DisplayStyle.None : DisplayStyle.Flex;
            container.Add(loadingScenePickRow);
            return container;
        }

        private PopupField<string> MakeLoadingScenePicker(SceneGroup _group, SceneSystemSettings _settings)
        {
            string defaultName = _settings.DefaultLoadingScene == null ? string.Empty : _settings.DefaultLoadingScene.SceneName;
            string defaultLabel = string.IsNullOrEmpty(defaultName) ? DefaultLoadingSceneChoice + " (none set)" : DefaultLoadingSceneChoice + " (" + defaultName + ")";
            List<string> choices = new List<string> { defaultLabel };
            foreach (SceneReference scene in _settings.LoadingScenes)
            {
                if (scene != null && !string.IsNullOrEmpty(scene.SceneName) && !choices.Contains(scene.SceneName))
                {
                    choices.Add(scene.SceneName);
                }
            }

            PopupField<string> pick = new PopupField<string>("Loading scene", choices, LoadingSceneChoiceOf(_group, choices));
            pick.tooltip = LoadingScenePickTooltip;
            pick.RegisterValueChangedCallback(_change => OnLoadingScenePicked(_settings, _change.newValue));
            return pick;
        }

        // The group's own pick when it is one of the choices, otherwise the first choice, "Default".
        private static string LoadingSceneChoiceOf(SceneGroup _group, List<string> _choices)
        {
            bool hasOwnPick = _group.LoadingScene != null && !string.IsNullOrEmpty(_group.LoadingScene.SceneName);
            if (hasOwnPick && _choices.Contains(_group.LoadingScene.SceneName))
            {
                return _group.LoadingScene.SceneName;
            }
            return _choices[0];
        }

        // The popups are not bound to the asset, so Undo, Redo and edits made elsewhere are copied into them here.
        private void ShowValuesInPopups(SceneGroup _group)
        {
            sceneSetPopup?.SetValueWithoutNotify(_group.SceneSet);
            showLoadingScenePopup?.SetValueWithoutNotify(ShowLoadingSceneChoices[(int)_group.ShowLoadingScene]);
            loadingScenePopup?.SetValueWithoutNotify(LoadingSceneChoiceOf(_group, loadingScenePopup.choices));
            if (loadingScenePickRow != null)
            {
                loadingScenePickRow.style.display = _group.ShowLoadingScene == ShowLoadingScene.Never ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void OnShowLoadingScenePicked(int _choiceIndex)
        {
            ShowLoadingScene choice = (ShowLoadingScene)Mathf.Max(0, _choiceIndex);
            foreach (Object item in targets)
            {
                SceneGroup group = (SceneGroup)item;
                Undo.RecordObject(group, "Show the loading scene");
                group.ShowLoadingScene = choice;
                SaveGroupNow(group);
            }
            serializedObject.Update();
            if (loadingScenePickRow != null)
            {
                loadingScenePickRow.style.display = choice == ShowLoadingScene.Never ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void OnLoadingScenePicked(SceneSystemSettings _settings, string _choice)
        {
            SceneReference picked = new SceneReference(string.Empty);
            foreach (SceneReference scene in _settings.LoadingScenes)
            {
                if (scene != null && scene.SceneName == _choice)
                {
                    picked = new SceneReference(scene.SceneName, scene.SceneGuid, scene.ScenePath);
                }
            }
            foreach (Object item in targets)
            {
                SceneGroup group = (SceneGroup)item;
                Undo.RecordObject(group, "Loading scene");
                group.LoadingScene = picked;
                SaveGroupNow(group);
            }
            serializedObject.Update();
        }

        // Saved at once, like every group inspector edit, so a crash cannot lose it.
        private static void SaveGroupNow(SceneGroup _group)
        {
            EditorUtility.SetDirty(_group);
            AssetDatabase.SaveAssetIfDirty(_group);
        }

        private void OnSetPicked(SceneSet _set)
        {
            foreach (Object item in targets)
            {
                SceneSetSync.PickSet((SceneGroup)item, _set);
            }
            serializedObject.Update();
            RefreshGroupState();
        }

        // Sample scene group (ADR 0014): its scenes open in the Editor but never go into a build.
        private VisualElement MakeSampleToggle()
        {
            PropertyField sample = new PropertyField(serializedObject.FindProperty("isSample"), "Sample scene group");
            sample.tooltip = "Sample scene group: its scenes open in the Editor but never go into a build. A build that still contains one stops.";
            sample.style.marginTop = 4;
            sample.RegisterValueChangeCallback(_change =>
            {
                SampleSceneGroups.Forget();
                SceneGroup group = (SceneGroup)target;
                if (group != null && group.IsSample)
                {
                    // Ticked: its scenes leave Build Settings at once (the writer leaves sample scenes out).
                    AbstractPixel.Core.Editor.SceneBuildSettingsWriter.Write(EditorBuildSettings.scenes);
                }
            });

            VisualElement container = new VisualElement();
            container.Add(sample);
            VisualElement launchScene = MakeSampleLaunchSceneField();
            container.Add(launchScene);
            ShowSampleLaunchSceneWhenSample(launchScene);
            sample.RegisterValueChangeCallback(_change => ShowSampleLaunchSceneWhenSample(launchScene));
            return container;
        }

        // The sample launch scene (scene system S): shown only while "Sample scene group" is ticked.
        private VisualElement MakeSampleLaunchSceneField()
        {
            PropertyField launchScene = new PropertyField(serializedObject.FindProperty("sampleLaunchScene"), "Launch scene (sample only)");
            launchScene.name = "sample-launch-scene";
            launchScene.tooltip = "Editor only. Play started from this scene runs the launch scene hold with this sample scene group as the start scene group.";
            launchScene.style.marginLeft = 15;
            return launchScene;
        }

        private void ShowSampleLaunchSceneWhenSample(VisualElement _launchScene)
        {
            SceneGroup group = (SceneGroup)target;
            bool isSample = group != null && group.IsSample;
            DisplayStyle display = DisplayStyle.None;
            if (isSample)
            {
                display = DisplayStyle.Flex;
            }
            _launchScene.style.display = display;
        }

        private VisualElement MakeAdvanced()
        {
            Foldout advanced = new Foldout();
            advanced.text = "Advanced";
            advanced.value = SessionState.GetBool(AdvancedOpenKey, false);
            advanced.tooltip = "Manager scenes, shared scenes and Reset shared scenes. The scene set fills them; a part changed here becomes an override.";
            advanced.style.marginTop = 6;
            advanced.RegisterValueChangedCallback(_change =>
            {
                if (_change.target == advanced)
                {
                    SessionState.SetBool(AdvancedOpenKey, _change.newValue);
                }
            });

            advanced.Add(MakePartHeader(SceneSetPart.ManagerScenes, "Manager scenes", SceneSystemUI.ManagerScenesTooltip));
            advanced.Add(SceneSystemUI.SceneList(serializedObject, "ManagerialBootScenesList", string.Empty, SceneSystemUI.ManagerScenesTooltip));
            advanced.Add(MakePartHeader(SceneSetPart.SharedScenes, "Shared scenes", SceneSystemUI.SharedScenesTooltip));
            advanced.Add(SceneSystemUI.SceneList(serializedObject, "ContextualBootScenesList", string.Empty, SceneSystemUI.SharedScenesTooltip));
            // The toggle sits in its own header row, so the words "Reset shared scenes" appear once.
            VisualElement resetHeader = MakePartHeader(SceneSetPart.ResetSharedScenes, "Reset shared scenes", SceneSystemUI.ResetSharedScenesTooltip);
            Toggle reset = new Toggle();
            reset.name = "reset-shared-scenes";
            reset.tooltip = SceneSystemUI.ResetSharedScenesTooltip;
            reset.style.marginLeft = 8;
            reset.BindProperty(serializedObject.FindProperty("ForceReloadContextualScenes"));
            resetHeader.Insert(1, reset);
            advanced.Add(resetHeader);
            return advanced;
        }

        // "Manager scenes   [From set 'Level']" or "Manager scenes   [Override] [Revert to set]".
        private VisualElement MakePartHeader(SceneSetPart _part, string _title, string _tooltip)
        {
            VisualElement header = SceneSystemUI.Row();
            header.style.marginTop = 8;
            header.style.marginBottom = 2;
            Label title = new Label(_title);
            title.tooltip = _tooltip;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            SceneSystemUI.AddHoverTint(title);
            header.Add(title);
            header.Add(SceneSystemUI.Spacer());
            VisualElement state = SceneSystemUI.Row();
            header.Add(state);
            partHeaders[_part] = state;
            return header;
        }

        private VisualElement MakeButtons(SceneGroup _group)
        {
            VisualElement buttons = new VisualElement();
            buttons.style.marginTop = 10;

            addToBuildButton = SceneSystemUI.Button("Add scenes to Build Settings", AddToBuildTooltip, AddScenesToBuildSettings);
            buttons.Add(addToBuildButton);

            bool addressablesInstalled = AddressableScenes.IsInstalled;
            string moveTooltip = addressablesInstalled ? MoveToAddressablesTooltip : SceneMoves.NeedsAddressablesTooltip;
            Button moveToAddressables = SceneSystemUI.Button("Move this group's scenes to Addressables", moveTooltip, () =>
            {
                SceneMoves.MoveGroupToAddressables((SceneGroup)target);
                RefreshGroupState();
            });
            moveToAddressables.SetEnabled(addressablesInstalled);
            buttons.Add(moveToAddressables);

            buttons.Add(SceneSystemUI.Button("Open group", SceneSystemUI.OpenGroupTooltip, () => SceneGroupOpener.Open((SceneGroup)target)));

            // Its own row under the three group buttons.
            VisualElement pageRow = new VisualElement();
            pageRow.style.marginTop = 6;
            pageRow.Add(SceneSystemUI.PrimaryButton("Open Scene System", OpenSceneSystemTooltip, SceneSystemSettingsProvider.OpenAtDebugger));
            buttons.Add(pageRow);
            return buttons;
        }

        // A tracked change is a real change of the value (binding alone raises none), so it is safe to treat as an edit.
        private void OnPartEdited(SceneSetPart _part)
        {
            bool anyChanged = false;
            foreach (Object item in targets)
            {
                SceneGroup group = (SceneGroup)item;
                bool changed = SceneSetSync.SaveInspectorEdit(group, _part);
                if (changed)
                {
                    anyChanged = true;
                }
            }
            if (anyChanged)
            {
                serializedObject.Update();
            }
            RefreshPartHeaders();
        }

        private void RefreshPartHeaders()
        {
            if (target == null)
            {
                return;
            }
            SceneGroup group = (SceneGroup)target;
            foreach (SceneSetPart part in Parts)
            {
                if (!partHeaders.TryGetValue(part, out VisualElement state))
                {
                    continue;
                }
                state.Clear();
                if (group.SceneSet == null)
                {
                    state.Add(SceneSystemUI.Badge("Own value", false, "This group has no scene set, so every part is its own."));
                    continue;
                }
                if (!group.IsOverride(part))
                {
                    state.Add(SceneSystemUI.Badge("From set '" + group.SceneSet.name + "'", false, SceneSystemUI.FromSetTooltip));
                    continue;
                }
                state.Add(SceneSystemUI.Badge("Override", true, SceneSystemUI.OverrideTooltip));
                SceneSetPart revertedPart = part;
                Button revert = SceneSystemUI.Button("Revert to set", SceneSystemUI.RevertTooltip, () => RevertToSet(revertedPart));
                revert.style.marginLeft = 4;
                state.Add(revert);
            }
        }

        private void RevertToSet(SceneSetPart _part)
        {
            foreach (Object item in targets)
            {
                SceneSetSync.RevertToSetInProject((SceneGroup)item, _part);
            }
            serializedObject.Update();
            RefreshGroupState();
        }

        internal void SubscribeToProjectChanges()
        {
            EditorBuildSettings.sceneListChanged += RefreshGroupState;
            // A deleted or moved scene changes no scene list, only the project.
            EditorApplication.projectChanged += RefreshGroupState;
            SceneSystemSettingsAssets.Changed += RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed += RefreshGroupState;
            }
        }

        internal void UnsubscribeFromProjectChanges()
        {
            EditorBuildSettings.sceneListChanged -= RefreshGroupState;
            EditorApplication.projectChanged -= RefreshGroupState;
            SceneSystemSettingsAssets.Changed -= RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed -= RefreshGroupState;
            }
        }

        // The problems list, the boot group note, the part headers and the Add scenes button follow the group and the project.
        internal void RefreshGroupState()
        {
            if (target == null)
            {
                return;
            }
            SceneGroup group = (SceneGroup)target;
            if (addToBuildButton != null)
            {
                BuildSettingsChange change = SceneGroupBuildSettings.PreviewForProject(group);
                addToBuildButton.SetEnabled(change.HasChanges);
            }
            if (problemsView != null)
            {
                problemsView.Refresh(group, AfterProblemFix);
            }
            if (bootGroupBox != null)
            {
                bootGroupBox.style.display = SceneGroupCheck.IsBootGroup(group) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            ShowValuesInPopups(group);
            RefreshPartHeaders();
        }

        private void AfterProblemFix()
        {
            serializedObject.Update();
            RefreshGroupState();
        }

        private void AddScenesToBuildSettings()
        {
            SceneGroup group = (SceneGroup)target;
            SceneGroupBuildSettings.ApplyToProject(group);
            RefreshGroupState();
        }
    }
}
