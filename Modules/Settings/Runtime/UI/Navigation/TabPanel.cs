using AbstractPixel.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TabPanel : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)] private CanvasGroup panelCanvasGroup;

        [Header("Tab Panel Feedbacks")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine activeFadeRoutine;

        private void OnValidate()
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void ShowPanel()
        {
            gameObject.SetActive(true);
            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
            activeFadeRoutine = StartCoroutine(FadeRoutine(1.0f, true));
            SetFirstElementSelected();
        }

        public void HidePanel(bool _disableGameObject)
        {
            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
            activeFadeRoutine = StartCoroutine(FadeRoutine(0.0f, false));
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool makeInteractable)
        {
            panelCanvasGroup.interactable = makeInteractable;
            panelCanvasGroup.blocksRaycasts = makeInteractable;

            float startAlpha = panelCanvasGroup.alpha;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                // Normalize time between 0 and 1 for the curve
                float normalizedTime = elapsedTime / fadeDuration;
                float curveValue = fadeCurve.Evaluate(normalizedTime);

                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                yield return null;
            }

            panelCanvasGroup.alpha = targetAlpha;
            activeFadeRoutine = null;
        }

        public void SetFirstElementSelected()
        {
            Selectable firstSelectable = GetComponentInChildren<Selectable>();
            if (firstSelectable != null)
            {
                firstSelectable.Select();
            }
        }
    }
}