using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>"How to set up each menu": one card for each menu this run makes, plus one for the Settings screen when a menu asks for it.</summary>
    internal class OnboardingPathsPage : OnboardingPage
    {
        private OnboardingContext context;
        private VisualElement settingsHost;

        internal override string Template
        {
            get { return "OnboardingSetupPaths"; }
        }

        internal override void Wire(VisualElement _root, OnboardingContext _context)
        {
            context = _context;
            VisualElement container = _root.Q<VisualElement>("pathContainer");
            List<OnboardingMenuChoice> menus = context.Choices.NewMenus();
            if (menus.Count == 0)
            {
                Label none = new Label("No new menu is ticked, so only the Play state is made and there is nothing to set up here.");
                none.AddToClassList("gm-hint");
                container.Add(none);
            }
            int number = 1;
            foreach (OnboardingMenuChoice menu in menus)
            {
                container.Add(MakeMenuCard(menu, number));
                number++;
            }
            settingsHost = new VisualElement();
            container.Add(settingsHost);
            RefreshSettingsCard(menus.Count + 1);
        }

        private VisualElement MakeMenuCard(OnboardingMenuChoice _menu, int _number)
        {
            VisualElement card = MakeCard(OnboardingMenuInfo.DisplayName(_menu.Kind), _number, _menu.UseOwnMenu, _menu.OwnPrefab, _useOwn =>
            {
                _menu.UseOwnMenu = _useOwn;
                if (!_useOwn)
                {
                    _menu.OwnPrefab = null;
                }
            }, _prefab => _menu.OwnPrefab = _prefab);

            Toggle settingsToggle = card.Q<Toggle>("settingsToggle");
            Label settingsHint = card.Q<Label>("settingsHint");
            if (!OnboardingMenuInfo.CanIncludeSettings(_menu.Kind))
            {
                settingsToggle.style.display = DisplayStyle.None;
                settingsHint.style.display = DisplayStyle.None;
                return card;
            }
            settingsHint.text = "<b>NOTE:</b> Adds a Settings sub-state, opening over " + OnboardingMenuInfo.DisplayName(_menu.Kind) + ".";
            settingsToggle.SetValueWithoutNotify(_menu.IncludeSettings);
            settingsHint.style.display = _menu.IncludeSettings ? DisplayStyle.Flex : DisplayStyle.None;
            settingsToggle.RegisterValueChangedCallback(_change =>
            {
                _menu.IncludeSettings = _change.newValue;
                settingsHint.style.display = _change.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                RefreshSettingsCard(context.Choices.NewMenus().Count + 1);
            });
            return card;
        }

        // The Settings screen has its own card, only while some menu asks for one and the project has no Settings state yet.
        private void RefreshSettingsCard(int _number)
        {
            settingsHost.Clear();
            if (!context.Choices.MakesSettingsMenu)
            {
                return;
            }
            OnboardingChoices choices = context.Choices;
            VisualElement card = MakeCard(OnboardingMenuInfo.SettingsStateName, _number, choices.UseOwnSettingsMenu, choices.OwnSettingsPrefab, _useOwn =>
            {
                choices.UseOwnSettingsMenu = _useOwn;
                if (!_useOwn)
                {
                    choices.OwnSettingsPrefab = null;
                }
            }, _prefab => choices.OwnSettingsPrefab = _prefab);
            card.Q<Toggle>("settingsToggle").style.display = DisplayStyle.None;
            List<string> parents = choices.SettingsParents().ConvertAll(_menu => OnboardingMenuInfo.DisplayName(_menu.Kind));
            Label hint = card.Q<Label>("settingsHint");
            hint.text = "<b>NOTE:</b> One Settings screen, opening over " + OnboardingPlan.JoinWithAnd(parents) + ".";
            hint.style.display = DisplayStyle.Flex;
            settingsHost.Add(card);
        }

        private VisualElement MakeCard(string _name, int _number, bool _useOwn, GameObject _prefab, System.Action<bool> _onPath, System.Action<GameObject> _onPrefab)
        {
            VisualElement card = OnboardingPageLoader.LoadTemplate("OnboardingPathCard").CloneTree().Q<VisualElement>("pathCard");
            card.Q<Label>("cardIndex").text = _number.ToString();
            Label name = card.Q<Label>("cardName");
            name.text = _name;
            name.style.whiteSpace = WhiteSpace.Normal;

            RadioButtonGroup radio = card.Q<RadioButtonGroup>("pathRadio");
            radio.choices = new List<string> { "Copy the library's menu", "Use my own" };
            VisualElement ownExtra = card.Q<VisualElement>("ownExtra");
            ObjectField field = card.Q<ObjectField>("prefab-menu");
            field.objectType = typeof(GameObject);
            field.allowSceneObjects = false;
            field.SetValueWithoutNotify(_prefab);
            Label problem = card.Q<Label>("prefabProblem");
            ShowPath(card, ownExtra, _useOwn);
            radio.SetValueWithoutNotify(_useOwn ? 1 : 0);
            radio.RegisterValueChangedCallback(_change =>
            {
                bool useOwn = _change.newValue == 1;
                _onPath(useOwn);
                ShowPath(card, ownExtra, useOwn);
                if (!useOwn)
                {
                    field.SetValueWithoutNotify(null);
                    problem.RemoveFromClassList("gm-shown");
                }
            });
            field.RegisterValueChangedCallback(_change =>
            {
                GameObject picked = _change.newValue as GameObject;
                string reason = PrefabProblem(picked);
                problem.text = reason;
                problem.EnableInClassList("gm-shown", reason != null);
                if (reason != null)
                {
                    field.SetValueWithoutNotify(null);
                    picked = null;
                }
                _onPrefab(picked);
            });
            card.Q<Button>("learnMore").clicked += GameManagementSettingsProvider.OpenReadMe;
            return card;
        }

        private static void ShowPath(VisualElement _card, VisualElement _ownExtra, bool _useOwn)
        {
            _card.EnableInClassList("gm-selected", _useOwn);
            _ownExtra.style.display = _useOwn ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Null when the object is a prefab asset the setup can add the reactor to, otherwise why it cannot be used.</summary>
        internal static string PrefabProblem(GameObject _picked)
        {
            if (_picked == null)
            {
                return null;
            }
            PrefabAssetType type = PrefabUtility.GetPrefabAssetType(_picked);
            if (!EditorUtility.IsPersistent(_picked) || (type != PrefabAssetType.Regular && type != PrefabAssetType.Variant))
            {
                return "Pick a prefab from the Project window, not a scene object or a model.";
            }
            return null;
        }
    }
}
