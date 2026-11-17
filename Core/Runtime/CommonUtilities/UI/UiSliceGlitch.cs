using UnityEngine;

namespace AbstractPixel.Core.UI
{
    /// <summary>
    /// Glitch-shake for a UI logo: the whole graphic jitters and its horizontal slices shift sideways, in choppy steps.
    /// Animate Strength (Timeline, Animator): 0 is perfectly at rest, 1 is the full shake. Ease it to 0 to settle.
    /// Each slice is a RectTransform whose content is offset on X. Typically each slice is a RectMask2D band that holds a
    /// copy of the graphic, so together the bands show the graphic whole while Strength is 0.
    /// The offsets come from a stepped hash of the clock, so nothing is allocated and nothing accumulates.
    /// </summary>
    [ExecuteAlways]
    public sealed class UiSliceGlitch : MonoBehaviour
    {
        [Tooltip("0 = at rest, 1 = full shake.")]
        [SerializeField, Range(0f, 1f)] private float strength = 0f;

        [Tooltip("Moved as a whole by the positional jitter.")]
        [SerializeField] private RectTransform jitterTarget;

        [Tooltip("Shifted sideways one by one (the content inside each slice band).")]
        [SerializeField] private RectTransform[] sliceContents = new RectTransform[0];

        [Tooltip("Largest whole-graphic jitter at full strength, in canvas units.")]
        [SerializeField] private Vector2 maxJitter = new Vector2(14f, 6f);

        [Tooltip("Largest sideways slice shift at full strength, in canvas units.")]
        [SerializeField] private float maxSliceShift = 36f;

        [Tooltip("How many new offsets per second. Low values read as choppy digital glitches.")]
        [SerializeField, Min(1f)] private float stepsPerSecond = 30f;

        private Vector2 jitterRestPosition;
        private Vector2[] sliceRestPositions = new Vector2[0];
        private bool hasRestPositions;
        private int appliedStep = -1;
        private float appliedStrength = -1f;

        public float Strength
        {
            get { return strength; }
            set { strength = Mathf.Clamp01(value); }
        }

        private void OnEnable()
        {
            CaptureRestPositions();
            appliedStep = -1;
        }

        private void OnDisable()
        {
            RestoreRestPositions();
        }

        // The slice list changed in the Inspector: the graphic is at rest, so its rest positions can be taken again.
        private void OnValidate()
        {
            bool sliceCountChanged = sliceRestPositions.Length != sliceContents.Length;
            if (sliceCountChanged && strength <= 0f)
            {
                CaptureRestPositions();
            }
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void OnDidApplyAnimationProperties()
        {
            Apply();
        }

        private void Apply()
        {
            if (hasRestPositions == false)
            {
                return;
            }

            int step = Mathf.FloorToInt(Time.realtimeSinceStartup * stepsPerSecond);
            bool isUnchanged = step == appliedStep && Mathf.Approximately(strength, appliedStrength);
            if (isUnchanged)
            {
                return;
            }

            appliedStep = step;
            appliedStrength = strength;

            if (strength <= 0f)
            {
                RestoreRestPositions();
                return;
            }

            ApplyJitter(step);
            ApplySliceShifts(step);
        }

        private void ApplyJitter(int _step)
        {
            if (jitterTarget == null)
            {
                return;
            }

            float jitterX = SignedHash(_step, 11) * maxJitter.x * strength;
            float jitterY = SignedHash(_step, 23) * maxJitter.y * strength;
            jitterTarget.anchoredPosition = jitterRestPosition + new Vector2(jitterX, jitterY);
        }

        private void ApplySliceShifts(int _step)
        {
            int sliceCount = Mathf.Min(sliceContents.Length, sliceRestPositions.Length);
            for (int sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++)
            {
                RectTransform slice = sliceContents[sliceIndex];
                if (slice == null)
                {
                    continue;
                }

                // Only some slices tear on each step, which reads more like a signal glitch than a uniform shake.
                float tearChance = SignedHash(_step, 101 + sliceIndex * 7);
                bool isTearing = tearChance > -0.2f;
                float shift = 0f;
                if (isTearing)
                {
                    shift = SignedHash(_step, 53 + sliceIndex * 13) * maxSliceShift * strength;
                }

                slice.anchoredPosition = sliceRestPositions[sliceIndex] + new Vector2(shift, 0f);
            }
        }

        private void CaptureRestPositions()
        {
            if (jitterTarget != null)
            {
                jitterRestPosition = jitterTarget.anchoredPosition;
            }

            sliceRestPositions = new Vector2[sliceContents.Length];
            for (int sliceIndex = 0; sliceIndex < sliceContents.Length; sliceIndex++)
            {
                RectTransform slice = sliceContents[sliceIndex];
                if (slice != null)
                {
                    sliceRestPositions[sliceIndex] = slice.anchoredPosition;
                }
            }

            hasRestPositions = true;
        }

        private void RestoreRestPositions()
        {
            if (hasRestPositions == false)
            {
                return;
            }

            if (jitterTarget != null)
            {
                jitterTarget.anchoredPosition = jitterRestPosition;
            }

            int sliceCount = Mathf.Min(sliceContents.Length, sliceRestPositions.Length);
            for (int sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++)
            {
                RectTransform slice = sliceContents[sliceIndex];
                if (slice != null)
                {
                    slice.anchoredPosition = sliceRestPositions[sliceIndex];
                }
            }
        }

        // A cheap integer hash mapped to -1..1, the same for the same step and salt.
        private static float SignedHash(int _step, int _salt)
        {
            unchecked
            {
                uint value = (uint)(_step * 374761393 + _salt * 668265263);
                value = (value ^ (value >> 13)) * 1274126177u;
                value ^= value >> 16;
                float unit = (value & 0xFFFF) / 65535f;
                return unit * 2f - 1f;
            }
        }
    }
}
