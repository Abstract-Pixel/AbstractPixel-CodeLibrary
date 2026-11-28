using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Makes everything the setup window collected. Nothing existing is written over; if a step fails, everything
    /// this run made is deleted again and the error says what to fix.</summary>
    public static class OnboardingBuilder
    {
        public static OnboardingResult Run(OnboardingChoices _choices)
        {
            OnboardingResult result = new OnboardingResult();
            Object[] preloadedBefore = PlayerSettings.GetPreloadedAssets();
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set up game management");
            try
            {
                Build(_choices, result);
            }
            catch (System.Exception)
            {
                Undo.RevertAllDownToGroup(group);
                RemoveEverythingMade(result);
                PlayerSettings.SetPreloadedAssets(preloadedBefore);
                throw;
            }
            Undo.CollapseUndoOperations(group);
            return result;
        }

        private static void Build(OnboardingChoices _choices, OnboardingResult _result)
        {
            GameManagementSettings settings = _choices.SettingsExist ? GameManagementSettingsAssets.FindProjectSettings() : null;
            if (_choices.SettingsExist && (settings == null || settings.Catalog == null))
            {
                throw new System.InvalidOperationException("This project's game management settings or its state list are gone. Open Project Settings > Abstract Pixel > Game Management and use Create settings first.");
            }
            string folder = _choices.SettingsExist ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(settings)).Replace('\\', '/') : OnboardingChoices.CleanFolder(_choices.SettingsFolder);
            string problem = OnboardingChoices.FolderProblem(folder);
            if (problem != null)
            {
                throw new System.InvalidOperationException("The settings folder '" + folder + "' cannot be used: " + problem);
            }

            bool isNew = settings == null;
            if (isNew)
            {
                settings = CreateSettings(folder, _choices, _result);
            }
            _result.Settings = settings;

            string stateFolder = MakeFolder(folder + "/" + OnboardingMenuInfo.StateFolderName, _result);
            OnboardingStateMaker states = new OnboardingStateMaker(stateFolder, settings.Catalog, _result);
            Dictionary<OnboardingMenuKind, StateSO> menuStates = MakeStates(_choices, settings.Catalog, states, out StateSO play, out StateSO settingsState);

            bool copiesAny = _choices.NewMenus().Exists(_menu => !_menu.UseOwnMenu) || (_choices.MakesSettingsMenu && !_choices.UseOwnSettingsMenu);
            string menuFolder = copiesAny ? MakeFolder(folder + "/" + OnboardingMenuInfo.MenuFolderName, _result) : null;
            MakeMenus(_choices, menuFolder, menuStates, play, settingsState, settings.Catalog, _result);

            _result.ToDo.AddRange(OnboardingPlan.ToDoItems(_choices));
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (isNew)
            {
                GameManagementSettingsAssets.Use(settings);
            }
        }

        private static GameManagementSettings CreateSettings(string _folder, OnboardingChoices _choices, OnboardingResult _result)
        {
            MakeFolder(_folder, _result);
            GameManagementSettings settings = GameManagementSettingsAssets.Create(_folder);
            _result.CreatedPaths.Add(AssetDatabase.GetAssetPath(settings.Catalog));
            _result.CreatedPaths.Add(AssetDatabase.GetAssetPath(settings));
            settings.InputSource = _choices.InputSource;
            settings.InputActions = _choices.InputSource == InputSourceKind.ChosenAsset ? _choices.InputActions : null;
            return settings;
        }

        // Records the outermost folder made, so deleting it later removes every folder this run added and nothing else.
        private static string MakeFolder(string _folder, OnboardingResult _result)
        {
            string outermost = GameManagementSettingsAssets.EnsureFolder(_folder);
            if (outermost != null)
            {
                _result.CreatedPaths.Add(outermost);
            }
            if (!AssetDatabase.IsValidFolder(_folder))
            {
                throw new System.InvalidOperationException("The folder '" + _folder + "' could not be made. A file with the same name as one of its parts may be in the way. Pick another settings folder.");
            }
            return _folder;
        }

        private static Dictionary<OnboardingMenuKind, StateSO> MakeStates(OnboardingChoices _choices, GameStateCatalog _catalog, OnboardingStateMaker _states, out StateSO _play, out StateSO _settingsState)
        {
            _play = OnboardingStateMaker.FindListed(_catalog, OnboardingMenuInfo.PlayStateName);
            if (_play == null)
            {
                _play = _states.Make(OnboardingMenuInfo.PlayStateName, new List<StateSO>());
            }
            Dictionary<OnboardingMenuKind, StateSO> menuStates = new Dictionary<OnboardingMenuKind, StateSO>();
            foreach (OnboardingMenuChoice menu in _choices.TickedMenus())
            {
                string name = OnboardingMenuInfo.StateName(menu.Kind);
                if (menu.Locked)
                {
                    menuStates[menu.Kind] = OnboardingStateMaker.FindListed(_catalog, name);
                    continue;
                }
                List<StateSO> parents = new List<StateSO>();
                if (menu.Kind == OnboardingMenuKind.Pause)
                {
                    parents.Add(_play);
                }
                menuStates[menu.Kind] = _states.Make(name, parents);
            }

            _settingsState = OnboardingStateMaker.FindListed(_catalog, OnboardingMenuInfo.SettingsStateName);
            List<StateSO> settingsParents = _choices.SettingsParents().ConvertAll(_menu => menuStates[_menu.Kind]);
            if (_settingsState == null && settingsParents.Count > 0)
            {
                _settingsState = _states.Make(OnboardingMenuInfo.SettingsStateName, settingsParents);
            }
            else if (_settingsState != null)
            {
                AddMissingParents(_settingsState, settingsParents);
            }
            return menuStates;
        }

        private static void AddMissingParents(StateSO _state, List<StateSO> _parents)
        {
            foreach (StateSO parent in _parents)
            {
                if (!_state.Parents.Contains(parent))
                {
                    Undo.RecordObject(_state, "Set up game management");
                    _state.Parents.Add(parent);
                    EditorUtility.SetDirty(_state);
                }
            }
        }

        private static void MakeMenus(OnboardingChoices _choices, string _menuFolder, Dictionary<OnboardingMenuKind, StateSO> _menuStates, StateSO _play, StateSO _settingsState, GameStateCatalog _catalog, OnboardingResult _result)
        {
            OnboardingMenuMaker maker = new OnboardingMenuMaker(_menuFolder, _result);
            StateSO mainMenu = _menuStates.ContainsKey(OnboardingMenuKind.MainMenu) ? _menuStates[OnboardingMenuKind.MainMenu] : null;
            foreach (OnboardingMenuChoice menu in _choices.NewMenus())
            {
                StateSO state = _menuStates[menu.Kind];
                if (menu.UseOwnMenu)
                {
                    if (menu.OwnPrefab != null)
                    {
                        maker.AddReactorToOwn(menu.OwnPrefab, state);
                    }
                    continue;
                }
                OnboardingMenuWiring wiring = new OnboardingMenuWiring();
                wiring.Kind = menu.Kind;
                wiring.State = state;
                wiring.Play = _play;
                wiring.MainMenu = mainMenu;
                wiring.Settings = _settingsState;
                wiring.HasSettingsButton = menu.WantsSettings;
                maker.Copy(OnboardingMenuInfo.LibraryPrefabPath(menu.Kind), wiring);
            }
            if (!_choices.MakesSettingsMenu)
            {
                return;
            }
            if (_choices.UseOwnSettingsMenu)
            {
                if (_choices.OwnSettingsPrefab != null)
                {
                    maker.AddReactorToOwn(_choices.OwnSettingsPrefab, _settingsState);
                }
                return;
            }
            OnboardingMenuWiring settingsWiring = new OnboardingMenuWiring();
            settingsWiring.IsSettingsMenu = true;
            settingsWiring.State = _settingsState;
            maker.Copy(OnboardingMenuInfo.LibrarySettingsPrefabPath, settingsWiring);
        }

        private static void RemoveEverythingMade(OnboardingResult _result)
        {
            for (int index = _result.CreatedPaths.Count - 1; index >= 0; index--)
            {
                AssetDatabase.DeleteAsset(_result.CreatedPaths[index]);
            }
            _result.CreatedPaths.Clear();
            AssetDatabase.SaveAssets();
        }
    }
}
