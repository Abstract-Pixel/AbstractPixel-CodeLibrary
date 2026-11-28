namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Names and library prefab paths of the menus the setup window makes. The prefab names are the library's own,
    /// including the space before the dot in "Game Over Menu .prefab".</summary>
    public static class OnboardingMenuInfo
    {
        public const string PlayStateName = "Play";
        public const string SettingsStateName = "Settings";
        public const string MenuFolderName = "Menus";
        public const string StateFolderName = "States";
        private const string PrefabFolder = "Packages/com.abstractpixel.library/Modules/GameManagement/Runtime/MenuPrefabs/";

        public static string DisplayName(OnboardingMenuKind _kind)
        {
            switch (_kind)
            {
                case OnboardingMenuKind.MainMenu:
                    return "Main Menu";
                case OnboardingMenuKind.Pause:
                    return "Pause";
                case OnboardingMenuKind.GameOver:
                    return "Game Over";
                default:
                    return "Game Win";
            }
        }

        /// <summary>The state asset's name. Same as the display name, so a run later can tell which menus already exist.</summary>
        public static string StateName(OnboardingMenuKind _kind)
        {
            return DisplayName(_kind);
        }

        public static string LibraryPrefabPath(OnboardingMenuKind _kind)
        {
            switch (_kind)
            {
                case OnboardingMenuKind.MainMenu:
                    return PrefabFolder + "StartMenu.prefab";
                case OnboardingMenuKind.Pause:
                    return PrefabFolder + "Pause Menu.prefab";
                case OnboardingMenuKind.GameOver:
                    return PrefabFolder + "Game Over Menu .prefab";
                default:
                    return PrefabFolder + "Game Win Menu.prefab";
            }
        }

        public static string LibrarySettingsPrefabPath
        {
            get { return PrefabFolder + "Settings Menu.prefab"; }
        }

        /// <summary>Only the menus a player opens Settings from can ask for a Settings screen.</summary>
        public static bool CanIncludeSettings(OnboardingMenuKind _kind)
        {
            return _kind == OnboardingMenuKind.MainMenu || _kind == OnboardingMenuKind.Pause;
        }

        /// <summary>The name without spaces, dashes, capitals or a "State" ending, so "MainMenu", "Main menu" and "PauseStateSO" are compared fairly.
        /// Projects name their states their own way, and a run later must still know which menus they already have.</summary>
        public static string Normalize(string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                return "";
            }
            string name = _name.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
            foreach (string suffix in new[] { "stateso", "state" })
            {
                if (name.Length > suffix.Length && name.EndsWith(suffix))
                {
                    return name.Substring(0, name.Length - suffix.Length);
                }
            }
            return name;
        }

        /// <summary>True when a state name (spacing, capitals and a "State" ending ignored) is one this menu is usually called.</summary>
        public static bool IsStateNameOf(OnboardingMenuKind _kind, string _stateName)
        {
            string name = Normalize(_stateName);
            if (_kind == OnboardingMenuKind.MainMenu)
            {
                return name == "mainmenu" || name == "start" || name == "startmenu";
            }
            return name == Normalize(StateName(_kind));
        }

        /// <summary>Where a state sits in the list; lower is higher up, so it wins. Sub-states sit above their parents.
        /// Names the setup window does not know have no rank (-1).</summary>
        public static int ListRank(string _stateName)
        {
            if (IsStateNameOf(OnboardingMenuKind.MainMenu, _stateName))
            {
                return 5;
            }
            switch (Normalize(_stateName))
            {
                case "gamewin":
                    return 0;
                case "gameover":
                    return 1;
                case "settings":
                    return 2;
                case "pause":
                    return 3;
                case "play":
                    return 4;
                default:
                    return -1;
            }
        }
    }
}
