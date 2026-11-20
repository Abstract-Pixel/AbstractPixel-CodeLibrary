using System;
using System.Collections.Generic;
using System.IO;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// "Set up scene system": the settings asset, then either the migration of the project's scene groups or a starter
    /// scene set with one empty manager scene holding a copy of the basic fade (ADR 0004). Validated before it runs, and
    /// a failing step undoes everything made before it.
    /// </summary>
    public static class SceneSystemSetup
    {
        public const string DefaultFolder = "Assets/SceneSystem";
        public const string SettingsFileName = "SceneSystemSettings.asset";
        private const string StarterSetFileName = "Default Scene Set.asset";
        private const string ManagerSceneFileName = "Managers.unity";
        private const string FadeFileName = "Fade.asset";

        // The library's basic fade profile and the component that plays it, found by GUID and name: the scene module
        // never references the transitions module (ADR 0001), and works without it.
        private const string LibraryFadeProfileGuid = "71228699d5d42f64da0bffcc76a723c9";
        private const string InitializeTransitionsTypeName = "AbstractPixel.SceneTransitions.InitializeTransitions";
        private const string LogPrefix = "[Scene system]";
        private static readonly char[] InvalidNameCharacters = { '<', '>', ':', '"', '|', '?', '*', '\\' };

        public static string NormalizeFolder(string _folder)
        {
            if (_folder == null)
            {
                return string.Empty;
            }
            string normalized = _folder.Trim().Replace('\\', '/');
            while (normalized.Length > 1 && normalized.EndsWith("/"))
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }
            return normalized;
        }

        /// <summary>Why this folder can't hold the settings asset, in one sentence; empty when it can.</summary>
        public static string FolderProblem(string _folder)
        {
            string folder = NormalizeFolder(_folder);
            if (string.IsNullOrEmpty(folder))
            {
                return "Pick a folder.";
            }
            bool insideAssets = folder == "Assets" || folder.StartsWith("Assets/", StringComparison.Ordinal);
            if (!insideAssets)
            {
                return "Must be inside Assets/.";
            }

            string[] parts = folder.Split('/');
            for (int index = 1; index < parts.Length; index++)
            {
                string part = parts[index];
                if (part.Length == 0)
                {
                    return "Has an empty folder name ('//').";
                }
                if (part == "." || part == "..")
                {
                    return "Can't use '.' or '..'.";
                }
                if (part.IndexOfAny(InvalidNameCharacters) >= 0)
                {
                    return "'" + part + "' has a character folders can't use.";
                }
                if (part.EndsWith(".") || part.EndsWith(" "))
                {
                    return "'" + part + "' can't end with a dot or space.";
                }
                if (string.Equals(part, "Editor", StringComparison.OrdinalIgnoreCase))
                {
                    return "Can't be inside an Editor folder (builds leave it out).";
                }
            }
            return string.Empty;
        }

        /// <summary>The folder problem, plus the checks that need the project: a file in the way.</summary>
        public static string FolderProblemInProject(string _folder)
        {
            string problem = FolderProblem(_folder);
            if (!string.IsNullOrEmpty(problem))
            {
                return problem;
            }
            string folder = NormalizeFolder(_folder);
            if (File.Exists(folder))
            {
                return "That's a file, not a folder.";
            }
            return string.Empty;
        }

        /// <summary>Why setup can't run with these choices right now; empty when it can.</summary>
        public static string BlockingProblem(SceneSystemSetupChoices _choices, SceneGroupMigrationPlan _plan)
        {
            string folderProblem = FolderProblemInProject(_choices.Folder);
            if (!string.IsNullOrEmpty(folderProblem))
            {
                return folderProblem;
            }
            if (SceneSystemSettingsAssets.Find() != null)
            {
                return "This project is already set up.";
            }
            bool makesStarterSet = !WillMigrate(_choices, _plan);
            if (makesStarterSet && HasUntitledOpenScene())
            {
                return "Save the open scene first.";
            }
            return string.Empty;
        }

        public static bool WillMigrate(SceneSystemSetupChoices _choices, SceneGroupMigrationPlan _plan)
        {
            return _choices.MigrateGroups && _plan != null && _plan.HasWork;
        }

        /// <summary>The lines of "Setup will create", in the words the page uses.</summary>
        public static List<string> WillCreate(SceneSystemSetupChoices _choices, SceneGroupMigrationPlan _plan, bool _hasFade, bool _addressablesInstalled)
        {
            string folder = NormalizeFolder(_choices.Folder);
            bool toAddressables = _choices.NewScenesAreAddressable && _addressablesInstalled;
            string place = toAddressables ? "Addressables" : "Build Settings";
            List<string> lines = new List<string>();
            lines.Add("The scene system settings asset: " + folder + "/" + SettingsFileName + ", added to Player Settings > Preloaded Assets.");
            if (WillMigrate(_choices, _plan))
            {
                lines.Add(_plan.SetNames.Count + " scene set(s) (" + string.Join(", ", _plan.SetNames) + ") in " + folder + "/Scene Sets, filled from " + _plan.GroupsToMigrateCount + " scene group(s); '" + _plan.SetNames[_plan.DefaultSetIndex] + "' is the default.");
                lines.Add("Each of those scene groups uses its scene set with no overrides and keeps every scene. A report next to the settings asset lists every group before and after.");
                if (_plan.GroupsWithoutMainScene.Count > 0)
                {
                    lines.Add(_plan.GroupsWithoutMainScene.Count + " scene group(s) without a main scene stay as they are and are listed in the report as unused.");
                }
            }
            else
            {
                lines.Add("A starter scene set '" + Path.GetFileNameWithoutExtension(StarterSetFileName) + "', the default, with one empty manager scene '" + Path.GetFileNameWithoutExtension(ManagerSceneFileName) + "' in " + place + ".");
                if (_hasFade)
                {
                    lines.Add("A copy of the basic fade (" + FadeFileName + ") in that folder, played by the manager scene. Restyle the copy; the library's original stays untouched.");
                }
                else
                {
                    lines.Add("No fade: the transitions module is not in this project, so scenes change without an effect.");
                }
            }
            lines.Add("New scenes go to " + place + ". Check scene groups before a build: on.");
            return lines;
        }

        public static bool LibraryFadeExists()
        {
            string path = AssetDatabase.GUIDToAssetPath(LibraryFadeProfileGuid);
            return !string.IsNullOrEmpty(path) && AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        /// <summary>Runs the setup. Returns the new settings, or null with &lt;paramref name="_error"/&gt; and nothing left behind.</summary>
        public static SceneSystemSettings Run(SceneSystemSetupChoices _choices, out string _error)
        {
            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(SceneGroupAssets.FindAll());
            _error = BlockingProblem(_choices, plan);
            if (!string.IsNullOrEmpty(_error))
            {
                return null;
            }

            string folder = NormalizeFolder(_choices.Folder);
            List<string> createdPaths = new List<string>();
            List<SceneGroup> changedGroups = new List<SceneGroup>();
            UnityEngine.Object[] preloadedBefore = PlayerSettings.GetPreloadedAssets();
            EditorBuildSettingsScene[] buildScenesBefore = EditorBuildSettings.scenes;
            try
            {
                CreateFolders(folder, createdPaths);
                SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
                settings.NewScenesAreAddressable = _choices.NewScenesAreAddressable;
                settings.CheckSceneGroupsBeforeBuild = true;
                string settingsPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + SettingsFileName);
                AssetDatabase.CreateAsset(settings, settingsPath);
                createdPaths.Add(settingsPath);
                SceneSystemSettingsAssets.Remember(settings);
                SceneSystemSettingsAssets.PushToCore();

                if (WillMigrate(_choices, plan))
                {
                    string reportPath = SceneGroupMigration.Run(settings, false, createdPaths, changedGroups);
                    createdPaths.Add(reportPath);
                }
                else
                {
                    CreateStarterSet(folder, settings, createdPaths);
                }

                SceneSystemSettingsAssets.RefreshLists(settings);
                SceneSystemSettingsAssets.EnsurePreloaded(settings);
                AssetDatabase.SaveAssets();
                Debug.Log(LogPrefix + " Set up the scene system in '" + folder + "': " + string.Join(" ", WillCreate(_choices, plan, LibraryFadeExists(), AddressableScenes.IsInstalled)), settings);
                SceneSystemSettingsAssets.NotifyChanged();
                return settings;
            }
            catch (Exception exception)
            {
                Undo(createdPaths, changedGroups, preloadedBefore, buildScenesBefore);
                _error = "Setup stopped and removed what it had made: " + exception.Message;
                Debug.LogError(LogPrefix + " " + _error + "\n" + exception);
                return null;
            }
        }

        private static void CreateStarterSet(string _folder, SceneSystemSettings _settings, List<string> _createdPaths)
        {
            ScriptableObject fadeCopy = CopyLibraryFade(_folder, _createdPaths);
            string scenePath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + ManagerSceneFileName);
            CreateManagerScene(scenePath, fadeCopy);
            _createdPaths.Add(scenePath);

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            SceneReference managerScene = new SceneReference(sceneAsset.name, AssetDatabase.AssetPathToGUID(scenePath), scenePath);
            managerScene.SceneFieldReference.SetSceneAsset(sceneAsset);

            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            set.ManagerScenes.Add(managerScene);
            string setPath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + StarterSetFileName);
            AssetDatabase.CreateAsset(set, setPath);
            _createdPaths.Add(setPath);

            _settings.SceneSets.Add(set);
            _settings.DefaultSceneSet = set;
            EditorUtility.SetDirty(_settings);

            SceneStatusReport report = SceneStatusReader.Read(managerScene.SceneFieldReference);
            SceneStatusReader.MoveTo(report, SceneStatusReader.ProjectDefaultPlace);
        }

        private static ScriptableObject CopyLibraryFade(string _folder, List<string> _createdPaths)
        {
            string originalPath = AssetDatabase.GUIDToAssetPath(LibraryFadeProfileGuid);
            if (string.IsNullOrEmpty(originalPath) || AssetDatabase.LoadMainAssetAtPath(originalPath) == null)
            {
                Debug.LogWarning(LogPrefix + " The transitions module's basic fade is not in this project, so the starter manager scene has no fade.");
                return null;
            }
            string copyPath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + FadeFileName);
            bool copied = AssetDatabase.CopyAsset(originalPath, copyPath);
            if (!copied)
            {
                throw new InvalidOperationException("Unity could not copy the basic fade to '" + copyPath + "'.");
            }
            _createdPaths.Add(copyPath);
            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(copyPath);
        }

        // An empty scene opened next to the current ones, so the designer's open scenes stay as they are.
        private static void CreateManagerScene(string _scenePath, ScriptableObject _fadeCopy)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                if (_fadeCopy != null)
                {
                    AddFade(scene, _fadeCopy);
                }
                bool saved = EditorSceneManager.SaveScene(scene, _scenePath);
                if (!saved)
                {
                    throw new InvalidOperationException("Unity could not save the manager scene to '" + _scenePath + "'.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void AddFade(Scene _scene, ScriptableObject _fadeCopy)
        {
            Type componentType = FindComponentType(InitializeTransitionsTypeName);
            if (componentType == null)
            {
                Debug.LogWarning(LogPrefix + " Found the fade but not '" + InitializeTransitionsTypeName + "', so the manager scene has no fade.");
                return;
            }
            GameObject transitions = new GameObject("Transitions");
            SceneManager.MoveGameObjectToScene(transitions, _scene);
            Component component = transitions.AddComponent(componentType);
            SerializedObject serializedComponent = new SerializedObject(component);
            SerializedProperty profileProperty = serializedComponent.FindProperty("transitionProfile");
            if (profileProperty == null)
            {
                throw new InvalidOperationException("'" + InitializeTransitionsTypeName + "' has no 'transitionProfile' field to point at the fade.");
            }
            profileProperty.objectReferenceValue = _fadeCopy;
            serializedComponent.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Type FindComponentType(string _fullName)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            {
                if (type.FullName == _fullName)
                {
                    return type;
                }
            }
            return null;
        }

        private static void CreateFolders(string _folder, List<string> _createdPaths)
        {
            string[] parts = _folder.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, parts[index]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        throw new InvalidOperationException("Unity could not create the folder '" + next + "'.");
                    }
                    _createdPaths.Add(next);
                }
                current = next;
            }
        }

        private static bool HasUntitledOpenScene()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (string.IsNullOrEmpty(scene.path))
                {
                    return true;
                }
            }
            return false;
        }

        private static void Undo(List<string> _createdPaths, List<SceneGroup> _changedGroups, UnityEngine.Object[] _preloadedBefore, EditorBuildSettingsScene[] _buildScenesBefore)
        {
            foreach (SceneGroup group in _changedGroups)
            {
                if (group == null)
                {
                    continue;
                }
                group.SceneSet = null;
                EditorUtility.SetDirty(group);
            }
            for (int index = _createdPaths.Count - 1; index >= 0; index--)
            {
                AssetDatabase.DeleteAsset(_createdPaths[index]);
            }
            PlayerSettings.SetPreloadedAssets(_preloadedBefore);
            EditorBuildSettings.scenes = _buildScenesBefore;
            SceneSystemSettingsAssets.CachedGuid = string.Empty;
            AssetDatabase.SaveAssets();
            SceneSystemSettingsAssets.NotifyChanged();
        }
    }
}
