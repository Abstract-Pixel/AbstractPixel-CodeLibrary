using System;
using System.Collections.Generic;
using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The Problems block. It shows what the validator finds for this state and hides when there is nothing.</summary>
    internal class StateProblemsSection : Foldout
    {
        private readonly StateSO state;
        private readonly Action changed;

        public StateProblemsSection(StateSO _state, Action _changed)
        {
            state = _state;
            changed = _changed;
            value = false;
            viewDataKey = "gm-state-problems";
            AddToClassList("si-section");
        }

        public void Refresh(GameManagementSettings _settings, GameStateCatalog _catalog)
        {
            Clear();
            if (_catalog == null)
            {
                style.display = DisplayStyle.Flex;
                text = "Problems (1)";
                string message = _settings == null
                    ? "No game management settings, so this state is not checked and states are off."
                    : "The settings have no state list, so this state is not checked and every request for it is refused.";
                Add(EditorUI.HelpBoxWithButton(message, HelpBoxMessageType.Warning, "Set up", "Opens the Game Management settings page",
                    () => SettingsService.OpenProjectSettings(GameManagementSettingsProvider.PagePath)));
                return;
            }

            List<StateProblem> found = StateListValidator.FindForState(_catalog, state);
            style.display = found.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            text = "Problems (" + found.Count + ")";
            foreach (StateProblem problem in found)
            {
                HelpBox box = StateProblemBox.Create(problem, () =>
                {
                    StateListValidator.Fix(_catalog, problem);
                    changed();
                });
                Add(box);
            }
        }
    }
}
