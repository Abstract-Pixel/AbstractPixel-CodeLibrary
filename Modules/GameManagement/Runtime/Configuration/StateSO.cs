using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    [CreateAssetMenu(fileName = "StateSO", menuName = "Abstract Pixel/Game Management/State")]
    public class StateSO : ScriptableObject
    {
        [Tooltip("The delay in seconds before the state is executed.")]
        public float StateExecutionDelay = 0f;
        [Tooltip("If true, the state leaves when the active scene changes.")]
        public bool DisableStateOnSceneChange = true;

        [Tooltip("The states this sub-state may open over. Empty means a main state.")]
        public List<StateSO> Parents = new List<StateSO>();
        [Tooltip("What this state controls while on top. Each effect can follow the parent.")]
        public StateEffectSettings Effects = new StateEffectSettings();
    }
}