using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.GameManagement.Samples
{
    /// <summary>
    /// Sample 01: starts the state machine with the sample's own settings, and reloads the scene for Retry and Quit.
    /// The scene change makes the machine leave every state marked to leave; the scene's SceneStartRequester then
    /// enters Main Menu, and Retry asks for Play right after it.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class MinimalGameSceneFlow : MonoBehaviour
    {
        [Tooltip("The settings this sample runs on. Used only when no machine exists yet.")]
        [SerializeField] private GameManagementSettings settings;
        [Tooltip("The state Retry enters once the scene is loaded again.")]
        [SerializeField] private StateSO play;

        private static bool shouldPlayAfterReload;

        private void Awake()
        {
            bool usesOtherSettings = GameStates.HasMachine && !GameStates.Machine.UsesSettings(settings);
            if (!GameStates.HasMachine || usesOtherSettings)
            {
                GameStates.Use(GameStateDriver.CreateMachine(settings));
            }
        }

        private void Start()
        {
            if (shouldPlayAfterReload)
            {
                shouldPlayAfterReload = false;
                GameStates.Request(play, name);
            }
        }

        /// <summary>Loads the scene again and goes straight back to Play.</summary>
        public void Retry()
        {
            shouldPlayAfterReload = true;
            ReloadScene();
        }

        /// <summary>Loads the scene again; it opens on Main Menu.</summary>
        public void QuitToMainMenu()
        {
            shouldPlayAfterReload = false;
            ReloadScene();
        }

        private void ReloadScene()
        {
            string scenePath = gameObject.scene.path;
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
        }
    }
}
