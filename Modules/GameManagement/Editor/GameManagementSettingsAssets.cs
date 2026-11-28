using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Finds the game management settings asset wherever it lives and keeps it in Player Settings' Preloaded Assets,
    /// so builds load it from any folder. Same method as the scene system's settings.</summary>
    [InitializeOnLoad]
    public static class GameManagementSettingsAssets
    {
        static GameManagementSettingsAssets()
        {
            GameManagementSettingsSource.EditorLookup = Find;
        }

        /// <summary>The project's settings if it has any, otherwise whatever settings exist in a package (a sample's). A project's own
        /// settings always win over a package's. Read-only for a package one: check <see cref="IsProjectPath"/> before writing.</summary>
        public static GameManagementSettings Find()
        {
            GameManagementSettings project = FindProjectSettings();
            if (project != null)
            {
                return project;
            }
            return FindInPreloadedOrAnywhere(false);
        }

        /// <summary>The settings inside this project's Assets folder: the one in Preloaded Assets if any, otherwise the first found by
        /// type. Null when the project has none, even if a package ships some.</summary>
        public static GameManagementSettings FindProjectSettings()
        {
            return FindInPreloadedOrAnywhere(true);
        }

        private static GameManagementSettings FindInPreloadedOrAnywhere(bool _projectOnly)
        {
            foreach (Object item in PlayerSettings.GetPreloadedAssets())
            {
                if (item is GameManagementSettings preloaded && IsWanted(preloaded, _projectOnly))
                {
                    return preloaded;
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(GameManagementSettings)))
            {
                GameManagementSettings found = AssetDatabase.LoadAssetAtPath<GameManagementSettings>(AssetDatabase.GUIDToAssetPath(guid));
                if (found != null && IsWanted(found, _projectOnly))
                {
                    return found;
                }
            }
            return null;
        }

        private static bool IsWanted(GameManagementSettings _settings, bool _projectOnly)
        {
            return !_projectOnly || IsProjectPath(AssetDatabase.GetAssetPath(_settings));
        }

        /// <summary>True for a path inside this project's Assets folder. Anything else (a package) is not the project's to edit.</summary>
        public static bool IsProjectPath(string _path)
        {
            return _path == "Assets" || (_path != null && _path.StartsWith("Assets/"));
        }

        /// <summary>True when these settings live outside Assets, for example a sample's inside the library package.</summary>
        public static bool IsPackageAsset(GameManagementSettings _settings)
        {
            return _settings != null && !IsProjectPath(AssetDatabase.GetAssetPath(_settings));
        }

        /// <summary>True when builds will load these settings.</summary>
        public static bool IsPreloaded(GameManagementSettings _settings)
        {
            return System.Array.IndexOf(PlayerSettings.GetPreloadedAssets(), _settings) >= 0;
        }

        /// <summary>Makes every missing folder of a path under Assets. Returns the outermost one it made, or null.</summary>
        internal static string EnsureFolder(string _folder)
        {
            string outermostMade = null;
            string parent = "Assets";
            foreach (string part in _folder.Substring("Assets".Length).Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string child = parent + "/" + part;
                if (!AssetDatabase.IsValidFolder(child))
                {
                    AssetDatabase.CreateFolder(parent, part);
                    if (outermostMade == null)
                    {
                        outermostMade = child;
                    }
                }
                parent = child;
            }
            return outermostMade;
        }

        /// <summary>Makes the settings and an empty state list in this folder and uses them. Existing assets are never written over.</summary>
        public static GameManagementSettings Create(string _folder)
        {
            EnsureFolder(_folder);
            string parent = _folder;
            GameStateCatalog catalog = ScriptableObject.CreateInstance<GameStateCatalog>();
            AssetDatabase.CreateAsset(catalog, AssetDatabase.GenerateUniqueAssetPath(parent + "/GameStateCatalog.asset"));
            GameManagementSettings settings = ScriptableObject.CreateInstance<GameManagementSettings>();
            settings.Catalog = catalog;
            AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath(parent + "/GameManagementSettings.asset"));
            AssetDatabase.SaveAssets();
            Use(settings);
            Debug.Log("[Game management] Created the settings in " + parent + ".", settings);
            return settings;
        }

        /// <summary>Makes this asset the project's settings: the only settings in Preloaded Assets.</summary>
        public static void Use(GameManagementSettings _settings)
        {
            Object[] current = PlayerSettings.GetPreloadedAssets();
            Object[] wanted = WithSettingsPreloaded(current, _settings);
            PlayerSettings.SetPreloadedAssets(wanted);
        }

        /// <summary>The preloaded list with empty entries and other settings removed and these settings present once. Pure.</summary>
        public static Object[] WithSettingsPreloaded(Object[] _preloaded, GameManagementSettings _settings)
        {
            List<Object> result = new List<Object>();
            foreach (Object item in _preloaded)
            {
                if (item == null || item is GameManagementSettings)
                {
                    continue;
                }
                result.Add(item);
            }
            if (_settings != null)
            {
                result.Add(_settings);
            }
            return result.ToArray();
        }

        /// <summary>Before a build, puts the found settings into Preloaded Assets so the build can load them.</summary>
        private class PreloadBeforeBuild : IPreprocessBuildWithReport
        {
            public int callbackOrder
            {
                get { return 0; }
            }

            public void OnPreprocessBuild(BuildReport _report)
            {
                GameManagementSettings settings = Find();
                if (settings != null)
                {
                    Use(settings);
                }
            }
        }
    }
}
