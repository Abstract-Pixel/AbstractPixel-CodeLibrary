using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The one scene system settings asset per project: the scene sets and the default one, where new scenes go,
    /// whether builds check scene groups, and the list of scene groups. It can live in any folder; the Editor finds it
    /// by type and keeps it in Player Settings' Preloaded Assets. Edited in Project Settings &gt; Abstract Pixel &gt; Scenes.
    /// </summary>
    public class SceneSystemSettings : ScriptableObject
    {
        [SerializeField] private List<SceneSet> sceneSets = new List<SceneSet>();
        [SerializeField] private SceneSet defaultSceneSet = null;
        [SerializeField] private bool newScenesAreAddressable = false;
        [SerializeField] private bool checkSceneGroupsBeforeBuild = true;
        [SerializeField] private List<SceneGroup> sceneGroups = new List<SceneGroup>();

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
