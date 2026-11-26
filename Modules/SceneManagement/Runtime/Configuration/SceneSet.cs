using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The default manager scenes, shared scenes and Reset shared scenes choice for the scene groups that use it.</summary>
    /// <remarks>
    /// - A scene set fills its groups in the Editor (ADR 0008); the loader still reads each group's own lists.
    /// </remarks>
    [CreateAssetMenu(fileName = "SceneSet", menuName = "Utility/SceneRelated/Scene Set", order = 2)]
    public class SceneSet : ScriptableObject
    {
        [SerializeField] private List<SceneReference> managerScenes = new List<SceneReference>();
        [SerializeField] private List<SceneReference> sharedScenes = new List<SceneReference>();
        [SerializeField] private bool resetSharedScenes = false;

        public List<SceneReference> ManagerScenes
        {
            get { return managerScenes; }
        }

        public List<SceneReference> SharedScenes
        {
            get { return sharedScenes; }
        }

        public bool ResetSharedScenes
        {
            get { return resetSharedScenes; }
            set { resetSharedScenes = value; }
        }
    }
}
