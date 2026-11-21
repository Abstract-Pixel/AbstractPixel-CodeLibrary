using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The one scene system settings asset per project: the start scene group and launch scene, the scene sets and
    /// the default one, where new scenes go,
    /// whether builds check scene groups, and the list of scene groups. It can live in any folder; the Editor finds it
    /// by type and keeps it in Player Settings' Preloaded Assets. Edited in Project Settings &gt; Abstract Pixel &gt; Scene System.
    /// </summary>
    public class SceneSystemSettings : ScriptableObject
    {
        [SerializeField] private SceneGroup startSceneGroup = null;
        [SerializeField] private SceneReference launchScene = new SceneReference(string.Empty);
        [Tooltip("If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group.")]
        [SerializeField] private SceneGroup fallbackSceneGroup = null;
        [SerializeField] private List<SceneSet> sceneSets = new List<SceneSet>();
        [SerializeField] private SceneSet defaultSceneSet = null;
        [SerializeField] private bool newScenesAreAddressable = false;
        [SerializeField] private bool checkSceneGroupsBeforeBuild = true;
        [SerializeField] private List<SceneGroup> sceneGroups = new List<SceneGroup>();
        [Tooltip("The loading scenes of this project. A scene group that shows a loading scene picks one of them, or the default.")]
        [SerializeField] private List<SceneReference> loadingScenes = new List<SceneReference>();
        [Tooltip("The loading scene a scene group shows when it picked none.")]
        [SerializeField] private SceneReference defaultLoadingScene = new SceneReference(string.Empty);
        [Tooltip("Minimum time on screen, in unscaled seconds, so a fast load does not flash the loading scene. Counts from Show screen starting.")]
        [Min(0f)] [SerializeField] private float loadingSceneMinimumSeconds = 1f;
        [Tooltip("Ground layers: the physics layers that count as ground, for Wait for ground below and for adding spawn point positions in the Editor. Triggers never count.")]
        [SerializeField] private LayerMask groundLayers = ~(1 << 2);
        [Tooltip("Steepest ground for relocatables with no label, in degrees. A steeper spawn point position is allowed but shown as a warning.")]
        [Range(0f, 90f)] [SerializeField] private float noLabelSteepestGround = 35f;

        /// <summary>The scene group the game opens at launch. Start-up needs it, except when Play starts in a main scene.</summary>
        public SceneGroup StartSceneGroup
        {
            get { return startSceneGroup; }
            set { startSceneGroup = value; }
        }

        /// <summary>Optional scene, such as a splash, that Unity opens first. Start-up opens the start scene group behind it.</summary>
        public SceneReference LaunchScene
        {
            get { return launchScene; }
            set { launchScene = value; }
        }

        /// <summary>
        /// The scene group a failed scene change goes to (ADR 0011). Empty means the start scene group, which is what
        /// <see cref="FallbackSceneGroup"/> returns then.
        /// </summary>
        public SceneGroup FallbackSceneGroupSetting
        {
            get { return fallbackSceneGroup; }
            set { fallbackSceneGroup = value; }
        }

        /// <summary>The scene group a failed scene change goes to: the one set, otherwise the start scene group.</summary>
        public SceneGroup FallbackSceneGroup
        {
            get
            {
                if (fallbackSceneGroup != null)
                {
                    return fallbackSceneGroup;
                }
                return startSceneGroup;
            }
        }

        /// <summary>True when a launch scene is set.</summary>
        public bool HasLaunchScene
        {
            get { return launchScene != null && !string.IsNullOrEmpty(launchScene.SceneName); }
        }

        public List<SceneSet> SceneSets
        {
            get { return sceneSets; }
        }

        public SceneSet DefaultSceneSet
        {
            get { return defaultSceneSet; }
            set { defaultSceneSet = value; }
        }

        /// <summary>True when new scenes go to Addressables, false when they go to Build Settings.</summary>
        public bool NewScenesAreAddressable
        {
            get { return newScenesAreAddressable; }
            set { newScenesAreAddressable = value; }
        }

        public bool CheckSceneGroupsBeforeBuild
        {
            get { return checkSceneGroupsBeforeBuild; }
            set { checkSceneGroupsBeforeBuild = value; }
        }

        /// <summary>The project's loading scenes.</summary>
        public List<SceneReference> LoadingScenes
        {
            get { return loadingScenes; }
        }

        /// <summary>The loading scene a group shows when it picked none.</summary>
        public SceneReference DefaultLoadingScene
        {
            get { return defaultLoadingScene; }
            set { defaultLoadingScene = value; }
        }

        /// <summary>Minimum time a loading scene stays on screen, in unscaled seconds.</summary>
        public float LoadingSceneMinimumSeconds
        {
            get { return loadingSceneMinimumSeconds; }
            set { loadingSceneMinimumSeconds = Mathf.Max(0f, value); }
        }

        /// <summary>The physics layers that count as ground for relocation (default: everything except Ignore Raycast).</summary>
        public LayerMask GroundLayers
        {
            get { return groundLayers; }
            set { groundLayers = value; }
        }

        /// <summary>Steepest ground, in degrees, for relocatables and spawn points with no relocation label.</summary>
        public float NoLabelSteepestGround
        {
            get { return noLabelSteepestGround; }
            set { noLabelSteepestGround = Mathf.Clamp(value, 0f, 90f); }
        }

        /// <summary>Every scene group in the project, kept current by an editor script.</summary>
        public List<SceneGroup> SceneGroups
        {
            get { return sceneGroups; }
        }
    }
}
