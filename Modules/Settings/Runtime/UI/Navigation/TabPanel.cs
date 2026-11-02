using AbstractPixel.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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

        // World-space distance under which two selectables count as one row when picking the first option.
        private const float RowTolerance = 0.01f;

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
            activeFadeRoutine = StartCoroutine(FadeRoutine(1.0f, true, false));
            SetFirstElementSelected();
        }

        public void HidePanel(bool _disableGameObject)
        {
            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
            activeFadeRoutine = StartCoroutine(FadeRoutine(0.0f, false, _disableGameObject));
        }

        public void HidePanelImmediate(bool _disableGameObject)
        {
            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
            panelCanvasGroup.alpha = 0.0f;
            gameObject.SetActive(!_disableGameObject);
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool makeInteractable, bool _disableGameObjectAfterFade)
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
            gameObject.SetActive(!_disableGameObjectAfterFade);
        }

        /// <summary>
        /// Controller only: selects the panel's first option (the top-most, then left-most usable selectable).
        /// With the mouse nothing is selected, so no option shows a stale highlight.
        /// </summary>
        public void SetFirstElementSelected()
        {
            bool isControllerInUse = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            if (isControllerInUse == false)
            {
                return;
            }

            Selectable firstSelectable = FindFirstUsableSelectable();
            EventSystem eventSystem = EventSystem.current;
            if (firstSelectable == null || eventSystem == null)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
        }

        /// <summary>The usable selectable nearest the top of the panel (left-most on a tie).</summary>
        private Selectable FindFirstUsableSelectable()
        {
            Selectable firstSelectable = null;
            Vector3 firstPosition = Vector3.zero;
            Selectable[] selectables = GetComponentsInChildren<Selectable>(false);
            foreach (Selectable selectable in selectables)
            {
                bool isUsable = selectable.gameObject.activeInHierarchy && selectable.IsInteractable();
                if (isUsable == false)
                {
                    continue;
                }

                Vector3 position = selectable.transform.position;
                bool isFirstFound = firstSelectable == null;
                bool isHigher = position.y > firstPosition.y + RowTolerance;
                bool isSameRowFurtherLeft = Mathf.Abs(position.y - firstPosition.y) <= RowTolerance && position.x < firstPosition.x;
                if (isFirstFound || isHigher || isSameRowFurtherLeft)
                {
                    firstSelectable = selectable;
                    firstPosition = position;
                }
            }

            return firstSelectable;
        }
    }
}