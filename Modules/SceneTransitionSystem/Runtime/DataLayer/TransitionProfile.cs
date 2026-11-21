using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    [CreateAssetMenu(fileName = "TransitionProfile", menuName = "Utility/SceneRelated/TransitionProfile", order = 1)]
    public abstract class TransitionProfile : ScriptableObject
    {
        [SerializeField] public GameObject TransitionControllerPrefab;

        [Tooltip("Screen counts as covered at: the share of the Hide screen effect after which the scene change goes on (0.5 to 1). The effect keeps finishing on top while scenes change underneath.")]
        [Range(0.5f, 1f)] public float ScreenCountsAsCoveredAt = 1f;

        [Tooltip("Screen counts as visible at: the share of the Show screen effect at which the screen counts as visible enough to release relocatables (0 to 1). The effect keeps finishing on top.")]
        [Range(0f, 1f)] public float ScreenCountsAsVisibleAt = 0.75f;
    }
}
