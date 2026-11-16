using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A scene loader for tests. It records every call in order and finishes each one at once, unless a test asks it to
    /// hold the unloads or to fail loading one scene.
    /// </summary>
    internal sealed class RecordingSceneLoader : ISceneLoader
    {
        public const string ReleaseCall = "release memory";
        public const string LoadPrefix = "load ";
        public const string UnloadPrefix = "unload ";

        public readonly List<string> Calls = new List<string>();

        /// <summary>While set, every unload waits for this task. Lets a test hold a scene change half way.</summary>
        public Task UnloadGate;

        /// <summary>Loading the scene with this name fails with an exception.</summary>
        public string FailLoadOfScene;

        public Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            Calls.Add(LoadPrefix + sceneReference.SceneName);
            if (sceneReference.SceneName == FailLoadOfScene)
            {
                return Task.FromException(new InvalidOperationException("test load failure for " + FailLoadOfScene));
            }

            return Task.CompletedTask;
        }

        public Task UnloadScene(SceneReference sceneReference)
        {
            Calls.Add(UnloadPrefix + sceneReference.SceneName);
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
    }
}
