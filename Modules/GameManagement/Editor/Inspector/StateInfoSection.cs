using System;
using System.Collections.Generic;
using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Everything above Problems: the type tag, "Opens over", the sub-state box and the two parent panels.</summary>
    internal class StateInfoSection : VisualElement
    {
        private readonly StateSO state;
        private readonly Label typeTag = new Label();
        private readonly VisualElement opensOver = new VisualElement();
        private readonly Toggle isSubState = new Toggle("Is sub-state");
        private readonly Label notSavedHint = new Label("NOTE: Add a parent. Without one this stays a main state.");
        private readonly VisualElement parentPanels = new VisualElement();
        private readonly StateParentList parentList;
        private readonly StateParentTable table = new StateParentTable();
        private readonly Label tableNote = new Label("<b>NOTE:</b> The values the game uses while this state is open over each parent. Teal values are changed by this state.");
        private readonly HelpBox noParents = new HelpBox(
            "No parents have been added, so there is no real difference between this sub-state and a parent.", HelpBoxMessageType.Warning);

        public StateInfoSection(SerializedObject _data, Action<bool> _subStateTicked)
        {
            state = (StateSO)_data.targetObject;
            AddToClassList("si-info");

            VisualElement titleRow = new VisualElement();
            titleRow.AddToClassList("si-title-row");
            typeTag.AddToClassList("si-tag");
            titleRow.Add(typeTag);
            Button open = EditorUI.Button("Open in Project Settings", "Opens the state list, where this state's rank is set",
                () => SettingsService.OpenProjectSettings(GameManagementSettingsProvider.PagePath));
            open.AddToClassList("si-open-button");
            titleRow.Add(open);
            Add(titleRow);

            opensOver.AddToClassList("si-opens-over");
            Add(opensOver);

            isSubState.tooltip = "On: this state opens over its parents. Off: it is a main state. Turning it off removes the parents (Undo brings them back).";
            isSubState.AddToClassList("si-field");
            isSubState.RegisterValueChangedCallback(_change => _subStateTicked(_change.newValue));
            Add(isSubState);
            notSavedHint.AddToClassList("si-inherited");
            Add(notSavedHint);

            parentList = new StateParentList(_data);
            parentPanels.Add(parentList);

            Foldout over = new Foldout { text = "What this state does over each parent", value = false, viewDataKey = "gm-state-over-parents" };
            over.AddToClassList("si-panel-foldout");
            noParents.AddToClassList("si-no-parents");
            over.Add(noParents);
            tableNote.AddToClassList("si-table-hint");
            over.Add(tableNote);
            over.Add(table);
            over.Add(table.Legend);
            VisualElement overPanel = new VisualElement();
            overPanel.AddToClassList("si-panel");
            overPanel.Add(over);
            parentPanels.Add(overPanel);
            Add(parentPanels);
        }

        public void OpenParents()
        {
            parentList.Open();
        }

        public void Refresh(bool _showsSubState)
        {
            typeTag.text = _showsSubState ? "Sub-state" : "Main state";
            isSubState.SetValueWithoutNotify(_showsSubState);
            Show(parentPanels, _showsSubState);
            Show(notSavedHint, _showsSubState && state.Parents.Count == 0);
            RefreshOpensOver(_showsSubState);
            parentList.Refresh();
            table.Rebuild(state);
            bool hasParent = StateStacks.Parents(state).Count > 0;
            Show(noParents, !hasParent);
            Show(tableNote, hasParent);
            Show(table, hasParent);
        }

        private void RefreshOpensOver(bool _showsSubState)
        {
            opensOver.Clear();
            opensOver.Add(Text("Opens over", "si-opens-caption", null));
            List<StateSO> parents = StateStacks.Parents(state);
            if (!_showsSubState || parents.Count == 0)
            {
                opensOver.Add(Text(_showsSubState ? "no parent yet" : "nothing", "si-opens-none", null));
                return;
            }
            foreach (StateSO parent in parents)
            {
                Label chip = Text(parent.name, "si-opens-chip", parent.name);
                chip.RegisterCallback<ClickEvent>(_event => EditorGUIUtility.PingObject(parent));
                opensOver.Add(chip);
            }
        }

        private static Label Text(string _text, string _class, string _tooltip)
        {
            Label label = new Label(_text) { tooltip = _tooltip, enableRichText = false };
            label.AddToClassList(_class);
            return label;
        }

        private static void Show(VisualElement _element, bool _shown)
        {
            _element.style.display = _shown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
