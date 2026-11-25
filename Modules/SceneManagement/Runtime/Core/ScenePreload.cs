using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The one scene group opened in the background before the scene change to it, and every scene that preload
    /// opened (its held main scene included). At most one group is preloaded.
    /// </summary>
    internal sealed class ScenePreload
    {
        /// <summary>The preloaded group, set once every scene of the preload has loaded. Null when none.</summary>
        public SceneGroup Group { get; set; }

        /// <summary>Every scene the current preload opened, the held main scene included. Cleared, never null.</summary>
        public readonly HashSet<SceneReference> Scenes = new HashSet<SceneReference>();

        private SceneReference mainScene = null;

        public bool HasAny
        {
            get { return Group != null || Scenes.Count > 0; }
        }

        /// <summary>Remembers the scenes a preload is about to open, so they can be closed again if it is replaced.</summary>
        public void Remember(SceneChangePlan _plan)
        {
            Scenes.UnionWith(_plan.ManagerialToOpen);
            Scenes.UnionWith(_plan.ContextualToOpen);
            Scenes.Add(_plan.MainSceneToOpen);
            mainScene = _plan.MainSceneToOpen;
        }

        public void Clear()
        {
            Scenes.Clear();
            mainScene = null;
            Group = null;
        }

        /// <summary>
        /// Closes the scenes of the preload. Replacing a preload counts as a scene change for the memory release:
        /// without it the old level's assets would stay in memory next to the new preload. A scene change that follows
        /// releases memory itself, so it passes false.
        /// </summary>
        public async Task Discard(ISceneLoader _loader, bool _releaseMemory)
        {
            if (Scenes.Count == 0)
            {
                Group = null;
                return;
            }

            // The held main scene closes first. Unity holds every later scene operation, unloads included, while it waits.
            List<SceneReference> scenesToClose = new List<SceneReference>();
            scenesToClose.Add(mainScene);
            foreach (SceneReference scene in Scenes)
            {
                if (!scene.Equals(mainScene))
                {
                    scenesToClose.Add(scene);
                }
            }
            Clear();

            try
            {
                foreach (SceneReference scene in scenesToClose)
                {
                    await _loader.UnloadScene(scene);
                }
                if (_releaseMemory)
                {
                    await _loader.ReleaseUnusedMemory();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
