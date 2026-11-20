using System;
using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Finds the scene system settings asset by type (its GUID is cached for the Editor session, never a path), keeps it
    /// in Player Settings' Preloaded Assets, keeps its lists of scene sets and scene groups current, and pushes where new
    /// scenes go into Core's scene field. &lt;see cref="Changed"/&gt; tells open windows to refresh.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSystemSettingsAssets
    {
        private const string CacheKey = "AbstractPixel.SceneSystem.SettingsGuid";
        private const string LogPrefix = "[Scene system]";

        /// <summary>Raised after the settings, a scene set or a scene group changed in the project.</summary>
        public static event Action Changed;

        static SceneSystemSettingsAssets()
        {
            EditorIdleQueue.Run(PushToCore);
        }

        public static string CachedGuid
        {
            get { return SessionState.GetString(CacheKey, string.Empty); }
            set { SessionState.SetString(CacheKey, value ?? string.Empty); }
        }

        /// <summary>The preloaded list without empty entries and with the settings asset in it once.</summary>
        public static UnityEngine.Object[] WithSettingsPreloaded(UnityEngine.Object[] _preloaded, UnityEngine.Object _settings)
        {
            List<UnityEngine.Object> repaired = new List<UnityEngine.Object>();
            bool hasSettings = false;
            if (_preloaded != null)
            {
                foreach (UnityEngine.Object item in _preloaded)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    if (item == _settings)
                    {
                        hasSettings = true;
                    }
                    repaired.Add(item);
                }
            }
            if (_settings != null && !hasSettings)
            {
                repaired.Add(_settings);
            }
            return repaired.ToArray();
        }

        /// <summary>The cached GUID while it still leads to an asset, otherwise the first settings asset found by type.</summary>
        public static string PickSettingsGuid(string _cachedGuid, Func<string, string> _guidToPath, Func<string[]> _findAllGuids)
        {
            if (!string.IsNullOrEmpty(_cachedGuid))
            {
                string cachedPath = _guidToPath(_cachedGuid);
                if (!string.IsNullOrEmpty(cachedPath))
                {
                    return _cachedGuid;
                }
            }
            string[] found = _findAllGuids();
            if (found == null || found.Length == 0)
            {
                return string.Empty;
            }
            return found[0];
        }

        public static SceneSystemSettings Find()
        {
            string guid = PickSettingsGuid(CachedGuid, AssetDatabase.GUIDToAssetPath, FindAllGuids);
            SceneSystemSettings settings = LoadByGuid(guid);
            if (settings == null && guid == CachedGuid && !string.IsNullOrEmpty(guid))
            {
                // The cached GUID now names something else (or a deleted file Unity still remembers): search again.
                CachedGuid = string.Empty;
                guid = PickSettingsGuid(string.Empty, AssetDatabase.GUIDToAssetPath, FindAllGuids);
                settings = LoadByGuid(guid);
            }
            CachedGuid = settings == null ? string.Empty : guid;
            return settings;
        }

        /// <summary>Caches this asset as the project's scene system settings.</summary>
        public static void Remember(SceneSystemSettings _settings)
        {
            if (_settings == null)
            {
                CachedGuid = string.Empty;
                return;
            }
            CachedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_settings));
        }

        /// <summary>Every scene system settings asset under Assets/. More than one is a mistake the page offers to fix.</summary>
        public static List<string> FindAllPaths()
        {
            List<string> paths = new List<string>();
            foreach (string guid in FindAllGuids())
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return paths;
        }

        /// <summary>The folder that holds the settings asset; new scene sets and the migration report go there.</summary>
        public static string FolderOf(SceneSystemSettings _settings)
        {
            string path = AssetDatabase.GetAssetPath(_settings);
            return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        public static bool IsPreloaded(SceneSystemSettings _settings)
        {
            if (_settings == null)
            {
                return false;
            }
            foreach (UnityEngine.Object item in PlayerSettings.GetPreloadedAssets())
            {
                if (item == _settings)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Puts the settings into Preloaded Assets (and drops empty entries). Returns true when the list changed.</summary>
        public static bool EnsurePreloaded(SceneSystemSettings _settings)
        {
            UnityEngine.Object[] current = PlayerSettings.GetPreloadedAssets();
            UnityEngine.Object[] repaired = WithSettingsPreloaded(current, _settings);
            bool unchanged = SameItems(new List<UnityEngine.Object>(current), new List<UnityEngine.Object>(repaired));
            if (unchanged)
            {
                return false;
            }
            PlayerSettings.SetPreloadedAssets(repaired);
            AssetDatabase.SaveAssets();
            Debug.Log(LogPrefix + " Put the scene system settings into Player Settings > Preloaded Assets, so a build can read them before any scene opens.", _settings);
            return true;
        }

        /// <summary>Rebuilds the settings' lists of scene sets and scene groups from the project. Returns true when they changed.</summary>
        public static bool RefreshLists(SceneSystemSettings _settings)
        {
            if (_settings == null)
            {
                return false;
            }
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
            List<SceneSet> sets = FindAllSets();
            bool groupsSame = SameItems(_settings.SceneGroups, groups);
            bool setsSame = SameItems(_settings.SceneSets, sets);
            if (groupsSame && setsSame)
            {
                return false;
            }
            _settings.SceneGroups.Clear();
            _settings.SceneGroups.AddRange(groups);
            _settings.SceneSets.Clear();
            _settings.SceneSets.AddRange(sets);
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(_settings);
            return true;
        }

        public static List<SceneSet> FindAllSets()
        {
            List<string> paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneSet), new[] { "Assets" }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(StringComparer.OrdinalIgnoreCase);
            List<SceneSet> sets = new List<SceneSet>();
            foreach (string path in paths)
            {
                SceneSet set = AssetDatabase.LoadAssetAtPath<SceneSet>(path);
                if (set != null)
                {
                    sets.Add(set);
                }
            }
            return sets;
        }

        /// <summary>
        /// Moves the settings asset into another folder inside Assets/, keeping its name. Returns why it can't, or empty
        /// when it moved (or was already there).
        /// </summary>
        public static string MoveTo(SceneSystemSettings _settings, string _folder)
        {
            if (_settings == null)
            {
                return "There is no settings asset to move.";
            }
            string problem = SceneSystemSetup.FolderProblemInProject(_folder);
            if (!string.IsNullOrEmpty(problem))
            {
                return problem;
            }
            string folder = SceneSystemSetup.NormalizeFolder(_folder);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return "That folder is not in the project.";
            }
            string from = AssetDatabase.GetAssetPath(_settings);
            string to = folder + "/" + System.IO.Path.GetFileName(from);
            if (from == to)
            {
                return string.Empty;
            }
            if (AssetDatabase.LoadMainAssetAtPath(to) != null)
            {
                return "That folder already has a file named " + System.IO.Path.GetFileName(from) + ".";
            }
            string error = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }
            Debug.Log(LogPrefix + " Moved the scene system settings to " + to + ".", _settings);
            return string.Empty;
        }

        /// <summary>Saves a change made on the page and tells open windows.</summary>
        public static void Save(SceneSystemSettings _settings)
        {
            if (_settings == null)
            {
                return;
            }
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(_settings);
            NotifyChanged();
        }

        public static void NotifyChanged()
        {
            PushToCore();
            if (Changed != null)
            {
                Changed();
            }
        }

        /// <summary>Core's scene field offers the fix for where new scenes go; Core can't read the scene module, so it is told.</summary>
        public static void PushToCore()
        {
            SceneSystemSettings settings = Find();
            bool toAddressables = settings != null && settings.NewScenesAreAddressable;
            SceneStatusReader.ProjectDefaultPlace = toAddressables ? ScenePlace.Addressables : ScenePlace.BuildSettings;
        }

        private static string[] FindAllGuids()
        {
            List<string> guids = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneSystemSettings), new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SceneSystemSettings))
                {
                    guids.Add(guid);
                }
            }
            guids.Sort(StringComparer.Ordinal);
            return guids.ToArray();
        }

        private static SceneSystemSettings LoadByGuid(string _guid)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneSystemSettings>(path);
        }

        private static bool SameItems<T>(List<T> _current, List<T> _found) where T : UnityEngine.Object
        {
            // Compares references, so a destroyed entry (null to Unity) never counts as the settings asset.
            if (_current.Count != _found.Count)
            {
                return false;
            }
            for (int index = 0; index < _current.Count; index++)
            {
                if (_current[index] != _found[index])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
