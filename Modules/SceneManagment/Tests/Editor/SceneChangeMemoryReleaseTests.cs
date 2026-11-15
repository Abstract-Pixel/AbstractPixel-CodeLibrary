using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Releasing unused memory is slow, so a scene change does it once, after the old scenes have closed, instead of once
    /// for every scene it closes.
    /// </summary>
    public class SceneChangeMemoryReleaseTests
    {
        private const string ReleaseCall = "release memory";
        private const string UnloadPrefix = "unload ";

        /// <summary>Records every loader call in order and finishes each one at once.</summary>
        private sealed class RecordingSceneLoader : ISceneLoader
        {
            public readonly List<string> Calls = new List<string>();

            public Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
            {
                Calls.Add("load " + sceneReference.SceneName);
                return Task.CompletedTask;
            }

            public Task UnloadScene(SceneReference sceneReference)
            {
                Calls.Add(UnloadPrefix + sceneReference.SceneName);
                return Task.CompletedTask;
            }

            public Task ReleaseUnusedMemory()
            {
                Calls.Add(ReleaseCall);
                return Task.CompletedTask;
            }
        }

        private readonly List<Object> createdObjects = new List<Object>();
        private SceneCoordinator coordinator;
        private RecordingSceneLoader loader;

        [SetUp]
        public void StartInLevelOne()
        {
            GameObject coordinatorObject = new GameObject("SceneCoordinator under test");
            createdObjects.Add(coordinatorObject);
            coordinator = coordinatorObject.AddComponent<SceneCoordinator>();
            loader = new RecordingSceneLoader();
            coordinator.SetSceneLoader(loader);
            coordinator.InitializeStartSceneGroup(CreateLevelGroup("Level1"));
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public async Task SceneChangeReleasesMemoryOnceAfterClosingScenes()
        {
            await coordinator.TransitionToSceneGroup(CreateLevelGroup("Level2"));

            AssertReleasedOnceAfterLastUnload();
        }

        [Test]
        public async Task RetryReleasesMemoryOnceAfterClosingScenes()
        {
            await coordinator.ReloadActiveSceneGroup();

            AssertReleasedOnceAfterLastUnload();
        }

        // Shaped like a Rally Shift level group: a manager scene, a force-reloaded car scene and the level.
        private SceneGroup CreateLevelGroup(string _levelName)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            createdObjects.Add(group);
            SceneReference[] managerialScenes = { new SceneReference("ManagerBootScene") };
            SceneReference[] contextualScenes = { new SceneReference("GameBootScene") };
            group.Initialize(managerialScenes, contextualScenes, new SceneReference(_levelName), true);
            return group;
        }

        private void AssertReleasedOnceAfterLastUnload()
        {
            string callLog = string.Join(", ", loader.Calls);
            int unloadCount = loader.Calls.FindAll(call => call.StartsWith(UnloadPrefix)).Count;
            Assert.Greater(unloadCount, 1, "the scene change should close more than one scene: " + callLog);

            int releaseCount = loader.Calls.FindAll(call => call == ReleaseCall).Count;
            Assert.AreEqual(1, releaseCount, "memory releases in: " + callLog);

            int lastUnloadIndex = loader.Calls.FindLastIndex(call => call.StartsWith(UnloadPrefix));
            Assert.Greater(loader.Calls.IndexOf(ReleaseCall), lastUnloadIndex, "release must follow the last unload: " + callLog);
        }
    }
}
