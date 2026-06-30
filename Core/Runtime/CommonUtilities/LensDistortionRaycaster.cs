using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Input
{
    [RequireComponent(typeof(Canvas))]
    [AddComponentMenu("UI/URP Lens Distortion Raycaster (Readable)")]
    public class URPLensDistortionRaycaster : GraphicRaycaster
    {
        [Header("Match These Exactly To URP Volume")]
        [Range(-1f, 1f)]
        [SerializeField] private float intensity = 0.129f;

        [Range(0f, 1f)]
        [SerializeField] private float xMultiplier = 0.791f;

        [Range(0f, 1f)]
        [SerializeField] private float yMultiplier = 0.796f;

        [SerializeField] private Vector2 center = new Vector2(0.5f, 0.5f);

        [Range(0.01f, 5f)]
        [SerializeField] private float scale = 1.34f;

        // --- URP MAGIC NUMBERS DEMYSTIFIED ---
        // URP internally maps the -1.0 to 1.0 intensity slider to a -100 to 100 scale.
        private const float URP_INTENSITY_SCALE = 100f;
        // This is a focal length multiplier URP uses to simulate camera lens curves.
        private const float LENS_FOV_MULTIPLIER = 1.6f;
        // URP caps the maximum distortion bend to 160 degrees so the screen doesn't invert on itself.
        private const float MAX_DISTORTION_DEGREES = 160f;
        // A tiny number used to prevent division-by-zero errors in the math.
        private const float MATH_EPSILON = 1e-4f;
        // The minimum distance the cursor must be from the center to bother distorting.
        private const float MIN_RADIUS_EPSILON = 1e-6f;

        // --- CACHED MATHEMATICS ---
        // These variables hold the "Setup" math. We cache them so we don't recalculate them every frame.
        private float cachedIntensity;
        private float cachedXMultiplier;
        private float cachedYMultiplier;
        private float cachedScale;
        private Vector2 cachedCenter;

        private float precalculatedCurveTheta;
        private float precalculatedCurveSigma;
        private float precalculatedZoomScale;
        private float precalculatedIntensity;
        private Vector2 precalculatedAxis;
        private Vector2 precalculatedCenterOffset;

        protected override void Awake()
        {
            base.Awake();
            ForceRecalculateMathCache();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            ForceRecalculateMathCache();
        }
#endif

        public override void Raycast(PointerEventData _eventData, List<RaycastResult> _resultAppendList)
        {
            // If any values were changed by another script at runtime, recalculate the lens curve.
            if (CheckIfValuesChanged())
            {
                ForceRecalculateMathCache();
            }

            Vector2 originalPosition = _eventData.position;

            try
            {
                // Un-warp the position based on the cached URP math
                _eventData.position = ApplyURPShaderMath(originalPosition);

                // Fire the standard raycast
                base.Raycast(_eventData, _resultAppendList);
            }
            finally
            {
                // CRITICAL: Always restore shared pointer data so other UI/Physics isn't broken
                _eventData.position = originalPosition;
            }
        }

        /// <summary>
        /// Checks if any inspector values were modified since the last check.
        /// </summary>
        private bool CheckIfValuesChanged()
        {
            return intensity != cachedIntensity ||
                   xMultiplier != cachedXMultiplier ||
                   yMultiplier != cachedYMultiplier ||
                   scale != cachedScale ||
                   center != cachedCenter;
        }

        /// <summary>
        /// Executes the heavy trigonometric setup for the URP Lens curve only once, 
        /// saving CPU performance during rapid mouse movements.
        /// </summary>
        private void ForceRecalculateMathCache()
        {
            // 1. Update trackers
            cachedIntensity = intensity;
            cachedXMultiplier = xMultiplier;
            cachedYMultiplier = yMultiplier;
            cachedScale = scale;
            cachedCenter = center;

            // 2. Map the intensity from [-1 to 1] up to [-100 to 100]
            float mappedIntensity = intensity * URP_INTENSITY_SCALE;

            // 3. Prevent division by zero if intensity is exactly 0
            float safeIntensity = Mathf.Max(Mathf.Abs(mappedIntensity), MATH_EPSILON);

            // 4. Calculate the "Field of View" angle for the distortion
            float fieldOfViewAmount = LENS_FOV_MULTIPLIER * safeIntensity;

            // 5. Clamp the angle so it doesn't bend past 160 degrees, and convert to Radians for C# math
            float angleInRadians = Mathf.Deg2Rad * Mathf.Min(MAX_DISTORTION_DEGREES, fieldOfViewAmount);

            // 6. Calculate the base Tangent curve (Sigma) used to push pixels around
            float baseCurveSigma = 2f * Mathf.Tan(angleInRadians * 0.5f);

            // --- SAVE TO PRECALCULATED VARIABLES ---
            // These final values are exactly what the URP shader uses in its DistortUV function.
            precalculatedCenterOffset = new Vector2(center.x - 0.5f, center.y - 0.5f);

            precalculatedAxis = new Vector2(
                Mathf.Max(xMultiplier, MATH_EPSILON),
                Mathf.Max(yMultiplier, MATH_EPSILON)
            );

            // If intensity is negative (Pincushion), we invert the angle. Otherwise (Barrel), use normal angle.
            precalculatedCurveTheta = mappedIntensity >= 0f ? angleInRadians : 1f / angleInRadians;
            precalculatedCurveSigma = baseCurveSigma;
            precalculatedZoomScale = 1f / scale;
            precalculatedIntensity = mappedIntensity;
        }

        /// <summary>
        /// Applies the cached Lens Distortion curve to the raw mouse position.
        /// </summary>
        private Vector2 ApplyURPShaderMath(Vector2 _rawScreenPosition)
        {
            if (Mathf.Approximately(intensity, 0f))
            {
                return _rawScreenPosition;
            }

            // Get screen/canvas dimensions
            Camera raycastCamera = eventCamera;
            float screenWidth = raycastCamera != null ? raycastCamera.pixelWidth : Screen.width;
            float screenHeight = raycastCamera != null ? raycastCamera.pixelHeight : Screen.height;

            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                return _rawScreenPosition;
            }

            // ---------------------------------------------------------
            // STEP 1: CONVERT MOUSE PIXELS TO UV SPACE (0.0 to 1.0)
            // ---------------------------------------------------------
            Vector2 screenCoordinate = new Vector2(
                _rawScreenPosition.x / screenWidth,
                _rawScreenPosition.y / screenHeight
            );

            // ---------------------------------------------------------
            // STEP 2: APPLY THE OVERALL ZOOM/SCALE
            // ---------------------------------------------------------
            // Shift coordinate so center is (0,0)
            Vector2 centeredCoordinate = screenCoordinate - new Vector2(0.5f, 0.5f);

            // Zoom in or out based on the Scale slider
            centeredCoordinate = centeredCoordinate * precalculatedZoomScale;

            // Shift coordinate back to standard (0.0 to 1.0) UV space
            screenCoordinate = centeredCoordinate + new Vector2(0.5f, 0.5f);

            // ---------------------------------------------------------
            // STEP 3: FIND THE PIXEL'S DISTANCE FROM THE CENTER
            // ---------------------------------------------------------
            float distanceFromCenterX = screenCoordinate.x - 0.5f - precalculatedCenterOffset.x;
            float distanceFromCenterY = screenCoordinate.y - 0.5f - precalculatedCenterOffset.y;

            // Apply X and Y multipliers to create the oval/elliptical bounds of the distortion
            Vector2 ellipticalOffset = new Vector2(
                precalculatedAxis.x * distanceFromCenterX,
                precalculatedAxis.y * distanceFromCenterY
            );

            // Get the absolute distance (magnitude) from the center
            float radius = ellipticalOffset.magnitude;

            // ---------------------------------------------------------
            // STEP 4: BEND THE COORDINATE (THE CORE MAGIC)
            // ---------------------------------------------------------
            // Only bend if we are actually slightly away from the exact center point
            if (radius > MIN_RADIUS_EPSILON)
            {
                if (precalculatedIntensity > 0.0f) // Barrel Distortion (Bulging out)
                {
                    // Calculate the angle based on how far we are from the center
                    float warpedAngle = radius * precalculatedCurveTheta;

                    // Use standard Tangent math to push the pixel outward
                    float tangentCurve = Mathf.Tan(warpedAngle);
                    float curveMultiplier = 1f / (radius * precalculatedCurveSigma);

                    float distortionAmount = tangentCurve * curveMultiplier;

                    // Apply the final bend offset back to the coordinate
                    screenCoordinate.x += ellipticalOffset.x * (distortionAmount - 1f);
                    screenCoordinate.y += ellipticalOffset.y * (distortionAmount - 1f);
                }
                else // Pincushion Distortion (Sucking in)
                {
                    // Use Arc Tangent (Atan) math to pull the pixel inward
                    float inwardCurve = Mathf.Atan(radius * precalculatedCurveSigma);
                    float curveMultiplier = (1f / radius) * precalculatedCurveTheta;

                    float distortionAmount = curveMultiplier * inwardCurve;

                    // Apply the final bend offset back to the coordinate
                    screenCoordinate.x += ellipticalOffset.x * (distortionAmount - 1f);
                    screenCoordinate.y += ellipticalOffset.y * (distortionAmount - 1f);
                }
            }

            // ---------------------------------------------------------
            // STEP 5: CONVERT UV SPACE BACK TO MOUSE PIXELS
            // ---------------------------------------------------------
            float finalPixelX = screenCoordinate.x * screenWidth;
            float finalPixelY = screenCoordinate.y * screenHeight;

            return new Vector2(finalPixelX, finalPixelY);
        }
    }
}