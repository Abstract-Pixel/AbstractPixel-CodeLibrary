using AbstractPixel.Core;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Works out which scenes a scene change closes and opens. Plain data in, plain data out: it never calls Unity's
    /// scene API, so it can be tested without opening a scene.
    /// Manager scenes are never closed. Going to the group that is already open (Retry) gives a plan that closes and
    /// reopens the main scene, and the shared scenes too when the group force reloads them.
    /// Two more cases exist for preload: <see cref="ForPreload"/> opens only what is not open, and a plan made while
    /// <see cref="OpenScenes.PreloadedScenes"/> is filled skips the scenes the preload already opened.
    /// </summary>
    internal class SceneChangePlan
    {
        private enum Kind
        {
            Change,
            Preload,
        }

        /// <summary>The scenes that are open: the record the coordinator keeps, and the input of a plan.</summary>
        internal class OpenScenes
        {
            /// <summary>The scene group these scenes belong to. Null before start-up and after a failed change.</summary>
            public SceneGroup Group;
            public SceneReference MainScene;

            /// <summary>
            /// True when a failed change closed the main scene and opened none. A null main scene then means "none is
            /// open", not "unknown", so the next change does not close Unity's active scene.
            /// </summary>
            public bool MainSceneClosed;
            public HashSet<SceneReference> ManagerialScenes = new HashSet<SceneReference>();
            public HashSet<SceneReference> ContextualScenes = new HashSet<SceneReference>();

            /// <summary>
            /// Scenes a preload of the target group opened and is holding. The main scene is among them. Filled only when
            /// the change is a switch to a preloaded group.
            /// </summary>
            public HashSet<SceneReference> PreloadedScenes = new HashSet<SceneReference>();

            /// <summary>A copy whose sets can change without touching this record.</summary>
            public OpenScenes Copy()
            {
                OpenScenes copy = new OpenScenes();
                copy.Group = Group;
                copy.MainScene = MainScene;
                copy.MainSceneClosed = MainSceneClosed;
                copy.ManagerialScenes = new HashSet<SceneReference>(ManagerialScenes);
                copy.ContextualScenes = new HashSet<SceneReference>(ContextualScenes);
                return copy;
            }

            /// <summary>The record of a group that is open with all of its scenes.</summary>
            public static OpenScenes OfGroup(SceneGroup _group)
            {
                OpenScenes record = new OpenScenes();
                record.Group = _group;
                record.MainScene = _group.MainScene;
                record.ManagerialScenes = new HashSet<SceneReference>(_group.ManagerialBootScenesList);
                record.ContextualScenes = new HashSet<SceneReference>(_group.ContextualBootScenesList);
                return record;
            }
        }

        public SceneGroup TargetGroup { get; }

        /// <summary>The group whose scenes close, for the "group closed" message. Null for start-up and preload.</summary>
        public SceneGroup GroupThatCloses { get; }
        public SceneReference MainSceneToClose { get; }
        public SceneReference MainSceneToOpen { get; }
        public HashSet<SceneReference> ContextualToClose { get; }
        public HashSet<SceneReference> ContextualToOpen { get; }
        public HashSet<SceneReference> ManagerialToOpen { get; }

        /// <summary>The shared scenes that are open once the change has finished.</summary>
        public HashSet<SceneReference> ContextualOpenAfter { get; }

        /// <summary>The manager scenes that are open once the change has finished.</summary>
        public HashSet<SceneReference> ManagerialOpenAfter { get; }

        /// <summary>False when the record knows no main scene is open, so nothing stands in for it.</summary>
        public bool ClosesActiveSceneWhenNoMainScene { get; }

        /// <summary>True when a preload already opened some of the target's scenes (a switch to a preloaded group).</summary>
        public bool UsesPreloadedScenes { get; }

        public SceneChangePlan(SceneGroup _targetGroup, OpenScenes _open) : this(_targetGroup, _open, Kind.Change)
        {
        }

        /// <summary>
        /// What is really open after this plan failed part way: the scenes it opened are added, the old scenes are taken
        /// out when they were closed, and no group counts as open.
        /// </summary>
        public OpenScenes OpenAfterFailure(OpenScenes _before, ICollection<SceneReference> _opened, bool _oldScenesClosed)
        {
            OpenScenes record = _before.Copy();
            record.Group = null;
            if (_oldScenesClosed)
            {
                record.ContextualScenes.ExceptWith(ContextualToClose);
                record.MainScene = null;
                record.MainSceneClosed = MainSceneToClose != null;
            }
            foreach (SceneReference scene in _opened)
            {
                if (scene.Equals(MainSceneToOpen))
                {
                    record.MainScene = scene;
                    record.MainSceneClosed = false;
                }
                else if (ManagerialToOpen.Contains(scene))
                {
                    record.ManagerialScenes.Add(scene);
                }
                else
                {
                    record.ContextualScenes.Add(scene);
                }
            }
            return record;
        }

        /// <summary>What is open once this plan has run to its end: the coordinator's new record.</summary>
        public OpenScenes OpenAfterChange()
        {
            OpenScenes record = new OpenScenes();
            record.Group = TargetGroup;
            record.MainScene = TargetGroup.MainScene;
            record.ManagerialScenes = new HashSet<SceneReference>(ManagerialOpenAfter);
            record.ContextualScenes = new HashSet<SceneReference>(ContextualOpenAfter);
            return record;
        }

        /// <summary>
        /// The scenes a preload opens: the main scene (held back by the coordinator) and every manager or shared scene
        /// that is not open yet. A shared scene that is open stays as it is, even when the group force reloads it,
        /// because a second copy of it must never open. Nothing is closed.
        /// </summary>
        public static SceneChangePlan ForPreload(SceneGroup _targetGroup, OpenScenes _open)
        {
            return new SceneChangePlan(_targetGroup, _open, Kind.Preload);
        }

        /// <summary>
        /// The scenes start-up opens: every scene of the group that Unity has not opened yet. Nothing is closed: a
        /// launch scene closes itself. Once it ends, the group's manager and shared scenes are open. The main scene to
        /// open is null when it is already open.
        /// </summary>
        public static SceneChangePlan ForStartUp(SceneGroup _targetGroup, HashSet<SceneReference> _scenesAlreadyOpen)
        {
            return new SceneChangePlan(_targetGroup, _scenesAlreadyOpen);
        }

        private SceneChangePlan(SceneGroup _targetGroup, HashSet<SceneReference> _scenesAlreadyOpen)
        {
            TargetGroup = _targetGroup;
            UsesPreloadedScenes = false;
            MainSceneToClose = null;
            ContextualToClose = new HashSet<SceneReference>();

            MainSceneToOpen = _targetGroup.MainScene;
            bool mainSceneIsOpen = MainSceneToOpen != null && _scenesAlreadyOpen.Contains(MainSceneToOpen);
            if (mainSceneIsOpen)
            {
                MainSceneToOpen = null;
            }

            ManagerialToOpen = new HashSet<SceneReference>(_targetGroup.ManagerialBootScenesList);
            ManagerialToOpen.ExceptWith(_scenesAlreadyOpen);
            ContextualToOpen = new HashSet<SceneReference>(_targetGroup.ContextualBootScenesList);
            ContextualToOpen.ExceptWith(_scenesAlreadyOpen);

            ManagerialOpenAfter = new HashSet<SceneReference>(_targetGroup.ManagerialBootScenesList);
            ContextualOpenAfter = new HashSet<SceneReference>(_targetGroup.ContextualBootScenesList);
        }

        private SceneChangePlan(SceneGroup _targetGroup, OpenScenes _open, Kind _kind)
        {
            bool isPreload = _kind == Kind.Preload;
            HashSet<SceneReference> preloaded = _open.PreloadedScenes;
            UsesPreloadedScenes = preloaded.Count > 0;
            TargetGroup = _targetGroup;
            MainSceneToClose = null;
            if (!isPreload)
            {
                MainSceneToClose = _open.MainScene;
                GroupThatCloses = _open.Group;
            }
            ClosesActiveSceneWhenNoMainScene = !isPreload && !_open.MainSceneClosed;
            MainSceneToOpen = _targetGroup.MainScene;

            HashSet<SceneReference> targetContextual = new HashSet<SceneReference>(_targetGroup.ContextualBootScenesList);
            bool reloadSharedScenes = _targetGroup.ForceReloadContextualScenes && !isPreload;
            if (reloadSharedScenes)
            {
                // Shared scenes hold per-run state, so every one is closed and opened again.
                ContextualToClose = new HashSet<SceneReference>(_open.ContextualScenes);
                ContextualToOpen = new HashSet<SceneReference>(targetContextual);
            }
            else
            {
                ContextualToClose = new HashSet<SceneReference>(_open.ContextualScenes);
                ContextualToClose.ExceptWith(targetContextual);

                ContextualToOpen = new HashSet<SceneReference>(targetContextual);
                ContextualToOpen.ExceptWith(_open.ContextualScenes);
            }

            if (isPreload)
            {
                ContextualToClose.Clear();
            }
            ContextualToOpen.ExceptWith(preloaded);

            ManagerialToOpen = new HashSet<SceneReference>(_targetGroup.ManagerialBootScenesList);
            ManagerialToOpen.ExceptWith(_open.ManagerialScenes);
            ManagerialToOpen.ExceptWith(preloaded);

            // Scenes the preload opened are already part of the target group, so they are open once the change ends.
            HashSet<SceneReference> preloadedContextual = new HashSet<SceneReference>(targetContextual);
            preloadedContextual.IntersectWith(preloaded);
            HashSet<SceneReference> preloadedManagerial = new HashSet<SceneReference>(_targetGroup.ManagerialBootScenesList);
            preloadedManagerial.IntersectWith(preloaded);

            ContextualOpenAfter = new HashSet<SceneReference>(_open.ContextualScenes);
            ContextualOpenAfter.ExceptWith(ContextualToClose);
            ContextualOpenAfter.UnionWith(ContextualToOpen);
            ContextualOpenAfter.UnionWith(preloadedContextual);

            ManagerialOpenAfter = new HashSet<SceneReference>(_open.ManagerialScenes);
            ManagerialOpenAfter.UnionWith(ManagerialToOpen);
            ManagerialOpenAfter.UnionWith(preloadedManagerial);
        }
    }
}
