using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Core
{
    [AddComponentMenu("Animation/Cursor Rotation Follower")]
    public class CursorRotationFollower : MonoBehaviour
    {
        

        [Header("General Settings")]
        [Tooltip("The Transform to rotate. If left empty, it defaults to the GameObject this script is attached to.")]
        public Transform TargetTransform;
        [Min(0.1f)]
        public float InterpolationSpeed = 10f;

        [Header("Axis Configuration")]
        [Tooltip("Configure how the X rotation (Pitch) reacts to the pointer.")]
        public RotationAxisConfig AxisX = new RotationAxisConfig { InputSource = ScreenAxis.ScreenY, MinRotation = -30f, MaxRotation = 30f };

        [Tooltip("Configure how the Y rotation (Yaw) reacts to the pointer.")]
        public RotationAxisConfig AxisY = new RotationAxisConfig { InputSource = ScreenAxis.ScreenX, MinRotation = -45f, MaxRotation = 45f };

        [Tooltip("Configure how the Z rotation (Roll) reacts to the pointer.")]
        public RotationAxisConfig AxisZ = new RotationAxisConfig { InputSource = ScreenAxis.None, MinRotation = -15f, MaxRotation = 15f };

        private Transform cachedTransform;
        private Vector3 currentOffsetEuler = Vector3.zero;

        // Tracking variables for the Dynamic Additive Base algorithm (IK Safe)
        private Quaternion cleanBaseRotation;
        private Quaternion expectedAppliedRotation;

        private void Start()
        {
            SetTarget(TargetTransform);
        }

        private void LateUpdate()
        {
            if (cachedTransform == null)
            {
                return;
            }

            DetectExternalAnimationIntervention();

            // Pointer.current handles Physical Mice, Touchscreens, Pens, AND Gamepad Virtual Mice.
            Vector2 pointerPos;
            if (Pointer.current != null)
            {
                pointerPos = Pointer.current.position.ReadValue();
            }
            else
            {
                // Fallback: If no pointing device exists at all, default to the center of the screen.
                pointerPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            // Normalize coordinates (0,0 Bottom-Left to 1,1 Top-Right)
            float screenNormX = Mathf.Clamp01(pointerPos.x / Screen.width);
            float screenNormY = Mathf.Clamp01(pointerPos.y / Screen.height);


            //Calculate the target offset based on the settings and curves
            Vector3 targetOffsetEuler = new Vector3(
                CalculateAxisAngle(AxisX, screenNormX, screenNormY),
                CalculateAxisAngle(AxisY, screenNormX, screenNormY),
                CalculateAxisAngle(AxisZ, screenNormX, screenNormY)
            );

            currentOffsetEuler = Vector3.Lerp(currentOffsetEuler, targetOffsetEuler, Time.fixedDeltaTime * InterpolationSpeed);
            Quaternion offsetQuat = Quaternion.Euler(currentOffsetEuler);

            cachedTransform.localRotation = cleanBaseRotation * offsetQuat;
            expectedAppliedRotation = cachedTransform.localRotation;
        }

        /// <summary>
        /// Public API to dynamically change the target at runtime without breaking IK or snapping rotations.
        /// </summary>
        public void SetTarget(Transform _newTarget)
        {
            cachedTransform = _newTarget != null ? _newTarget : transform;

            // Recalculate baselines for the new target
            cleanBaseRotation = cachedTransform.localRotation;
            expectedAppliedRotation = cleanBaseRotation;

            // Reset the smoothing offset so it doesn't carry over the old target's momentum
            currentOffsetEuler = Vector3.zero;
        }

        private void DetectExternalAnimationIntervention()
        {
            if (Quaternion.Angle(cachedTransform.localRotation, expectedAppliedRotation) > 0.05f)
            {
                cleanBaseRotation = cachedTransform.localRotation;
            }
        }

        private float CalculateAxisAngle(RotationAxisConfig _config, float _screenX, float _screenY)
        {
            if (!_config.IsEnabled || _config.InputSource == ScreenAxis.None)
            {
                return 0f;
            }

            float rawInput = _config.InputSource == ScreenAxis.ScreenX ? _screenX : _screenY;
            if (_config.InvertInput)
            {
                rawInput = 1f - rawInput;
            }
            float curveEvaluated = _config.SensitivityCurve.Evaluate(rawInput);

            return Mathf.Lerp(_config.MinRotation, _config.MaxRotation, curveEvaluated);
        }
    }

    // =========================================================================
    // HELPER DATA STRUCTURES
    // =========================================================================

    [Serializable]
    public class RotationAxisConfig
    {
        public bool IsEnabled = true;
        public bool InvertInput = false;

        [Tooltip("Which screen coordinate drives this rotation axis?")]
        public ScreenAxis InputSource;

        public float MinRotation = -45f;
        public float MaxRotation = 45f;

        [Tooltip("Maps the 0 to 1 screen coordinate. Left side is 0 (Min Rotation), Right side is 1 (Max Rotation).")]
        public AnimationCurve SensitivityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public enum ScreenAxis
    {
        None,
        ScreenX,
        ScreenY
    }
}