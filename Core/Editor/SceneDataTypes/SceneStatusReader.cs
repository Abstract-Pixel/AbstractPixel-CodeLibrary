using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Reads a scene's status from the project (scene asset, Build Settings, Addressables) and applies its fix.
    /// Each fix also removes the scene's entry from the other place, so a scene is never In build and Addressable.
    /// </summary>
    public static class SceneStatusReader
    {
        /// <summary>
        /// Where a scene that is Not in build goes. The scene module sets it from the scene system settings (Core never
        /// references the scene module); Build Settings until it does.
        /// </summary>
        public static ScenePlace ProjectDefaultPlace { get; set; } = ScenePlace.BuildSettings;

        private const string LogPrefix = "[Scene system]";

        public static SceneStatusReport Read(SceneField _field)
        {
            if (_field == null)
            {
                return Read(null, string.Empty, string.Empty, string.Empty);
            }
            return Read(_field.SceneAssetInEditor, _field.SceneName, _field.SceneGuid, _field.ScenePath);
        }

        /// <summary>Reads the status from the field's scene asset, falling back to its stored GUID and path.</summary>
        public static SceneStatusReport Read(SceneAsset _sceneAsset, string _storedName, string _storedGuid, string _storedPath)
        {
            SceneAsset sceneAsset = ResolveSceneAsset(_sceneAsset, _storedGuid, _storedPath);
            bool hasSceneAsset = sceneAsset != null;

            string scenePath = _storedPath;
            string sceneGuid = _storedGuid;
            string sceneName = StoredSceneName(_storedName, _storedPath);
            if (hasSceneAsset)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
                sceneName = sceneAsset.name;
            }

            IAddressableSceneProvider provider = AddressableScenes.Provider;
            bool addressablesInstalled = provider != null;

            SceneStatusFacts facts = new SceneStatusFacts();
            facts.HasSceneAsset = hasSceneAsset;
            facts.HasStoredScene = !string.IsNullOrEmpty(_storedName) || !string.IsNullOrEmpty(_storedGuid) || !string.IsNullOrEmpty(_storedPath);
            facts.IsInBuild = hasSceneAsset && SceneBuildSettingsList.IsSwitchedOn(EditorBuildSettings.scenes, scenePath);
            facts.IsAddressable = hasSceneAsset && addressablesInstalled && provider.IsAddressable(sceneGuid);

            SceneStatus status = SceneStatusRules.Decide(facts);
            SceneFix fix = SceneStatusRules.FixFor(status, ProjectDefaultPlace, addressablesInstalled);
            string message = SceneStatusRules.Message(status, sceneName, scenePath, addressablesInstalled);
            return new SceneStatusReport(status, fix, sceneName, scenePath, sceneGuid, message);
        }

        /// <summary>Applies the report's fix to the project, with one console line naming the scene.</summary>
        public static void ApplyFix(SceneStatusReport _report)
        {
            if (_report == null || _report.Fix == SceneFix.None)
            {
                return;
            }
            ScenePlace place = _report.Fix == SceneFix.MakeAddressable ? ScenePlace.Addressables : ScenePlace.BuildSettings;
            MoveTo(_report, place);
        }

        /// <summary>True when the scene can move to this place: it has an asset, is not there yet, and Addressables is installed if needed.</summary>
        public static bool CanMoveTo(SceneStatusReport _report, ScenePlace _place)
        {
            if (_report == null)
            {
                return false;
            }
            bool hasAsset = _report.Status != SceneStatus.Missing && _report.Status != SceneStatus.NoScene;
            if (!hasAsset)
            {
                return false;
            }
            if (_place == ScenePlace.Addressables)
            {
                return AddressableScenes.IsInstalled && _report.Status != SceneStatus.Addressable;
            }
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            bool alsoAddressable = provider != null && provider.IsAddressable(_report.SceneGuid);
            return _report.Status != SceneStatus.InBuild || alsoAddressable;
        }

        /// <summary>
        /// Moves the scene to Build Settings or Addressables and removes its entry from the other place, so it loads from
        /// one place. Does nothing when &lt;see cref="CanMoveTo"/&gt; says no.
        /// </summary>
        public static void MoveTo(SceneStatusReport _report, ScenePlace _place)
        {
            if (!CanMoveTo(_report, _place))
            {
                return;
            }
            if (_place == ScenePlace.Addressables)
            {
                MakeAddressable(_report);
                return;
            }
            AddToBuild(_report);
        }

        private static void AddToBuild(SceneStatusReport _report)
        {
            // Read before the change: Addressables may drop the entry itself once the scene is in Build Settings.
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            bool wasAddressable = provider != null && provider.IsAddressable(_report.SceneGuid);

            SceneBuildSettingsWriter.Write(SceneBuildSettingsList.WithSceneSwitchedOn(EditorBuildSettings.scenes, _report.ScenePath));

            string line = LogPrefix + " Added '" + _report.SceneName + "' to Build Settings.";
            if (wasAddressable)
            {
                bool stillAddressable = provider.IsAddressable(_report.SceneGuid);
                if (stillAddressable)
                {
                    provider.RemoveFromAddressables(_report.SceneGuid);
                }
                line += " Removed it from Addressables, so it loads from one place.";
            }
            Debug.Log(line);
        }

        private static void MakeAddressable(SceneStatusReport _report)
        {
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider == null)
            {
                Debug.LogError(LogPrefix + " Can't make '" + _report.SceneName + "' Addressable: the Addressables package is not installed. Use Add to build instead.");
                return;
            }

            bool made = provider.TryMakeAddressable(_report.SceneGuid, out string error);
            if (!made)
            {
                Debug.LogError(LogPrefix + " Can't make '" + _report.SceneName + "' Addressable: " + error);
                return;
            }

            string line = LogPrefix + " Made '" + _report.SceneName + "' Addressable.";
            bool wasListed = SceneBuildSettingsList.IsListed(EditorBuildSettings.scenes, _report.ScenePath);
            if (wasListed)
            {
                SceneBuildSettingsWriter.Write(SceneBuildSettingsList.WithoutScene(EditorBuildSettings.scenes, _report.ScenePath));
                line += " Removed it from Build Settings, so it loads from one place.";
            }
            Debug.Log(line);
        }

        // The asset reference wins; a field written by code may only hold the GUID or the path.
        private static SceneAsset ResolveSceneAsset(SceneAsset _sceneAsset, string _storedGuid, string _storedPath)
        {
            if (_sceneAsset != null)
            {
                return _sceneAsset;
            }
            if (!string.IsNullOrEmpty(_storedGuid))
            {
                string pathFromGuid = AssetDatabase.GUIDToAssetPath(_storedGuid);
                SceneAsset fromGuid = LoadSceneAsset(pathFromGuid);
                if (fromGuid != null)
                {
                    return fromGuid;
                }
            }
            return LoadSceneAsset(_storedPath);
        }

        private static SceneAsset LoadSceneAsset(string _path)
        {
            if (string.IsNullOrEmpty(_path))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(_path);
        }

        private static string StoredSceneName(string _storedName, string _storedPath)
        {
            if (!string.IsNullOrEmpty(_storedName))
            {
                return _storedName;
            }
            if (!string.IsNullOrEmpty(_storedPath))
            {
                return Path.GetFileNameWithoutExtension(_storedPath);
            }
            return string.Empty;
        }
    }
}
