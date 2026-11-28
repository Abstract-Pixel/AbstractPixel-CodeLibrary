using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The words of the Ready and All set pages, worked out from the choices alone so they can be tested.</summary>
    public static class OnboardingPlan
    {
        /// <summary>Names of the states this run makes, in the order of the list: Play first when it is new, then the menus.</summary>
        public static List<string> NewStateNames(OnboardingChoices _choices, bool _playExists)
        {
            List<string> names = new List<string>();
            if (!_playExists)
            {
                names.Add(OnboardingMenuInfo.PlayStateName);
            }
            foreach (OnboardingMenuChoice menu in _choices.NewMenus())
            {
                names.Add(OnboardingMenuInfo.StateName(menu.Kind));
            }
            if (_choices.MakesSettingsMenu)
            {
                names.Add(OnboardingMenuInfo.SettingsStateName);
            }
            return names;
        }

        /// <summary>The numbered points of the Ready page.</summary>
        public static List<string> ReadyLines(OnboardingChoices _choices, bool _playExists)
        {
            List<string> lines = new List<string>();
            lines.Add(_choices.SettingsExist ? "Adds to the settings you already have" : "Settings asset in " + OnboardingChoices.CleanFolder(_choices.SettingsFolder));
            List<string> states = NewStateNames(_choices, _playExists);
            lines.Add(states.Count + " new " + (states.Count == 1 ? "state" : "states") + " in the state list: " + (states.Count == 0 ? "none" : string.Join(", ", states)));

            List<string> copied = _choices.NewMenus().FindAll(_menu => !_menu.UseOwnMenu).Select(_menu => OnboardingMenuInfo.DisplayName(_menu.Kind)).ToList();
            if (copied.Count > 0)
            {
                lines.Add(JoinWithAnd(copied) + (copied.Count == 1 ? " menu" : " menus") + ", copied from the library");
            }
            if (_choices.MakesSettingsMenu)
            {
                List<string> parents = _choices.SettingsParents().Select(_menu => OnboardingMenuInfo.DisplayName(_menu.Kind)).ToList();
                string opening = "opening over " + JoinWithAnd(parents);
                lines.Add(_choices.UseOwnSettingsMenu ? "Settings menu: " + OwnLine(_choices.OwnSettingsPrefab != null) + ". It opens over " + JoinWithAnd(parents) : "Settings menu, copied from the library, " + opening);
            }
            else if (_choices.SettingsStateExists && _choices.SettingsParents().Count > 0)
            {
                List<string> parents = _choices.SettingsParents().Select(_menu => OnboardingMenuInfo.DisplayName(_menu.Kind)).ToList();
                lines.Add("Your Settings state also opens over " + JoinWithAnd(parents));
            }
            foreach (OnboardingMenuChoice menu in _choices.NewMenus().FindAll(_menu => _menu.UseOwnMenu))
            {
                lines.Add(OnboardingMenuInfo.DisplayName(menu.Kind) + ": " + OwnLine(menu.OwnPrefab != null));
            }
            return lines;
        }

        private static string OwnLine(bool _hasPrefab)
        {
            return _hasPrefab ? "your prefab gets a menu reactor on its root" : "only the state, you make the menu";
        }

        /// <summary>One entry for each menu that was set to "Use my own": what is left to do by hand.</summary>
        public static List<OnboardingToDo> ToDoItems(OnboardingChoices _choices)
        {
            List<OnboardingToDo> items = new List<OnboardingToDo>();
            foreach (OnboardingMenuChoice menu in _choices.NewMenus().FindAll(_menu => _menu.UseOwnMenu))
            {
                items.Add(ToDoFor(OnboardingMenuInfo.DisplayName(menu.Kind), menu.OwnPrefab != null));
            }
            if (_choices.MakesSettingsMenu && _choices.UseOwnSettingsMenu)
            {
                items.Add(ToDoFor(OnboardingMenuInfo.SettingsStateName, _choices.OwnSettingsPrefab != null));
            }
            return items;
        }

        private static OnboardingToDo ToDoFor(string _name, bool _hasPrefab)
        {
            if (_hasPrefab)
            {
                return new OnboardingToDo(_name, "Wire your buttons (StateButtonRequester) on your menu prefab, and check the State Menu Reactor's Content is the part that should show.");
            }
            return new OnboardingToDo(_name, "Make a menu prefab, add a State Menu Reactor set to the " + _name + " state, and wire your buttons (StateButtonRequester).");
        }

        public static string JoinWithAnd(List<string> _names)
        {
            if (_names.Count == 0)
            {
                return "";
            }
            if (_names.Count == 1)
            {
                return _names[0];
            }
            return string.Join(", ", _names.Take(_names.Count - 1)) + " and " + _names[_names.Count - 1];
        }
    }
}
