using System.Collections;
using UnityEngine;

namespace AbstractPixel.Core
{
    public class LerpedIntSOTextDisplayer : IntSOTextDisplayer
    {
        [Header("AnimationSettings")]
        [SerializeField] AnimationCurve lerpCurve;
        [SerializeField] float lerpDuration = 1f;
        [SerializeField] int maximumExpectedChange;

        int currentBackingValue;
        int targetValue;
        Coroutine activeLerpCoroutine;

        protected override void OnEnable()
        {
            currentBackingValue = intSo.CurrentValue;
        }
        public override void UpdateDisplayText()
        {
            if(displayText == null || intSo == null)
            {
                return;
            }
            targetValue = intSo.CurrentValue;
            if(activeLerpCoroutine != null )
            {
                StopCoroutine(activeLerpCoroutine);
                activeLerpCoroutine = null;
                activeLerpCoroutine = StartCoroutine(StartLerpProcess());
                return;
            }
            activeLerpCoroutine = StartCoroutine(StartLerpProcess());
        }

        private IEnumerator StartLerpProcess()
        {
            int startValue = currentBackingValue;
            float distanceToTarget = Mathf.Abs(targetValue - startValue);
            
            float distanceRatio = Mathf.Clamp01(distanceToTarget / maximumExpectedChange);
            float adjustedDuration = lerpDuration * distanceRatio;

            float elapsedTime = 0f;

            while (elapsedTime < adjustedDuration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = Mathf.Clamp01(elapsedTime / adjustedDuration);
                float curveValue = lerpCurve.Evaluate(percentage);
                float lerpValue = Mathf.Lerp(startValue, targetValue, curveValue);
                currentBackingValue = Mathf.RoundToInt(lerpValue);
                ShowTextWithFormatting(currentBackingValue);
                yield return null;
            }
            currentBackingValue = targetValue;
            ShowTextWithFormatting(currentBackingValue);
            activeLerpCoroutine = null;

        }
    }
}
