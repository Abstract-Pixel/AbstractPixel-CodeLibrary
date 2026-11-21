using System.Threading.Tasks;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Unity refuses to close the last open scene ("Unloading the last loaded scene is not supported"). A scene group
    /// with no manager scene that resets its shared scenes closes everything in a change, so the scene that stays would
    /// be opened a second time and every object in it doubled (found in 4D with the game's drones). Before the last
    /// open scene closes, the guard opens an empty stand-in scene; it closes again once the next main scene is open.
    /// Play mode only; the loader layer is the only one that talks to Unity's SceneManager.
    /// </summary>
    internal sealed class LastSceneGuard
    {
        private const string StandInName = "Scene change stand-in";
        private Scene standIn;

        /// <summary>Opens the stand-in when the scene about to close is the only open scene.</summary>
        public void BeforeClosing(string _sceneName)
        {
            if (!Application.isPlaying || standIn.IsValid())
            {
                return;
            }
            bool closesTheLastScene = LoadedSceneCount() == 1 && SceneManager.GetSceneAt(0).name == _sceneName;
            if (!closesTheLastScene)
            {
                return;
            }
            standIn = SceneManager.CreateScene(StandInName);
            SceneManager.SetActiveScene(standIn);
        }

        /// <summary>Closes the stand-in once another scene is open to take over.</summary>
        public async Task AfterOpening()
        {
            if (!standIn.IsValid() || !standIn.isLoaded || LoadedSceneCount() < 2)
            {
                return;
            }
            Scene closing = standIn;
            standIn = default;
            AsyncOperation unload = SceneManager.UnloadSceneAsync(closing);
            if (unload != null)
            {
                await unload.AsTask();
            }
        }

        private static int LoadedSceneCount()
        {
            int loaded = 0;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index).isLoaded)
                {
                    loaded++;
                }
            }
            return loaded;
        }
    }
}
