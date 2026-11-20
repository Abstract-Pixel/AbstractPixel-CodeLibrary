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

            decidedLaunchSceneHold = true;
            List<SceneReference> openScenes = OpenScenesActiveFirst();
            bool shouldHold = StartUpRule.ShouldHoldLaunchScene(settings, openScenes);
            if (shouldHold == false)
            {
                return;
            }

            LaunchSceneHold.Hold();
            Debug.Log("[Scene system] Start-up: the launch scene '" + settings.LaunchScene.SceneName + "' holds the start scene group until it calls SceneActions.ReleaseLaunchScene().");
        }

        private static async Task RunAsync()
        {
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            List<SceneReference> openScenes = OpenScenesActiveFirst();
            StartUpChoice choice = StartUpRule.Choose(settings, openScenes);
            LogChoice(choice);

            if (choice.Kind == StartUpChoiceKind.DoNothing)
            {
                IsPending = false;
                HasFailed = true;
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
