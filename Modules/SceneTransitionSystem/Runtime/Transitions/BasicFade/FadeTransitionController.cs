using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.SceneTransitions
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class FadeTransitionController : MonoBehaviour, ITransitionController
    {
        [Header("References")]
        [Tooltip("The UI Image used to block the screen. Must stretch to fill the canvas.")]
        [SerializeField] private Image fadeImage;

        private ImageFadeProfile cachedProfile;

        private void Awake()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
            }
            fadeImage.gameObject.SetActive(false);
        }

        public void Initialize(TransitionProfile _profile)
        {
            if (_profile is ImageFadeProfile imageFadeProfile)
            {
                cachedProfile = imageFadeProfile;
            }
            else
            {
                Debug.LogError($"[FadeTransitionController] Profile passed is not an ImageFadeProfile! Found: {_profile.GetType()}");
            }
        }

        public async Task PlayTransitionOut()
        {
            if (cachedProfile == null)
            {
                Debug.LogError("[FadeTransitionController] Cannot play transition. Profile was not initialized.");
                return;
            }

            fadeImage.color = new Color(cachedProfile.FadeColor.r, cachedProfile.FadeColor.g, cachedProfile.FadeColor.b, 1f);
            fadeImage.gameObject.SetActive(true);

            float timer = 0f;
            while (timer < cachedProfile.Duration)
            {
                timer += Time.unscaledDeltaTime;
                float percent = Mathf.Clamp01(timer / cachedProfile.Duration);
                float alpha = cachedProfile.FadeCurve.Evaluate(1-percent);

                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;
                await Task.Yield();
            }

            Color finalColor = fadeImage.color;
            finalColor.a = cachedProfile.FadeCurve.Evaluate(0f);
            fadeImage.color = finalColor;
            fadeImage.gameObject.SetActive(false);
        }

        public async Task PlayTransitionIn()
        {
            if (cachedProfile == null)
            {
                Debug.LogError("[FadeTransitionController] Cannot play transition. Profile was not initialized.");
                return;
            }

            fadeImage.color = new Color(cachedProfile.FadeColor.r, cachedProfile.FadeColor.g, cachedProfile.FadeColor.b, 0f);
            fadeImage.gameObject.SetActive(true);

            float timer = 0f;
            while (timer < cachedProfile.Duration)
            {
                timer += Time.unscaledDeltaTime;
                float percent = Mathf.Clamp01(timer / cachedProfile.Duration);

                float alpha = cachedProfile.FadeCurve.Evaluate(percent);

                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;

                await Task.Yield();
            }

            Color finalColor = fadeImage.color;
            finalColor.a = cachedProfile.FadeCurve.Evaluate(1f);
            fadeImage.color = finalColor;
        }
    }
}