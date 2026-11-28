using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Finds, before anything is made, the files that already have the name a new one would get. The setup never writes
    /// over a file: a new one gets a unique name, and the Ready page says so.</summary>
    public static class OnboardingClashes
    {
        /// <summary>One sentence for each name that is taken.</summary>
        public static List<string> Find(OnboardingChoices _choices, string _folder, bool _playExists)
        {
            List<string> notes = new List<string>();
            if (!_choices.SettingsExist)
            {
                Check(_folder + "/GameStateCatalog.asset", notes);
                Check(_folder + "/GameManagementSettings.asset", notes);
            }
            foreach (string state in OnboardingPlan.NewStateNames(_choices, _playExists))
            {
                Check(_folder + "/" + OnboardingMenuInfo.StateFolderName + "/" + state + ".asset", notes);
            }
            foreach (OnboardingMenuChoice menu in _choices.NewMenus())
            {
                if (!menu.UseOwnMenu)
                {
                    Check(_folder + "/" + OnboardingMenuInfo.MenuFolderName + "/" + Path.GetFileName(OnboardingMenuInfo.LibraryPrefabPath(menu.Kind)), notes);
                }
            }
            foreach (OnboardingMenuChoice menu in _choices.NewMenus())
            {
                if (menu.UseOwnMenu && menu.OwnPrefab != null && menu.OwnPrefab.GetComponent<StateMenuReactor>() != null)
                {
                    notes.Add("'" + menu.OwnPrefab.name + "' already has a State Menu Reactor, so it will be left as it is.");
                }
            }
            if (_choices.MakesSettingsMenu && !_choices.UseOwnSettingsMenu)
            {
                Check(_folder + "/" + OnboardingMenuInfo.MenuFolderName + "/" + Path.GetFileName(OnboardingMenuInfo.LibrarySettingsPrefabPath), notes);
            }
            return notes;
        }

        private static void Check(string _path, List<string> _notes)
        {
            if (!File.Exists(_path))
            {
                return;
            }
            string unique = AssetDatabase.GenerateUniqueAssetPath(_path);
            _notes.Add("'" + Path.GetFileName(_path) + "' already exists in " + Path.GetDirectoryName(_path).Replace('\\', '/') + ", so the new one will be saved as '" + Path.GetFileName(unique) + "'.");
        }
    }
}
