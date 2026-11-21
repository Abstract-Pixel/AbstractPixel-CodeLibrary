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

        /// <summary>Every scene group in the project, kept current by an editor script.</summary>
        public List<SceneGroup> SceneGroups
        {
            get { return sceneGroups; }
        }
    }
}
