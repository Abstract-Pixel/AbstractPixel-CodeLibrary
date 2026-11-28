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

        /// <summary>The settings in Preloaded Assets if any (the project's choice), otherwise the first found by type.</summary>
        public static GameManagementSettings Find()
        {
            foreach (Object item in PlayerSettings.GetPreloadedAssets())
            {
                if (item is GameManagementSettings preloaded)
                {
                    return preloaded;
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(GameManagementSettings));
            if (guids.Length == 0)
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<GameManagementSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>True when builds will load these settings.</summary>
        public static bool IsPreloaded(GameManagementSettings _settings)
        {
            return System.Array.IndexOf(PlayerSettings.GetPreloadedAssets(), _settings) >= 0;
        }

        /// <summary>Makes the settings and an empty state list in this folder and uses them. Existing assets are never written over.</summary>
        public static GameManagementSettings Create(string _folder)
        {
            string parent = "Assets";
            foreach (string part in _folder.Substring("Assets".Length).Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (!AssetDatabase.IsValidFolder(parent + "/" + part))
                {
                    AssetDatabase.CreateFolder(parent, part);
                }
                parent += "/" + part;
            }
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
