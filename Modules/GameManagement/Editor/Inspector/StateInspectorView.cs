using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The state Inspector's body. The controls stay bound to the asset; everything worked out from it
    /// (and from its parents and the state list) is refreshed whenever any of those assets changes.</summary>
    internal class StateInspectorView : VisualElement
    {
        private const string StyleSheetPath = "Packages/com.abstractpixel.library/Modules/GameManagement/Editor/Inspector/StateInspector.uss";

        private const float NarrowWidth = 340f;

        private readonly SerializedObject data;
        private readonly StateSO state;
        private readonly StateInfoSection info;
        private readonly StateProblemsSection problems;
        private readonly List<StateEffectRow> effects = new List<StateEffectRow>();
        private readonly StateInputActionList inputActions;
        private readonly StateAssetWatcher watcher;
        private bool refreshQueued;

        // The "Is sub-state" box is not saved: a state is a sub-state when it has parents. This keeps the box ticked
        // while the developer has not added the first parent yet.
        private bool showsSubState;

        public StateInspectorView(SerializedObject _data)
        {
            data = _data;
            state = (StateSO)_data.targetObject;
            AddToClassList("si-root");
            // A narrow Inspector gives names the whole width: captions move above them and the switch labels shrink.
            RegisterCallback<GeometryChangedEvent>(_event => EnableInClassList("si-narrow", _event.newRect.width < NarrowWidth));
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null)
            {
                styleSheets.Add(sheet);
            }
            else
            {
                Debug.LogError("[Game management] The state Inspector style sheet is missing at " + StyleSheetPath + ". Reimport the library.");
            }

            VisualElement subLine = new VisualElement();
            subLine.AddToClassList("si-sub-line");
            Add(subLine);

            info = new StateInfoSection(_data, SetSubState);
            Add(info);
            problems = new StateProblemsSection(state, QueueRefresh);
            Add(problems);

            Foldout software = Section("Software Effects", "effects", "What the game does while this state is on top.");
            inputActions = new StateInputActionList(_data);
            AddEffect(software, StateEffectKind.TimeScale, "Time Scale", StateEffectFields.TimeScale(_data));
            AddEffect(software, StateEffectKind.Cursor, "Cursor", StateEffectFields.Cursor(_data));
            AddEffect(software, StateEffectKind.Hud, "HUD", StateEffectFields.Hud(_data));
            AddEffect(software, StateEffectKind.Input, "Input", inputActions);

            Foldout advanced = Section("Advanced", "advanced", "When the state applies and when it leaves.");
            advanced.Add(StateEffectFields.Property(_data, nameof(StateSO.StateExecutionDelay), "Execution delay"));
            advanced.Add(StateEffectFields.Property(_data, nameof(StateSO.DisableStateOnSceneChange), "Leave on scene change"));

            watcher = new StateAssetWatcher(QueueRefresh);
            Add(watcher);
            RegisterCallback<AttachToPanelEvent>(_event =>
            {
                Undo.undoRedoPerformed += QueueRefresh;
                EditorApplication.projectChanged += QueueRefresh;
            });
            RegisterCallback<DetachFromPanelEvent>(_event =>
            {
                Undo.undoRedoPerformed -= QueueRefresh;
                EditorApplication.projectChanged -= QueueRefresh;
                refreshQueued = false;
            });
            Refresh();
        }

        private Foldout Section(string _title, string _key, string _tooltip)
        {
            Foldout section = new Foldout { text = _title, value = false, viewDataKey = "gm-state-" + _key, tooltip = _tooltip };
            section.AddToClassList("si-section");
            Add(section);
            return section;
        }

        private void AddEffect(VisualElement _section, StateEffectKind _kind, string _label, VisualElement _ownValue)
        {
            StateEffectRow row = new StateEffectRow(data, _kind, _label, _ownValue, QueueRefresh);
            effects.Add(row);
            _section.Add(row);
        }

        private void SetSubState(bool _isSubState)
        {
            showsSubState = _isSubState;
            if (!_isSubState)
            {
                data.Update();
                data.FindProperty(nameof(StateSO.Parents)).ClearArray();
                data.ApplyModifiedProperties();
            }
            else
            {
                info.OpenParents();
            }
            Refresh();
        }

        // Delayed and merged: many assets can change in one frame, and a button must not be removed while it handles its click.
        private void QueueRefresh()
        {
            if (refreshQueued || panel == null)
            {
                return;
            }
            refreshQueued = true;
            schedule.Execute(Refresh);
        }

        private void Refresh()
        {
            refreshQueued = false;
            if (state == null)
            {
                return;
            }
            data.Update();
            if (state.Parents.Count > 0)
            {
                showsSubState = true;
            }
            EnableInClassList("si-substate", showsSubState);

            GameManagementSettings settings = GameManagementSettingsAssets.Find();
            GameStateCatalog catalog = settings != null ? settings.Catalog : null;
            info.Refresh(showsSubState);
            problems.Refresh(settings, catalog);
            foreach (StateEffectRow row in effects)
            {
                row.Refresh(showsSubState);
            }
            inputActions.Refresh();
            watcher.Watch(AssetsShown(settings, catalog));
        }

        // What this view reads: the state, every state above its parents, the settings and the state list.
        private IEnumerable<Object> AssetsShown(GameManagementSettings _settings, GameStateCatalog _catalog)
        {
            List<Object> assets = new List<Object> { state };
            for (int i = 0; i < assets.Count; i++)
            {
                foreach (StateSO parent in StateStacks.Parents((StateSO)assets[i]))
                {
                    if (!assets.Contains(parent))
                    {
                        assets.Add(parent);
                    }
                }
            }
            if (_settings != null)
            {
                assets.Add(_settings);
            }
            if (_catalog != null)
            {
                assets.Add(_catalog);
            }
            return assets;
        }
    }
}
