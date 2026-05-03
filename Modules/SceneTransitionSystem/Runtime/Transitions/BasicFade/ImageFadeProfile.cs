using UnityEngine;


namespace AbstractPixel.SceneTransitions
{
    [CreateAssetMenu(fileName = "ImageFadeProfile", menuName = "Utility/SceneRelated/Transitions/ImageFadeProfile", order = 2)]
    public class ImageFadeProfile : TransitionProfile
    {
        [Header("Fade Settings")]
        [Tooltip("The color the screen will fade to.")]
        public Color FadeColor = Color.black; [Tooltip("How long the transition takes in seconds (Unscaled Time).")]
        public float StartDelay = 0.5f;
        public float Duration = 1f;
        [Tooltip("The easing curve. X-axis is time (0 to 1), Y-axis is Alpha (0 to 1).")]
        public AnimationCurve FadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}
