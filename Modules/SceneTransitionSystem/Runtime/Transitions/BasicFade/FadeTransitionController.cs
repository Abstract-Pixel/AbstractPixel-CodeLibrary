using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// The ready-made fade: one full-screen image fades to the profile's colour (Hide screen) and back (Show screen),
    /// on unscaled time. Hide screen finishes at the profile's "Screen counts as covered at" share; the fade keeps
    /// finishing on top, and Show screen waits for it first.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class FadeTransitionController : MonoBehaviour, ITransitionController
    {
        [Header("References")]
        [Tooltip("The UI Image used to block the screen. Must stretch to fill the canvas.")]
        [SerializeField] private Image fadeImage;

        private ImageFadeProfile cachedProfile;
        private Task runningFade = Task.CompletedTask;

        private void Awake()
        {
            FindImage();
            fadeImage.gameObject.SetActive(false);
        }

        public void Initialize(TransitionProfile _profile)
        {
            FindImage();
            if (_profile is ImageFadeProfile imageFadeProfile)
            {
                cachedProfile = imageFadeProfile;
            }
            else
            {
                Debug.LogError($"[FadeTransitionController] Profile passed is not an ImageFadeProfile! Found: {_profile.GetType()}");
            }
        }

        public Task PlayHideScreen()
        {
            if (cachedProfile == null)
            {
                Debug.LogError("[FadeTransitionController] Cannot play Hide screen. Profile was not initialized.");
                return Task.CompletedTask;
            }

            runningFade = Fade(true);
            float effectSeconds = cachedProfile.StartDelay + cachedProfile.Duration;
            return TransitionTiming.WhenShareHasPlayed(runningFade, effectSeconds, cachedProfile.ScreenCountsAsCoveredAt);
        }

        public async Task PlayShowScreen()
        {
            if (cachedProfile == null)
            {
                Debug.LogError("[FadeTransitionController] Cannot play Show screen. Profile was not initialized.");
                return;
            }

            // A Hide screen that counted as covered early may still be finishing on top.
            await runningFade;
            runningFade = Fade(false);
            await runningFade;
        }

        private async Task Fade(bool _toCovered)
        {
            float startAlpha = _toCovered ? 0f : 1f;
            SetAlpha(cachedProfile.FadeCurve.Evaluate(startAlpha));
            fadeImage.gameObject.SetActive(true);

            await TransitionTiming.WaitSeconds(cachedProfile.StartDelay);
            float startTime = TransitionTiming.Now;
            float share = 0f;
            while (share < 1f)
            {
                if (fadeImage == null)
                {
                    return;
                }
                share = TransitionTiming.ShareSince(startTime, cachedProfile.Duration);
                float curveTime = _toCovered ? share : 1f - share;
                SetAlpha(cachedProfile.FadeCurve.Evaluate(curveTime));
                if (share < 1f)
                {
                    await Task.Yield();
                }
            }

            if (!_toCovered && fadeImage != null)
            {
                fadeImage.gameObject.SetActive(false);
            }
        }

        private void SetAlpha(float _alpha)
        {
            if (fadeImage == null)
            {
                return;
            }
            Color color = cachedProfile.FadeColor;
            color.a = _alpha;
            fadeImage.color = color;
        }

        private void FindImage()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
            }
        }
    }
}
