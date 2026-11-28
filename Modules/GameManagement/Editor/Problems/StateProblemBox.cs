using System;
using AbstractPixel.Core.Editor.UI;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>One problem's help box with its fix button and "Learn more". The settings page and the Inspector share it.</summary>
    internal static class StateProblemBox
    {
        public static HelpBox Create(StateProblem _problem, Action _fix)
        {
            HelpBox box = new HelpBox(_problem.Message, _problem.IsError ? HelpBoxMessageType.Error : HelpBoxMessageType.Warning);
            box.AddToClassList("gm-problem");
            VisualElement actions = new VisualElement();
            actions.AddToClassList("gm-help-actions");
            Button fix = EditorUI.Button(_problem.FixLabel, _problem.FixTooltip, _fix);
            fix.AddToClassList("gm-fix-button");
            actions.Add(fix);
            Button learnMore = EditorUI.Button("Learn more", "Opens the Game Management read-me", GameManagementSettingsProvider.OpenReadMe);
            learnMore.AddToClassList("gm-link");
            actions.Add(learnMore);
            box.Add(actions);
            return box;
        }
    }
}
