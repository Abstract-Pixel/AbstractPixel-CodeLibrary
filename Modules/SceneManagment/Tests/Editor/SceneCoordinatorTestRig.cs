using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A coordinator with a recording loader, already started in one level, plus a way to build scene groups.
    /// Dispose it in the test's tear down.
    /// </summary>
    internal sealed class SceneCoordinatorTestRig
    {
        public const string ManagerScene = "ManagerBootScene";
        public const string CarScene = "GameBootScene";

        private readonly List<Object> createdObjects = new List<Object>();

        public SceneCoordinator Coordinator { get; private set; }
        public RecordingSceneLoader Loader { get; private set; }
        public SceneGroup StartGroup { get; private set; }

        /// <summary>A coordinator with a recording loader that has not started the game yet.</summary>
        public static SceneCoordinatorTestRig BeforeStartUp()
        {
            SceneCoordinatorTestRig rig = new SceneCoordinatorTestRig();
            GameObject coordinatorObject = new GameObject("SceneCoordinator under test");
            rig.createdObjects.Add(coordinatorObject);
            rig.Coordinator = coordinatorObject.AddComponent<SceneCoordinator>();
            rig.Loader = new RecordingSceneLoader();
            rig.Coordinator.SetSceneLoader(rig.Loader);
            rig.Coordinator.FindFallbackSceneGroup = NoFallback;
            PinSingletonInstance(rig.Coordinator);
            return rig;
        }

        public static SceneCoordinatorTestRig StartInLevel(string _levelName)
        {
            SceneCoordinatorTestRig rig = new SceneCoordinatorTestRig();
            GameObject coordinatorObject = new GameObject("SceneCoordinator under test");
            rig.createdObjects.Add(coordinatorObject);
            rig.Coordinator = coordinatorObject.AddComponent<SceneCoordinator>();
            rig.Loader = new RecordingSceneLoader();
            rig.Coordinator.SetSceneLoader(rig.Loader);
            rig.Coordinator.FindFallbackSceneGroup = NoFallback;
            PinSingletonInstance(rig.Coordinator);
            rig.StartGroup = rig.CreateLevelGroup(_levelName);
            rig.Coordinator.InitializeStartSceneGroup(rig.StartGroup);
            return rig;
        }

        // Tests that need a fallback scene group set their own; the project's settings never leak into a test.
        private static SceneGroup NoFallback()
        {
            return null;
        }

        // Shaped like a Rally Shift level group: a manager scene, a force-reloaded car scene and the level.
        public SceneGroup CreateLevelGroup(string _levelName)
        {
            string[] sharedScenes = { CarScene };
            return CreateGroup(_levelName, sharedScenes, true);
        }

        public SceneGroup CreateGroup(string _levelName, string[] _sharedSceneNames, bool _forceReload)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            createdObjects.Add(group);
            SceneReference[] managerialScenes = { new SceneReference(ManagerScene) };
            List<SceneReference> contextualScenes = new List<SceneReference>();
            foreach (string sharedSceneName in _sharedSceneNames)
            {
                contextualScenes.Add(new SceneReference(sharedSceneName));
            }

            group.Initialize(managerialScenes, contextualScenes, new SceneReference(_levelName), _forceReload);
            return group;
        }

        // Awake does nothing in EditMode, so SceneCoordinator.Instance would search the open scenes and could find the
        // game's own coordinator (ManagerBootScene has one) instead of this test's. Pinning makes the test's the instance.
        private static void PinSingletonInstance(SceneCoordinator _coordinator)
        {
            System.Reflection.FieldInfo instanceField = typeof(PersistentSingleton<SceneCoordinator>).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            instanceField.SetValue(null, _coordinator);
        }

        public void Dispose()
        {
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }
    }
}
