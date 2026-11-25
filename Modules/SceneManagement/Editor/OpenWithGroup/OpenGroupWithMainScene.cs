using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The scene opening preferences at work: when a scene is opened on its own (Single mode), the scenes Play would
    /// load with it open next to it, so the Hierarchy shows what Play loads. Which preference applies depends on the
    /// scene (<see cref="GroupScenesToOpen"/>); when it is off, the tool does nothing for that scene. The opened scene stays active. Switching to
    /// another group closes the scenes this tool opened that the new group does not need (a scene with unsaved changes
    /// gets Unity's save prompt, and Cancel keeps it); a scene opened by hand is never closed.
    /// It does nothing in Play mode, during a build, in prefab mode, while tests run, while "Open group" runs, and while
    /// Unity restores scenes (Editor start, domain reload, leaving Play mode). It never marks a scene dirty and never
    /// changes Build Settings. Preferences &gt; Abstract Pixel &gt; Scenes turns each case off.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenGroupWithMainScene
    {
        private const string LogPrefix = "[Scene system] ";
        private const int IdleTicksAfterRestore = 2;

        private static bool isRestoringScenes;

        static OpenGroupWithMainScene()
        {
            // Unity restores the scenes after a domain load (Editor start, script reload); that is not a scene the user opened.
            BeginRestoring();
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _change)
        {
            if (_change == PlayModeStateChange.ExitingPlayMode)
            {
                isRestoringScenes = true;
                return;
            }
            if (_change == PlayModeStateChange.EnteredEditMode)
            {
                BeginRestoring();
            }
        }

        // Ends after a few idle ticks, once the scenes Unity restores have opened.
        private static void BeginRestoring()
        {
            isRestoringScenes = true;
            EndRestoringAfterIdleTicks(IdleTicksAfterRestore);
        }

        private static void EndRestoringAfterIdleTicks(int _ticksLeft)
        {
            EditorIdleQueue.Run(() =>
            {
                if (_ticksLeft > 1)
                {
                    EndRestoringAfterIdleTicks(_ticksLeft - 1);
                    return;
                }
                isRestoringScenes = false;
            });
        }

        private static void OnSceneOpened(Scene _scene, OpenSceneMode _mode)
        {
            if (_mode != OpenSceneMode.Single)
            {
                return;
            }
            if (!CanReact())
            {
                return;
            }
            string scenePath = _scene.path;
            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }
            EditorIdleQueue.Run(() => OpenGroupFor(scenePath));
        }

        private static bool CanReact()
        {
            if (isRestoringScenes || SceneGroupOpener.IsOpening)
            {
                return false;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer || Application.isBatchMode)
            {
                return false;
            }
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return false;
            }
            return !TestRunDetector.TestsAreRunning();
        }

        private static void OpenGroupFor(string _openedScenePath)
        {
            // Checked again: Play, a build or a test run may have started since the scene opened.
            if (!CanReact())
            {
                return;
            }
            Scene openedScene = EditorSceneManager.GetSceneByPath(_openedScenePath);
            if (!openedScene.IsValid() || !openedScene.isLoaded)
            {
                return;
            }
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings == null)
            {
                return;
            }

            SceneReference openedReference = ToReference(openedScene);
            SceneOpeningChoices choices = SceneOpeningPreferences.Choices;
            SceneOpeningPreference preference = GroupScenesToOpen.PreferenceFor(settings, openedReference);
            if (!choices.Allows(preference))
            {
                // The preference for this scene is off: open nothing and close nothing.
                return;
            }
            List<SceneReference> needed = GroupScenesToOpen.For(settings, openedReference, choices);
            List<SceneReference> open = OpenScenes();
            OpenedGroupScenesPlan plan = OpenedGroupScenesPlan.Compute(needed, open, OpenedGroupScenesRecord.Read());

            List<SceneReference> openedByTool = new List<SceneReference>(plan.OpenedByToolAfter);
            CloseScenesTheGroupDoesNotNeed(plan.ToClose, openedByTool);
            List<string> openedNames = OpenNeededScenes(plan.ToOpen, openedByTool);
            EditorSceneManager.SetActiveScene(openedScene);
            OpenedGroupScenesRecord.Write(openedByTool);

            if (openedNames.Count > 0)
            {
                Debug.Log(LogPrefix + "Opened what Play loads with '" + openedScene.name + "': " + string.Join(", ", openedNames) + ". Preferences > Abstract Pixel > Scenes turns this off.");
            }
        }

        // A scene with unsaved changes gets Unity's save prompt first; Cancel keeps it open, and it stays on record.
        private static void CloseScenesTheGroupDoesNotNeed(List<SceneReference> _toClose, List<SceneReference> _openedByTool)
        {
            foreach (SceneReference sceneToClose in _toClose)
            {
                Scene scene = EditorSceneManager.GetSceneByPath(sceneToClose.ScenePath);
                if (!scene.IsValid())
                {
                    continue;
                }
                if (scene.isDirty)
                {
                    bool userKeptGoing = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[] { scene });
                    if (!userKeptGoing)
                    {
                        _openedByTool.Add(sceneToClose);
                        continue;
                    }
                }
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static List<string> OpenNeededScenes(List<SceneReference> _toOpen, List<SceneReference> _openedByTool)
        {
            List<string> openedNames = new List<string>();
            foreach (SceneReference sceneToOpen in _toOpen)
            {
                string path = sceneToOpen.ScenePath;
                bool sceneExists = !string.IsNullOrEmpty(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
                if (!sceneExists)
                {
                    Debug.LogWarning(LogPrefix + "Could not open '" + sceneToOpen.SceneName + "' next to the opened scene: the scene asset is missing. Check the scene group or scene set on the Scene System page.");
                    _openedByTool.Remove(sceneToOpen);
                    continue;
                }
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                openedNames.Add(sceneToOpen.SceneName);
            }
            return openedNames;
        }

        private static List<SceneReference> OpenScenes()
        {
            List<SceneReference> scenes = new List<SceneReference>();
            for (int index = 0; index < EditorSceneManager.sceneCount; index++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(index);
                if (scene.isLoaded)
                {
                    scenes.Add(ToReference(scene));
                }
            }
            return scenes;
        }

        private static SceneReference ToReference(Scene _scene)
        {
            string guid = AssetDatabase.AssetPathToGUID(_scene.path);
            return new SceneReference(_scene.name, guid, _scene.path);
        }
    }
}
