using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Asks Addressables whether a scene has an entry, by its asset GUID. A scene is either in Build Settings or
    /// Addressable, never both, so this alone decides the loader. A scene known only by name cannot be Addressable.
    /// The answer for a scene cannot change while the game runs, so it is kept.
    /// </summary>
    internal sealed class AddressableSceneCheck : IAddressableSceneCheck
    {
        private readonly Dictionary<string, bool> answers = new Dictionary<string, bool>();
        private Task initialization;

        /// <summary>
        /// Set by the scene system's editor code when Addressables is installed: whether a scene (by GUID) has an
        /// Addressables entry, read from the editor settings. Null in a build, where the runtime locators answer.
        /// </summary>
        internal static Func<string, bool> EditorAnswer;

        private readonly Func<bool> hasStarted;
        private readonly Func<Task> start;

        public AddressableSceneCheck() : this(AddressablesHaveStarted, InitializeAddressables)
        {
        }

        // Stubs for tests: whether Addressables has started, and how to start it.
        internal AddressableSceneCheck(Func<bool> _hasStarted, Func<Task> _start)
        {
            hasStarted = _hasStarted;
            start = _start;
        }

        public async Task<bool> IsAddressable(SceneReference _scene)
        {
            if (_scene == null || !_scene.HasGuid)
            {
                return false;
            }

            string key = _scene.SceneGuid;
            if (answers.TryGetValue(key, out bool knownAnswer))
            {
                return knownAnswer;
            }

            // In the Editor the settings know the answer; starting Addressables for it would fail in a project that has
            // never built Addressables content.
            if (EditorAnswer != null)
            {
                bool knownInEditor = EditorAnswer(key);
                answers[key] = knownInEditor;
                return knownInEditor;
            }

            bool ready = await EnsureInitialized();
            bool isAddressable = ready && HasSceneEntry(key);
            answers[key] = isAddressable;
            return isAddressable;
        }

        private async Task<bool> EnsureInitialized()
        {
            // Started already (an earlier Play with domain reload off, or another caller): nothing to wait for. In the
            // Editor, InitializeAsync() can hand back a handle whose Completed never fires once Addressables has started.
            if (hasStarted())
            {
                return true;
            }
            if (initialization == null)
            {
                initialization = start();
            }

            await initialization;
            return hasStarted();
        }

        // Addressables has started once it has a resource locator.
        private static bool AddressablesHaveStarted()
        {
            if (Addressables.ResourceLocators == null)
            {
                return false;
            }
            foreach (var locator in Addressables.ResourceLocators)
            {
                return true;
            }
            return false;
        }

        private static async Task InitializeAddressables()
        {
            try
            {
                TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
                Addressables.InitializeAsync().Completed += _ => completion.TrySetResult(true);
                await completion.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SceneLoader] Addressables did not start, every scene loads from Build Settings. " + ex.Message);
            }
        }

        private static bool HasSceneEntry(string _key)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                bool found = locator.Locate(_key, typeof(SceneInstance), out IList<IResourceLocation> locations);
                if (found && locations != null && locations.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
