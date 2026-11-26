using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>The settings of a screen effect.</summary>
    /// <remarks>
    /// - Abstract: make an asset of a profile that derives from it, such as <see cref="ImageFadeProfile"/>.
    /// </remarks>
    public abstract class TransitionProfile : ScriptableObject
    {
        /// <summary>The prefab whose controller plays the effect.</summary>
        /// <remarks>
        /// - It must hold an ITransitionController, or an error is logged and no effect plays.
        /// </remarks>
        [SerializeField] public GameObject TransitionControllerPrefab;

        [Tooltip("Screen counts as covered at: the share of the Hide screen effect after which the scene change goes on (0.5 to 1). The effect keeps finishing on top while scenes change underneath.")]
        /// <summary>The share of Hide screen after which the scene change goes on, 0.5 to 1.</summary>
        [Range(0.5f, 1f)] public float ScreenCountsAsCoveredAt = 1f;

        [Tooltip("Screen counts as visible at: the share of the Show screen effect at which the screen counts as visible enough to release relocatables (0 to 1). The effect keeps finishing on top.")]
        /// <summary>The share of Show screen at which relocatables are released, 0 to 1.</summary>
        [Range(0f, 1f)] public float ScreenCountsAsVisibleAt = 0.75f;
    }
}
