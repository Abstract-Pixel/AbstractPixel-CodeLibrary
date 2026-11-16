using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A scene loader for tests. It records every call in order and finishes each one at once, unless a test asks it
    /// to hold the unloads or to fail loading one scene.
    /// It also follows Unity's rule for a held scene (the loader contract on <see cref="ISceneLoader"/>): a main scene
    /// loaded with <c>sceneActivatedByDefault = false</c> is held. Loading it again with activation, or unloading it,
    /// clears the hold. Any other load or unload while it is held returns a task that never completes.
    /// </summary>
    internal sealed class RecordingSceneLoader : ISceneLoader
    {
        public const string ReleaseCall = "release memory";
        public const string LoadPrefix = "load ";
        public const string UnloadPrefix = "unload ";

        public readonly List<string> Calls = new List<string>();

        /// <summary>The scenes that were asked to load without activating (held back).</summary>
        public readonly List<string> HeldLoads = new List<string>();

        /// <summary>The calls that were issued while a scene was held and so never complete (the stall).</summary>
        public readonly List<string> StalledCalls = new List<string>();

        /// <summary>Follow Unity's rule for a held scene. On by default, so every test is realistic.</summary>
        public bool FollowHeldSceneRule = true;

        /// <summary>The name of the scene that waits for activation, or null.</summary>
        public string HeldScene { get; private set; }

        /// <summary>While set, every unload waits for this task. Lets a test hold a scene change half way.</summary>
        public Task UnloadGate;

        /// <summary>Loading the scene with this name fails with an exception.</summary>
        public string FailLoadOfScene;

        public Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            string sceneName = sceneReference.SceneName;
            Calls.Add(LoadPrefix + sceneName);
            if (!sceneActivatedByDefault)
            {
                HeldLoads.Add(sceneName);
            }

            bool isAskingForTheHeldScene = sceneName == HeldScene;
            if (IsStalledByHeldScene(sceneName))
            {
                StalledCalls.Add(LoadPrefix + sceneName);
                return NeverCompletes();
            }

            if (sceneName == FailLoadOfScene)
            {
                return Task.FromException(new InvalidOperationException("test load failure for " + FailLoadOfScene));
            }

            if (isAskingForTheHeldScene && sceneActivatedByDefault)
            {
                HeldScene = null;
            }

            if (!sceneActivatedByDefault && FollowHeldSceneRule)
            {
                HeldScene = sceneName;
            }

            return Task.CompletedTask;
        }

        public Task UnloadScene(SceneReference sceneReference)
        {
            string sceneName = sceneReference.SceneName;
            Calls.Add(UnloadPrefix + sceneName);
            if (IsStalledByHeldScene(sceneName))
            {
                StalledCalls.Add(UnloadPrefix + sceneName);
                return NeverCompletes();
            }

            if (sceneName == HeldScene)
            {
                // Unity activates a held scene before it can close it.
                HeldScene = null;
            }

            if (UnloadGate != null)
            {
                return UnloadGate;
            }

            return Task.CompletedTask;
        }

        public Task ReleaseUnusedMemory()
        {
            Calls.Add(ReleaseCall);
            return Task.CompletedTask;
        }

        public int CountCalls(string _call)
        {
            return Calls.FindAll(call => call == _call).Count;
        }

        /// <summary>
        /// Replays the calls and returns the most copies of a scene that were open at once.
        /// A scene that was open before the first call is passed as <paramref name="_copiesOpenAtStart"/>.
        /// </summary>
        public int MostCopiesOpenAtOnce(string _sceneName, int _copiesOpenAtStart)
        {
            int open = _copiesOpenAtStart;
            int most = open;
            foreach (string call in Calls)
            {
                if (call == LoadPrefix + _sceneName)
                {
                    open++;
                }

                if (call == UnloadPrefix + _sceneName)
                {
                    open--;
                }

                most = Math.Max(most, open);
            }

            return most;
        }

        /// <summary>A load or unload of any scene other than the held one waits for ever while a scene is held.</summary>
        private bool IsStalledByHeldScene(string _sceneName)
        {
            if (!FollowHeldSceneRule || HeldScene == null)
            {
                return false;
            }

            return _sceneName != HeldScene;
        }

        private static Task NeverCompletes()
        {
            TaskCompletionSource<bool> never = new TaskCompletionSource<bool>();
            return never.Task;
        }
    }
}
