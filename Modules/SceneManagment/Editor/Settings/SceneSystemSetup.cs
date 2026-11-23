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
    /// "Set up scene system": the settings asset, then the scene sets: the ones the designer listed on the Scene Sets
    /// step, or a starter "Default Scene Set" with one empty manager scene holding a copy of the basic fade (ADR 0004).
    /// Setup no longer sorts existing scene groups into scene sets (session 5A2): the Scene System page's "Preview
    /// migration / Migrate" does. Validated before it runs, and a failing step undoes everything made before it.
    /// </summary>
    public static class SceneSystemSetup
    {
        public const string DefaultFolder = "Assets/SceneSystem";
        public const string SettingsFileName = "SceneSystemSettings.asset";
        private const string ManagerSceneFileName = "Managers.unity";
        private const string FadeFileName = "Fade.asset";
        private const string FadeControllerFileName = "Fade Controller.prefab";
        private const string LoadingSceneFileName = "Loading Scene.unity";

        // The ready-made loading scene in SceneScreens (ADR 0004): cloned, never referenced.
        private const string LibraryLoadingSceneGuid = "ca40e8005259bd345b8ba56e2de6f32e";

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
        public static string BlockingProblem(SceneSystemSetupChoices _choices)
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
            bool makesManagerScene = MakesStarterManagers(_choices);
            bool makesLaunchScene = _choices.LaunchScene == LaunchSceneChoice.CreateNew;
            if ((makesManagerScene || makesLaunchScene) && HasUntitledOpenScene())
            {
                return "Save the open scene first.";
            }
            string setsProblem = SceneSetsProblem(_choices);
            if (!string.IsNullOrEmpty(setsProblem))
            {
                return setsProblem;
            }
            string launchProblem = LaunchSceneProblem(_choices);
            if (!string.IsNullOrEmpty(launchProblem))
            {
                return launchProblem;
            }
            return LoadingSceneProblem(_choices);
        }

        /// <summary>Why the scene sets listed on the Scene Sets step can't be made, naming the set; empty when they can.</summary>
        public static string SceneSetsProblem(SceneSystemSetupChoices _choices)
        {
            if (!_choices.SetUpSceneSetsNow)
            {
                return string.Empty;
            }
            if (_choices.SceneSets == null || _choices.SceneSets.Count == 0)
            {
                return "Add a scene set, or choose Not now.";
            }
            List<string> earlierNames = new List<string>();
            foreach (SceneSetDraft draft in _choices.SceneSets)
            {
                string problem = SceneSetNames.Problem(draft.Name, earlierNames);
                if (!string.IsNullOrEmpty(problem))
                {
                    return problem;
                }
                earlierNames.Add(draft.Name);
            }
            return string.Empty;
        }

        /// <summary>True when setup makes the starter Managers scene (with the copied fade).</summary>
        public static bool MakesStarterManagers(SceneSystemSetupChoices _choices)
        {
            if (!_choices.SetUpSceneSetsNow)
            {
                return true;
            }
            foreach (SceneSetDraft draft in _choices.SceneSets)
            {
                if (draft.IncludeStarterManagers)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>One line per scene set setup will make: "Default Scene Set (default): Managers (new, with a fade); shared: GameBoot".</summary>
        public static List<string> SceneSetLines(SceneSystemSetupChoices _choices, bool _hasFade)
        {
            List<string> lines = new List<string>();
            string starter = Path.GetFileNameWithoutExtension(ManagerSceneFileName) + (_hasFade ? " (new, with a fade)" : " (new)");
            if (!_choices.SetUpSceneSetsNow)
            {
                lines.Add("Scene set '" + SceneSetDraft.DefaultName + "' (default), manager scene " + starter);
                return lines;
            }
            for (int index = 0; index < _choices.SceneSets.Count; index++)
            {
                SceneSetDraft draft = _choices.SceneSets[index];
                List<string> managers = new List<string>();
                if (draft.IncludeStarterManagers)
                {
                    managers.Add(starter);
                }
                AddNames(managers, draft.ManagerScenes);
                List<string> shared = new List<string>();
                AddNames(shared, draft.SharedScenes);
                string line = "Scene set '" + draft.Name.Trim() + "'" + (index == 0 ? " (default)" : string.Empty);
                line += ", manager scenes: " + NamesOrNone(managers) + "; shared scenes: " + NamesOrNone(shared);
                if (draft.ResetSharedScenes)
                {
                    line += "; Reset shared scenes";
                }
                lines.Add(line);
            }
            return lines;
        }

        private static void AddNames(List<string> _names, List<SceneAsset> _scenes)
        {
            foreach (SceneAsset scene in _scenes)
            {
                if (scene != null && !_names.Contains(scene.name))
                {
                    _names.Add(scene.name);
                }
            }
        }

        private static string NamesOrNone(List<string> _names)
        {
            return _names.Count == 0 ? "none" : string.Join(", ", _names);
        }

        /// <summary>The one line Ready adds when the project has scene groups to sort into scene sets; empty otherwise.</summary>
        public static string MigrationHint(SceneGroupMigrationPlan _plan)
        {
            if (_plan == null || !_plan.HasWork)
            {
                return string.Empty;
            }
            return _plan.GroupsToMigrateCount + " scene groups can be sorted into scene sets later: Project Settings > Abstract Pixel > Scene System > Scene Groups Debugger, Preview migration.";
        }

        /// <summary>Why the loading scene step cannot go on, or empty.</summary>
        public static string LoadingSceneProblem(SceneSystemSetupChoices _choices)
        {
            if (_choices.LoadingScene == LoadingSceneChoice.Existing && _choices.ExistingLoadingScene == null)
            {
                return "Pick the loading scene, or choose another option.";
            }
            if (_choices.LoadingScene == LoadingSceneChoice.ReadyMade && !LibraryLoadingSceneExists())
            {
                return "The ready-made loading scene is not in this project. Pick a scene, or skip.";
            }
            return string.Empty;
        }

        /// <summary>"A copy of the ready-made loading scene" or "Loading scene 'Loading'", or empty when skipped.</summary>
        public static string LoadingSceneLine(SceneSystemSetupChoices _choices)
        {
            if (_choices.LoadingScene == LoadingSceneChoice.ReadyMade)
            {
                return "A copy of the ready-made loading scene, as the default loading scene";
            }
            if (_choices.LoadingScene == LoadingSceneChoice.Existing && _choices.ExistingLoadingScene != null)
            {
                return "Loading scene '" + _choices.ExistingLoadingScene.name + "', as the default loading scene";
            }
            return string.Empty;
        }

        /// <summary>The lines of "Setup will create", in the words the page uses.</summary>
        public static List<string> WillCreate(SceneSystemSetupChoices _choices, bool _hasFade, bool _addressablesInstalled)
        {
            string folder = NormalizeFolder(_choices.Folder);
            bool toAddressables = _choices.NewScenesAreAddressable && _addressablesInstalled;
            string place = toAddressables ? "Addressables" : "Build Settings";
            List<string> lines = new List<string>();
            lines.Add("The scene system settings asset: " + folder + "/" + SettingsFileName + ", added to Player Settings > Preloaded Assets.");
            foreach (string setLine in SceneSetLines(_choices, _hasFade))
            {
                lines.Add(setLine + ", in " + folder + ".");
            }
            if (MakesStarterManagers(_choices))
            {
                lines.Add("The manager scene '" + Path.GetFileNameWithoutExtension(ManagerSceneFileName) + "' is new and empty, in " + place + ".");
                if (_hasFade)
                {
                    lines.Add("A copy of the basic fade (" + FadeFileName + ") and its controller prefab (" + FadeControllerFileName + ") in that folder, played by the manager scene. Restyle the copies; the library's originals stay untouched.");
                }
                else
                {
                    lines.Add("No fade: the transitions module is not in this project, so scenes change without an effect.");
                }
            }
            if (_choices.LoadingScene == LoadingSceneChoice.Existing && _choices.ExistingLoadingScene != null)
            {
                lines.Add("Loading scene '" + _choices.ExistingLoadingScene.name + "' listed as the default loading scene, in " + place + ". Scene groups show it only when their Show the loading scene says so.");
            }
            if (_choices.LoadingScene == LoadingSceneChoice.ReadyMade && LibraryLoadingSceneExists())
            {
                lines.Add("A copy of the ready-made loading scene (" + LoadingSceneFileName + ") in that folder, in " + place + ", listed as the default loading scene. Scene groups show it only when their Show the loading scene says so.");
            }
            lines.Add("New scenes go to " + place + ". Check scene groups before a build: on.");
            string launchLine = LaunchSceneLine(_choices);
            if (!string.IsNullOrEmpty(launchLine))
            {
                lines.Add(launchLine + ".");
            }
            return lines;
        }

        public static bool LibraryLoadingSceneExists()
        {
            string path = AssetDatabase.GUIDToAssetPath(LibraryLoadingSceneGuid);
            return !string.IsNullOrEmpty(path) && AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        public static bool LibraryFadeExists()
        {
            string path = AssetDatabase.GUIDToAssetPath(LibraryFadeProfileGuid);
            return !string.IsNullOrEmpty(path) && AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        /// <summary>Runs the setup. Returns the new settings, or null with &lt;paramref name="_error"/&gt; and nothing left behind.</summary>
        public static SceneSystemSettings Run(SceneSystemSetupChoices _choices, out string _error)
        {
            _error = BlockingProblem(_choices);
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

                if (_choices.SetUpSceneSetsNow)
                {
                    CreateSceneSets(folder, settings, _choices.SceneSets, createdPaths);
                }
                else
                {
                    CreateStarterSet(folder, settings, createdPaths);
                }

                ApplyLoadingScene(_choices, folder, settings, createdPaths);
                ApplyLaunchScene(_choices, settings, createdPaths);
                SceneSystemSettingsAssets.RefreshLists(settings);
                SceneSystemSettingsAssets.EnsurePreloaded(settings);
                AssetDatabase.SaveAssets();
                Debug.Log(LogPrefix + " Set up the scene system in '" + folder + "': " + string.Join(" ", WillCreate(_choices, LibraryFadeExists(), AddressableScenes.IsInstalled)), settings);
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

        /// <summary>"Launch scene 'Splash' at build index 0", or empty when the step was skipped.</summary>
        public static string LaunchSceneLine(SceneSystemSetupChoices _choices)
        {
            if (_choices.LaunchScene == LaunchSceneChoice.Existing && _choices.ExistingLaunchScene != null)
            {
                return "Launch scene '" + _choices.ExistingLaunchScene.name + "' at build index 0";
            }
            if (_choices.LaunchScene == LaunchSceneChoice.CreateNew)
            {
                return "A new launch scene at " + _choices.NewLaunchScenePath + ", at build index 0";
            }
            return string.Empty;
        }

        /// <summary>Why the launch scene choice can't work, or empty. Nothing to check when the step was skipped.</summary>
        public static string LaunchSceneProblem(SceneSystemSetupChoices _choices)
        {
            if (_choices.LaunchScene == LaunchSceneChoice.Existing && _choices.ExistingLaunchScene == null)
            {
                return "Pick the scene to open first, or choose No launch scene.";
            }
            if (_choices.LaunchScene != LaunchSceneChoice.CreateNew)
            {
                return string.Empty;
            }
            string path = _choices.NewLaunchScenePath;
            bool isScenePath = !string.IsNullOrEmpty(path) && path.StartsWith("Assets/") && path.EndsWith(".unity");
            if (!isScenePath)
            {
                return "The new launch scene needs a path inside Assets/ ending in .unity.";
            }
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                return "A file already exists at " + path + ".";
            }
            return string.Empty;
        }

        // Sets the chosen launch scene, making it first when asked, and puts it at build index 0.
        private static void ApplyLaunchScene(SceneSystemSetupChoices _choices, SceneSystemSettings _settings, List<string> _createdPaths)
        {
            SceneAsset launchScene = null;
            if (_choices.LaunchScene == LaunchSceneChoice.Existing)
            {
                launchScene = _choices.ExistingLaunchScene;
            }
            if (_choices.LaunchScene == LaunchSceneChoice.CreateNew)
            {
                string folder = System.IO.Path.GetDirectoryName(_choices.NewLaunchScenePath).Replace('\\', '/');
                CreateFolders(folder, _createdPaths);
                launchScene = LaunchSceneCreation.Create(_choices.NewLaunchScenePath, out string error);
                if (launchScene == null)
                {
                    throw new InvalidOperationException(error);
                }
                _createdPaths.Add(_choices.NewLaunchScenePath);
            }
            if (launchScene == null)
            {
                return;
            }
            _settings.LaunchScene.SceneFieldReference.SetSceneAsset(launchScene);
            EditorUtility.SetDirty(_settings);
            LaunchSceneBuildOrder.EnsureFirst(_settings);
        }

        private static void CreateStarterSet(string _folder, SceneSystemSettings _settings, List<string> _createdPaths)
        {
            SceneSetDraft starter = SceneSetDraft.Default();
            CreateSceneSets(_folder, _settings, new List<SceneSetDraft> { starter }, _createdPaths);
        }

        // One scene set per draft, the first being the default. The starter Managers scene is made once, for the sets
        // that list it; picked scenes go where new scenes go when they are in neither Build Settings nor Addressables.
        private static void CreateSceneSets(string _folder, SceneSystemSettings _settings, List<SceneSetDraft> _drafts, List<string> _createdPaths)
        {
            SceneAsset starterManagers = null;
            for (int index = 0; index < _drafts.Count; index++)
            {
                SceneSetDraft draft = _drafts[index];
                SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
                if (draft.IncludeStarterManagers)
                {
                    if (starterManagers == null)
                    {
                        starterManagers = CreateStarterManagers(_folder, _createdPaths);
                    }
                    set.ManagerScenes.Add(ReferenceTo(starterManagers));
                }
                AddScenes(set.ManagerScenes, draft.ManagerScenes);
                AddScenes(set.SharedScenes, draft.SharedScenes);
                set.ResetSharedScenes = draft.ResetSharedScenes;

                string setPath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + draft.Name.Trim() + ".asset");
                AssetDatabase.CreateAsset(set, setPath);
                _createdPaths.Add(setPath);
                _settings.SceneSets.Add(set);
                if (index == 0)
                {
                    _settings.DefaultSceneSet = set;
                }
                MoveToDefaultPlace(set.ManagerScenes);
                MoveToDefaultPlace(set.SharedScenes);
            }
            EditorUtility.SetDirty(_settings);
        }

        // The starter manager scene: empty, holding a copy of the basic fade when the transitions module is here.
        private static SceneAsset CreateStarterManagers(string _folder, List<string> _createdPaths)
        {
            ScriptableObject fadeCopy = CopyLibraryFade(_folder, _createdPaths);
            string scenePath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + ManagerSceneFileName);
            CreateManagerScene(scenePath, fadeCopy);
            _createdPaths.Add(scenePath);
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }

        private static void AddScenes(List<SceneReference> _list, List<SceneAsset> _scenes)
        {
            foreach (SceneAsset scene in _scenes)
            {
                if (scene == null)
                {
                    continue;
                }
                SceneReference reference = ReferenceTo(scene);
                bool alreadyListed = _list.Exists(_existing => _existing.SceneGuid == reference.SceneGuid);
                if (!alreadyListed)
                {
                    _list.Add(reference);
                }
            }
        }

        private static SceneReference ReferenceTo(SceneAsset _scene)
        {
            string path = AssetDatabase.GetAssetPath(_scene);
            SceneReference reference = new SceneReference(_scene.name, AssetDatabase.AssetPathToGUID(path), path);
            reference.SceneFieldReference.SetSceneAsset(_scene);
            return reference;
        }

        private static void MoveToDefaultPlace(List<SceneReference> _scenes)
        {
            foreach (SceneReference scene in _scenes)
            {
                SceneStatusReport report = SceneStatusReader.Read(scene.SceneFieldReference);
                if (report.Status == SceneStatus.NotInBuild)
                {
                    SceneStatusReader.MoveTo(report, SceneStatusReader.ProjectDefaultPlace);
                }
            }
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
            ScriptableObject fadeCopy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(copyPath);
            CopyFadeController(_folder, fadeCopy, _createdPaths);
            return fadeCopy;
        }

        // The copied profile still names the library's controller prefab; it gets its own copy, so nothing the project
        // uses points into the library (ADR 0004). The field is found by name: the scene module never references transitions.
        private static void CopyFadeController(string _folder, ScriptableObject _fadeCopy, List<string> _createdPaths)
        {
            SerializedObject profile = new SerializedObject(_fadeCopy);
            SerializedProperty controllerProperty = profile.FindProperty("TransitionControllerPrefab");
            if (controllerProperty == null || controllerProperty.objectReferenceValue == null)
            {
                return;
            }
            string originalPath = AssetDatabase.GetAssetPath(controllerProperty.objectReferenceValue);
            string copyPath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + FadeControllerFileName);
            if (!AssetDatabase.CopyAsset(originalPath, copyPath))
            {
                throw new InvalidOperationException("Unity could not copy the fade's controller prefab to '" + copyPath + "'.");
            }
            _createdPaths.Add(copyPath);
            controllerProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(copyPath);
            profile.ApplyModifiedPropertiesWithoutUndo();
        }

        // A copy of the ready-made loading scene, listed in the settings as the default loading scene and put where new
        // scenes go. Scene groups show it only when they ask to.
        // The loading scene step: a copy of the ready-made scene, the designer's own scene, or nothing.
        private static void ApplyLoadingScene(SceneSystemSetupChoices _choices, string _folder, SceneSystemSettings _settings, List<string> _createdPaths)
        {
            if (_choices.LoadingScene == LoadingSceneChoice.ReadyMade)
            {
                CopyLibraryLoadingScene(_folder, _settings, _createdPaths);
                return;
            }
            if (_choices.LoadingScene == LoadingSceneChoice.Existing && _choices.ExistingLoadingScene != null)
            {
                string path = AssetDatabase.GetAssetPath(_choices.ExistingLoadingScene);
                ListAsDefaultLoadingScene(_choices.ExistingLoadingScene, path, _settings);
            }
        }

        private static void CopyLibraryLoadingScene(string _folder, SceneSystemSettings _settings, List<string> _createdPaths)
        {
            string originalPath = AssetDatabase.GUIDToAssetPath(LibraryLoadingSceneGuid);
            if (string.IsNullOrEmpty(originalPath) || AssetDatabase.LoadMainAssetAtPath(originalPath) == null)
            {
                Debug.LogWarning(LogPrefix + " The ready-made loading scene is not in this project, so no loading scene was added.");
                return;
            }
            string copyPath = AssetDatabase.GenerateUniqueAssetPath(_folder + "/" + LoadingSceneFileName);
            if (!AssetDatabase.CopyAsset(originalPath, copyPath))
            {
                throw new InvalidOperationException("Unity could not copy the ready-made loading scene to '" + copyPath + "'.");
            }
            _createdPaths.Add(copyPath);

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(copyPath);
            ListAsDefaultLoadingScene(sceneAsset, copyPath, _settings);
        }

        // Lists the scene as a loading scene and the default one, and puts it where new scenes go so it loads in a build.
        private static void ListAsDefaultLoadingScene(SceneAsset _scene, string _path, SceneSystemSettings _settings)
        {
            SceneReference loadingScene = new SceneReference(_scene.name, AssetDatabase.AssetPathToGUID(_path), _path);
            loadingScene.SceneFieldReference.SetSceneAsset(_scene);
            _settings.LoadingScenes.Add(loadingScene);
            _settings.DefaultLoadingScene = new SceneReference(_scene.name, loadingScene.SceneGuid, _path);
            _settings.DefaultLoadingScene.SceneFieldReference.SetSceneAsset(_scene);
            EditorUtility.SetDirty(_settings);

            SceneStatusReport report = SceneStatusReader.Read(loadingScene.SceneFieldReference);
            SceneStatusReader.MoveTo(report, SceneStatusReader.ProjectDefaultPlace);
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
