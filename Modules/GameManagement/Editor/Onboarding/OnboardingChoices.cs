using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Everything the setup window collects before it makes anything. Nothing is written until Finish.</summary>
    public class OnboardingChoices
    {
        private readonly OnboardingMenuChoice[] menus;

        public string SettingsFolder = GameManagementSettings.DefaultFolder;
        public InputSourceKind InputSource = InputSourceKind.None;
        public UnityEngine.InputSystem.InputActionAsset InputActions;
        /// <summary>The Settings screen's own menu: copy the library's or use the developer's.</summary>
        public bool UseOwnSettingsMenu;
        public GameObject OwnSettingsPrefab;
        /// <summary>True once the project has settings (a run later): the folder and input pages are skipped.</summary>
        public bool SettingsExist;
        /// <summary>The project's Settings state already exists, so new menus only become its parents.</summary>
        public bool SettingsStateExists;

        public OnboardingChoices()
        {
            menus = new OnboardingMenuChoice[4];
            foreach (OnboardingMenuKind kind in System.Enum.GetValues(typeof(OnboardingMenuKind)))
            {
                menus[(int)kind] = new OnboardingMenuChoice(kind);
            }
            Get(OnboardingMenuKind.MainMenu).Ticked = true;
            Get(OnboardingMenuKind.Pause).Ticked = true;
            Get(OnboardingMenuKind.Pause).IncludeSettings = true;
            Get(OnboardingMenuKind.GameOver).Ticked = true;
        }

        /// <summary>The choices for a run later: the menus that already exist are ticked and locked, every other menu starts unticked.</summary>
        public static OnboardingChoices ForExistingSettings(GameManagementSettings _settings)
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.SettingsExist = true;
            foreach (OnboardingMenuKind kind in System.Enum.GetValues(typeof(OnboardingMenuKind)))
            {
                OnboardingMenuChoice menu = choices.Get(kind);
                bool exists = _settings.Catalog != null && OnboardingStateMaker.FindListed(_settings.Catalog, OnboardingMenuInfo.StateName(kind)) != null;
                menu.Ticked = exists;
                menu.Locked = exists;
            }
            choices.SettingsStateExists = _settings.Catalog != null && OnboardingStateMaker.FindListed(_settings.Catalog, OnboardingMenuInfo.SettingsStateName) != null;
            return choices;
        }

        public OnboardingMenuChoice Get(OnboardingMenuKind _kind)
        {
            return menus[(int)_kind];
        }

        /// <summary>Menus ticked now, in the window's order, existing ones included.</summary>
        public List<OnboardingMenuChoice> TickedMenus()
        {
            List<OnboardingMenuChoice> ticked = new List<OnboardingMenuChoice>();
            foreach (OnboardingMenuChoice menu in menus)
            {
                if (menu.Ticked)
                {
                    ticked.Add(menu);
                }
            }
            return ticked;
        }

        /// <summary>Menus this run makes.</summary>
        public List<OnboardingMenuChoice> NewMenus()
        {
            return TickedMenus().FindAll(_menu => _menu.IsNew);
        }

        /// <summary>The new menus that ask for a Settings screen: the parents of the Settings state.</summary>
        public List<OnboardingMenuChoice> SettingsParents()
        {
            return NewMenus().FindAll(_menu => _menu.WantsSettings);
        }

        /// <summary>True when this run makes the Settings state and its menu: some new menu asked for one and none exists yet.</summary>
        public bool MakesSettingsMenu
        {
            get { return !SettingsStateExists && SettingsParents().Count > 0; }
        }

        /// <summary>Null when the folder is fine, otherwise what is wrong with it, in words for the status row.</summary>
        public static string FolderProblem(string _folder)
        {
            if (string.IsNullOrWhiteSpace(_folder))
            {
                return "Type a folder inside Assets/.";
            }
            string folder = CleanFolder(_folder);
            if (folder != "Assets" && !folder.StartsWith("Assets/"))
            {
                return "Must be inside Assets/.";
            }
            if (folder.Contains("\\") || folder.Contains("//") || folder.Contains("..") || folder.IndexOfAny(new[] { ':', '*', '?', '"', '<', '>', '|' }) >= 0)
            {
                return "Use / between folders and no : * ? \" < > | characters.";
            }
            return null;
        }

        /// <summary>The folder without spaces around it or a slash at the end.</summary>
        public static string CleanFolder(string _folder)
        {
            return _folder.Trim().TrimEnd('/');
        }
    }
}
