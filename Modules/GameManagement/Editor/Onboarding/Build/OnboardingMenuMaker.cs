using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Copies the library's menu prefabs into the project and sets them up, or adds a State Menu Reactor to the developer's own prefab.</summary>
    internal class OnboardingMenuMaker
    {
        private readonly string folder;
        private readonly OnboardingResult result;

        internal OnboardingMenuMaker(string _menuFolder, OnboardingResult _result)
        {
            folder = _menuFolder;
            result = _result;
        }

        /// <summary>Copies the library prefab under its own file name (a taken name gets a unique one) and wires it.</summary>
        internal void Copy(string _libraryPath, OnboardingMenuWiring _wiring)
        {
            string wanted = folder + "/" + System.IO.Path.GetFileName(_libraryPath);
            string path = AssetDatabase.GenerateUniqueAssetPath(wanted);
            if (!AssetDatabase.CopyAsset(_libraryPath, path))
            {
                throw new System.InvalidOperationException("The library menu '" + _libraryPath + "' could not be copied to '" + path + "'. Reimport the library and run the setup again.");
            }
            result.CreatedPaths.Add(path);
            if (path != wanted)
            {
                result.Renamed.Add("A file named '" + System.IO.Path.GetFileName(wanted) + "' was already there, so the menu was saved as '" + System.IO.Path.GetFileName(path) + "'.");
            }
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                OnboardingMenuPrefab.Wire(root, _wiring);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Adds the reactor to the developer's prefab. A prefab outside Assets is not touched and the reason is reported.</summary>
        internal void AddReactorToOwn(GameObject _prefab, StateSO _state)
        {
            string path = AssetDatabase.GetAssetPath(_prefab);
            if (!GameManagementSettingsAssets.IsProjectPath(path))
            {
                result.Skipped.Add("'" + _prefab.name + "' is not inside this project's Assets folder, so it was left as it is. Copy it into Assets, add a State Menu Reactor set to " + _state.name + ", and wire your buttons.");
                return;
            }
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<StateMenuReactor>() != null)
                {
                    result.Skipped.Add("'" + _prefab.name + "' already has a State Menu Reactor, so it was left as it is. Set the reactor's State to " + _state.name + " if it should show this menu.");
                    return;
                }
                if (!root.activeSelf)
                {
                    result.Skipped.Add("The top object of '" + _prefab.name + "' is switched off, so its State Menu Reactor cannot run. Switch it on; the reactor shows and hides the menu's first child.");
                }
                OnboardingMenuPrefab.AddReactor(root, _state);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
