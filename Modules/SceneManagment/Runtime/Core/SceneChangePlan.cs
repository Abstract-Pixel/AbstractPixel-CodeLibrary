using AbstractPixel.Core;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Works out which scenes a scene change closes and opens. Plain data in, plain data out: it never calls Unity's
    /// scene API, so it can be tested without opening a scene.
    /// Manager scenes are never closed. Going to the group that is already open (Retry) gives a plan that closes and
    /// reopens the main scene, and the shared scenes too when the group force reloads them.
    /// </summary>
    internal class SceneChangePlan
    {
        /// <summary>The scenes that are open before the change.</summary>
        internal class OpenScenes
        {
            public SceneReference MainScene;
            public HashSet<SceneReference> ManagerialScenes = new HashSet<SceneReference>();
            public HashSet<SceneReference> ContextualScenes = new HashSet<SceneReference>();
        }

        public SceneGroup TargetGroup { get; }
        public SceneReference MainSceneToClose { get; }
        public SceneReference MainSceneToOpen { get; }
        public HashSet<SceneReference> ContextualToClose { get; }
        public HashSet<SceneReference> ContextualToOpen { get; }
        public HashSet<SceneReference> ManagerialToOpen { get; }

        /// <summary>The shared scenes that are open once the change has finished.</summary>
        public HashSet<SceneReference> ContextualOpenAfter { get; }

        /// <summary>The manager scenes that are open once the change has finished.</summary>
        public HashSet<SceneReference> ManagerialOpenAfter { get; }

        public SceneChangePlan(SceneGroup _targetGroup, OpenScenes _open)
        {
            TargetGroup = _targetGroup;
            MainSceneToClose = _open.MainScene;
            MainSceneToOpen = _targetGroup.MainScene;

            HashSet<SceneReference> targetContextual = new HashSet<SceneReference>(_targetGroup.ContextualBootScenesList);
            if (_targetGroup.ForceReloadContextualScenes)
            {
                // Shared scenes hold per-run state, so every one is closed and opened again.
                ContextualToClose = new HashSet<SceneReference>(_open.ContextualScenes);
                ContextualToOpen = targetContextual;
            }
            else
            {
                ContextualToClose = new HashSet<SceneReference>(_open.ContextualScenes);
                ContextualToClose.ExceptWith(targetContextual);

                ContextualToOpen = new HashSet<SceneReference>(targetContextual);
                ContextualToOpen.ExceptWith(_open.ContextualScenes);
            }

            ManagerialToOpen = new HashSet<SceneReference>(_targetGroup.ManagerialBootScenesList);
            ManagerialToOpen.ExceptWith(_open.ManagerialScenes);

            ContextualOpenAfter = new HashSet<SceneReference>(_open.ContextualScenes);
            ContextualOpenAfter.ExceptWith(ContextualToClose);
            ContextualOpenAfter.UnionWith(ContextualToOpen);

            ManagerialOpenAfter = new HashSet<SceneReference>(_open.ManagerialScenes);
            ManagerialOpenAfter.UnionWith(ManagerialToOpen);
        }
    }
}
