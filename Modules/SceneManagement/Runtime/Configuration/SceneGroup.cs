using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [CreateAssetMenu(fileName = "SceneGroup", menuName = "Utility/SceneRelated/SceneGroup", order = 1)]
    /// <summary>One place the player can be: a main scene with its manager scenes, shared scenes and loading scene choice.</summary>
    /// <remarks>
    /// - Its identity is the asset itself: two groups are equal only when they are the same object, even if they name the same main scene (session 4C).
    /// - A runtime group such as a temporary group equals only itself.
    /// </remarks>
    public class SceneGroup : ScriptableObject
    {
        public List<SceneReference> ManagerialBootScenesList = new List<SceneReference>();
        public List<SceneReference> ContextualBootScenesList = new List<SceneReference>();
        public SceneReference MainScene;
        public bool ForceReloadContextualScenes = false;

        [Tooltip("Show the loading scene: Never; When arriving from another scene group (a Retry goes straight back in without it); or Every time, Retry included.")]
        [SerializeField] private ShowLoadingScene showLoadingScene = ShowLoadingScene.Never;
        [Tooltip("The loading scene this group shows. Empty uses the default loading scene from the scene system settings.")]
        [SerializeField] private SceneReference loadingScene = new SceneReference(string.Empty);
        [Tooltip("Sample scene group: its scenes open in the Editor but never go into a build (ADR 0014).")]
        [SerializeField] private bool isSample = false;
        [Tooltip("Editor only. Play started from this scene runs the launch scene hold with this sample scene group as the start scene group.")]
        [SerializeField] private SceneReference sampleLaunchScene = new SceneReference(string.Empty);

        // The scene set that fills the three parts below, and which of them are overrides (ADR 0008). The loader does
        // not read these: it reads the lists above, which the Editor keeps in step with the set.
        [SerializeField] private SceneSet sceneSet = null;
        [SerializeField] private bool managerScenesOverride = false;
        [SerializeField] private bool sharedScenesOverride = false;
        [SerializeField] private bool resetSharedScenesOverride = false;

        public SceneSet SceneSet
        {
            get { return sceneSet; }
            set { sceneSet = value; }
        }

        /// <summary>True for a sample scene group: its scenes open in the Editor but never go into a build.</summary>
        public bool IsSample
        {
            get { return isSample; }
            set { isSample = value; }
        }

        /// <summary>The launch scene of a sample scene group.</summary>
        /// <remarks>
        /// - Play started with it open holds it, with this sample scene group as the start scene group (scene system S).
        /// - Read only when <see cref="IsSample"/> is true.
        /// </remarks>
        public SceneReference SampleLaunchScene
        {
            get { return sampleLaunchScene; }
            set { sampleLaunchScene = value; }
        }

        /// <summary>When this group shows its loading scene.</summary>
        public ShowLoadingScene ShowLoadingScene
        {
            get { return showLoadingScene; }
            set { showLoadingScene = value; }
        }

        /// <summary>The loading scene this group picked. Empty means the settings' default loading scene.</summary>
        public SceneReference LoadingScene
        {
            get { return loadingScene; }
            set { loadingScene = value; }
        }

        /// <summary>True when this part keeps the group's own value instead of following its scene set.</summary>
        public bool IsOverride(SceneSetPart _part)
        {
            switch (_part)
            {
                case SceneSetPart.ManagerScenes:
                    return managerScenesOverride;
                case SceneSetPart.SharedScenes:
                    return sharedScenesOverride;
                default:
                    return resetSharedScenesOverride;
            }
        }

        public void SetOverride(SceneSetPart _part, bool _isOverride)
        {
            switch (_part)
            {
                case SceneSetPart.ManagerScenes:
                    managerScenesOverride = _isOverride;
                    break;
                case SceneSetPart.SharedScenes:
                    sharedScenesOverride = _isOverride;
                    break;
                default:
                    resetSharedScenesOverride = _isOverride;
                    break;
            }
        }

        // For Runtime Use for creation
        public void Initialize(IEnumerable<SceneReference> _managerialBootScenesList, IEnumerable<SceneReference> _contextualBootScenesList, SceneReference _mainScene, bool _forceReloadContextual = false)
        {
            ManagerialBootScenesList = new List<SceneReference>(_managerialBootScenesList);
            ContextualBootScenesList = new List<SceneReference>(_contextualBootScenesList);
            MainScene = _mainScene;
            ForceReloadContextualScenes = _forceReloadContextual;
        }

        public bool IsEmpty()
        {
            bool isMainSceneNull = MainScene == null || string.IsNullOrEmpty(MainScene.SceneName);

            bool isManagerialNull = ManagerialBootScenesList == null || ManagerialBootScenesList.Count == 0;
            bool isContextualNull = ContextualBootScenesList == null || ContextualBootScenesList.Count == 0;

            bool isSceneGroupNull = isMainSceneNull && isManagerialNull && isContextualNull;
            return isSceneGroupNull;
        }

        public static implicit operator SceneGroup(SceneGroupData sceneGroupData)
        {
            return sceneGroupData.ToSceneGroup(sceneGroupData);
        }
    }
    
}
