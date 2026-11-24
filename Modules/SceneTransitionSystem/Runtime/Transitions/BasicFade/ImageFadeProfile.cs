using UnityEngine;


namespace AbstractPixel.SceneTransitions
{
    /// <summary>The basic fade's settings: its colour and times.</summary>
    [CreateAssetMenu(fileName = "ImageFadeProfile", menuName = "Utility/SceneRelated/Transitions/ImageFadeProfile", order = 2)]
    public class ImageFadeProfile : TransitionProfile
    {
        [Header("Fade Settings")]
        [Tooltip("The color the screen will fade to.")]
        /// <summary>The colour the screen fades to.</summary>
        public Color FadeColor = Color.black;
        [Tooltip("Seconds to wait before each fade starts (unscaled time). Fractions count.")]
        /// <summary>Seconds to wait before each fade starts, in unscaled time.</summary>
        [Min(0f)] public float StartDelay = 0.5f;
        [Tooltip("How long each fade takes in seconds (unscaled time).")]
        /// <summary>Seconds each fade takes, in unscaled time.</summary>
        [Min(0f)] public float Duration = 1f;
        [Tooltip("The easing curve. X-axis is time (0 to 1), Y-axis is Alpha (0 to 1).")]
        /// <summary>The easing curve: X is time (0 to 1), Y is alpha (0 to 1).</summary>
        public AnimationCurve FadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}
