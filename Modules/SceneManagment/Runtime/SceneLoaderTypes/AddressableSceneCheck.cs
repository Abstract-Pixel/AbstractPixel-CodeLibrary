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

            bool ready = await EnsureInitialized();
            bool isAddressable = ready && HasSceneEntry(key);
            answers[key] = isAddressable;
            return isAddressable;
        }

        private async Task<bool> EnsureInitialized()
        {
            if (initialization == null)
            {
                initialization = InitializeAddressables();
            }

            await initialization;
            return Addressables.ResourceLocators != null;
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
