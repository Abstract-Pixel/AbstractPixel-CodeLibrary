using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The short scene group inspector: problems at the top (only when the group check finds some), the main scene, the
    /// scene set, then an Advanced foldout with manager scenes, shared scenes and Reset shared scenes. Each of those says
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
        private readonly Dictionary<SceneSetPart, VisualElement> partHeaders = new Dictionary<SceneSetPart, VisualElement>();

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SceneGroup group = (SceneGroup)target;

            problemsView = new SceneGroupProblemsView(true);
            problemsView.style.marginBottom = 6;
            root.Add(problemsView);
            bootGroupBox = new HelpBox("This scene group has no main scene and no scene set: a boot group from before scene sets. It is not a place the player can go, and scene sets now hold its scenes.", HelpBoxMessageType.Info);
            root.Add(bootGroupBox);

            root.Add(SceneSystemUI.SceneField(serializedObject.FindProperty("MainScene"), "Main scene", SceneSystemUI.MainSceneTooltip));
            root.Add(MakeSceneSetField(group));
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
            container.Add(field);
            return container;
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
                bool changed = SceneSetSync.UpdateOverrideAfterEdit(group, _part);
                if (changed)
                {
                    EditorUtility.SetDirty(group);
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

        private void SubscribeToProjectChanges()
        {
            EditorBuildSettings.sceneListChanged += RefreshGroupState;
            SceneSystemSettingsAssets.Changed += RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed += RefreshGroupState;
            }
        }

        private void UnsubscribeFromProjectChanges()
        {
            EditorBuildSettings.sceneListChanged -= RefreshGroupState;
            SceneSystemSettingsAssets.Changed -= RefreshGroupState;
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider != null)
            {
                provider.Changed -= RefreshGroupState;
            }
        }

        // The problems list, the boot group note, the part headers and the Add scenes button follow the group and the project.
        private void RefreshGroupState()
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
