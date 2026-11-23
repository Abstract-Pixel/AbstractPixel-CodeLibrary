using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Start-up: the first scene change of a session, run by the scene system itself when the game launches or Play is
    /// pressed. It reads the scene system settings, picks a scene group from the scenes Unity opened
    /// (<see cref="StartUpRule"/>), creates the coordinator and has it open the group's missing scenes. Scene changes
    /// asked for before that are refused. State resets through <see cref="StaticsResetter"/>, so it runs on every Play
    /// with domain reload off.
    /// </summary>
    public static class SceneSystemStartUp
    {
        /// <summary>True from before the first scene loads until start-up has handed over to the coordinator.</summary>
        internal static bool IsPending { get; private set; }

        /// <summary>True when start-up could not open its scene group, or had nothing it could do.</summary>
        public static bool HasFailed { get; private set; }

        static SceneSystemStartUp()
        {
            StaticsResetter.OnResetStatics += ResetState;
        }

        /// <summary>
        /// Set by the scene system's editor code: whether a scene (by path) was loaded in edit mode when Play was pressed.
        /// Unity 7 lists those scenes as still loading when start-up looks; null in a build.
        /// </summary>
        internal static System.Func<string, bool> WasLoadedInEditMode;

        private const float EditModeScenesWaitSeconds = 10f;

        /// <summary>True when the launch scene hold was decided before the first scene's Awake (the settings were found then).</summary>
        private static bool decidedLaunchSceneHold;

        private static void ResetState()
        {
            IsPending = false;
            HasFailed = false;
            decidedLaunchSceneHold = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HoldSceneChangesUntilStartUp()
        {
            ResetState();
            IsPending = true;
            HoldLaunchSceneIfOpening();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RunAfterFirstScenesLoaded()
        {
            if (decidedLaunchSceneHold == false)
            {
                HoldLaunchSceneIfOpening();
            }
            RunAsync().ForgetTask();
        }

        // The first scenes are known at BeforeSceneLoad: their objects are in memory, but no Awake has run. So the hold is
        // in place before any scene's Awake, and only when the launch scene is one of them. Settings not found yet (none
        // in the project): decided again after the first scenes load, before any Start.
        private static void HoldLaunchSceneIfOpening()
        {
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            if (settings == null)
            {
                return;
            }

            List<SceneReference> openingScenes = ScenesUnityIsOpening();
            bool shouldHold = StartUpRule.ShouldHoldLaunchScene(settings, openingScenes);
            if (shouldHold == false)
            {
                // Unity 7 lists the scene Play opens as not loaded yet at BeforeSceneLoad: decide again after it loads.
                decidedLaunchSceneHold = AnySceneMayStillBeOpening() == false;
                return;
            }
            decidedLaunchSceneHold = true;

            LaunchSceneHold.Hold();
            Debug.Log("[Scene system] Start-up: the launch scene '" + HeldLaunchSceneName(settings, openingScenes) + "' holds the start scene group until it calls SceneActions.ReleaseLaunchScene().");
        }

        // The project launch scene's name, or the sample launch scene's when only that one is opening.
        private static string HeldLaunchSceneName(SceneSystemSettings _settings, List<SceneReference> _openingScenes)
        {
            bool projectLaunchSceneIsOpening = _settings.HasLaunchScene && StartUpSceneMatch.ListHolds(_openingScenes, _settings.LaunchScene);
            if (projectLaunchSceneIsOpening)
            {
                return _settings.LaunchScene.SceneName;
            }
            SceneGroup sampleGroup = StartUpRule.FindSampleGroupWithOpenLaunchScene(_settings, _openingScenes);
            if (sampleGroup == null)
            {
                return string.Empty;
            }
            return sampleGroup.SampleLaunchScene.SceneName;
        }

        private static async Task RunAsync()
        {
            await WaitForEditModeScenes();
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            List<SceneReference> openScenes = OpenScenesActiveFirst();
            StartUpChoice choice = StartUpRule.Choose(settings, openScenes);
            LogChoice(choice);

            if (choice.Kind == StartUpChoiceKind.DoNothing)
            {
                IsPending = false;
                HasFailed = true;
                // Relocatables that woke in the first scenes wait for start-up. With no start-up they are relocated
                // on what is open now, and the ones with no position stay held with an error.
                await RelocationBroadcasterSlot.Relocate();
                RelocationBroadcasterSlot.Release();
                return;
            }

            SceneGroup group = choice.Group;
            if (choice.Kind == StartUpChoiceKind.TemporaryGroup)
            {
                group = TemporarySceneGroup.Create(choice.MainScene, choice.SceneSet);
            }
            if (choice.Kind != StartUpChoiceKind.BehindLaunchScene)
            {
                MakeActiveIfOpen(group.MainScene);
            }

            SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
            HashSet<SceneReference> scenesAlreadyOpen = new HashSet<SceneReference>(openScenes);
            bool waitOneFrame = choice.Kind == StartUpChoiceKind.BehindLaunchScene;

            // The coordinator refuses other scene changes from here on, while start-up runs.
            IsPending = false;
            bool started = await coordinator.RunStartUp(group, scenesAlreadyOpen, waitOneFrame);
            if (!started)
            {
                HasFailed = true;
                Debug.LogError("[Scene system] Start-up could not open '" + group.name + "'. The game has not started.");
            }
        }

        private static void LogChoice(StartUpChoice _choice)
        {
            if (!string.IsNullOrEmpty(_choice.ErrorMessage))
            {
                Debug.LogError(_choice.ErrorMessage);
            }
            if (!string.IsNullOrEmpty(_choice.WarningMessage))
            {
                Debug.LogWarning(_choice.WarningMessage);
            }
            if (!string.IsNullOrEmpty(_choice.InfoMessage))
            {
                Debug.Log(_choice.InfoMessage);
            }
        }

        // The scenes Unity has loaded, the active scene first. In the Editor each one carries its asset GUID; a build
        // knows only path and name.
        private static List<SceneReference> OpenScenesActiveFirst()
        {
            List<SceneReference> scenes = new List<SceneReference>();
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                scenes.Add(ToReference(activeScene));
            }
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                bool isActive = scene == activeScene;
                if (isActive || !scene.isLoaded)
                {
                    continue;
                }
                scenes.Add(ToReference(scene));
            }
            return scenes;
        }

        // The scenes Unity is opening, for BeforeSceneLoad: a player's first scene is listed but not loaded yet then.
        private static List<SceneReference> ScenesUnityIsOpening()
        {
            List<SceneReference> scenes = new List<SceneReference>();
            bool isEditor = Application.isEditor;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                bool opening = StartUpRule.CountsAsOpening(scene.IsValid(), scene.isLoaded, isEditor, LoadedInEditMode(scene));
                if (opening)
                {
                    scenes.Add(ToReference(scene));
                }
            }
            return scenes;
        }

        private static bool LoadedInEditMode(Scene _scene)
        {
            if (WasLoadedInEditMode == null || string.IsNullOrEmpty(_scene.path))
            {
                return false;
            }
            return WasLoadedInEditMode(_scene.path);
        }

        // Unity 7 is still loading the scenes that were open in edit mode when start-up begins. Wait for them, so they
        // count as already open and are not loaded a second time. A scene left unloaded in the Hierarchy is not waited for.
        private static async Task WaitForEditModeScenes()
        {
            double endTime = Time.realtimeSinceStartupAsDouble + EditModeScenesWaitSeconds;
            while (AnyEditModeSceneStillLoading() && Time.realtimeSinceStartupAsDouble < endTime)
            {
                await Task.Yield();
            }
        }

        private static bool AnyEditModeSceneStillLoading()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                bool stillLoading = scene.IsValid() && scene.isLoaded == false && LoadedInEditMode(scene);
                if (stillLoading)
                {
                    return true;
                }
            }
            return false;
        }

        // True when a listed scene is not loaded yet in the Editor, so it may still be one that Play opens.
        private static bool AnySceneMayStillBeOpening()
        {
            bool isEditor = Application.isEditor;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (StartUpRule.MayStillBeOpening(scene.IsValid(), scene.isLoaded, isEditor))
                {
                    return true;
                }
            }
            return false;
        }

        private static SceneReference ToReference(Scene _scene)
        {
            string guid = string.Empty;
#if UNITY_EDITOR
            guid = UnityEditor.AssetDatabase.AssetPathToGUID(_scene.path);
#endif
            return new SceneReference(_scene.name, guid, _scene.path);
        }

        // Play pressed with the main scene open but another scene active: the main scene becomes active before any
        // other scene loads, so objects created while they wake land in it.
        private static void MakeActiveIfOpen(SceneReference _mainScene)
        {
            if (_mainScene == null)
            {
                return;
            }
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.isLoaded)
                {
                    continue;
                }
                if (StartUpSceneMatch.IsSameScene(ToReference(scene), _mainScene))
                {
                    SceneManager.SetActiveScene(scene);
                    return;
                }
            }
        }
    }
}
