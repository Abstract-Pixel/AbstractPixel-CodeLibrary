using UnityEngine;
using UnityEngine.Serialization;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// A one-shot rumble moment (crash, explosion, bomb zap...). Curves: X = 0..1 share of duration, Y = 0..1 strength.
    /// </summary>
    [CreateAssetMenu(fileName = "RumblePattern", menuName = "AbstractPixel/Rumble/Rumble Pattern")]
    public class RumblePattern : ScriptableObject
    {
        [Header("Shape")]
        [Tooltip("How long the moment lasts, in seconds.")]
        [SerializeField, Range(0.02f, 10f)] private float duration = 0.3f;

        [Tooltip("Heavy motor (left grip): deep, strong shake. X = time 0..1 of the duration, Y = strength 0..1.")]
        [FormerlySerializedAs("lowMotor")]
        [SerializeField] private AnimationCurve heavyMotorLeft = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        [Tooltip("Light motor (right grip): fast, buzzy shake. X = time 0..1 of the duration, Y = strength 0..1.")]
        [FormerlySerializedAs("highMotor")]
        [SerializeField] private AnimationCurve lightMotorRight = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 0f));

        [Tooltip("Turns the whole pattern up or down (both motors).")]
        [SerializeField, Range(0f, 1f)] private float strength = 1f;

        [Header("Timing")]
        [Tooltip("On: plays in real time, so pause and slow motion do not stretch it. Leave on.")]
        [SerializeField] private bool useUnscaledTime = true;

        [Tooltip("On: the driving rumble (road, throttle, brake) goes quiet while this plays, so this moment is felt on its own.")]
        [FormerlySerializedAs("ducksContinuous")]
        [SerializeField] private bool muteDrivingRumble = false;

        [Header("PlayStation light bar (optional)")]
        [Tooltip("On: the light bar shows the colour below while this plays (PS4 and PS5 pads).")]
        [FormerlySerializedAs("useLightBarFlash")]
        [SerializeField] private bool flashLightBar = false;

        [Tooltip("Light bar colour while this plays.")]
        [SerializeField] private Color lightBarFlashColor = Color.red;

        [Header("DualSense triggers (optional)")]
        [Tooltip("On: L2 and R2 vibrate under your fingers while this plays (PS5 pad through Steam only).")]
        [FormerlySerializedAs("useTriggerBuzz")]
        [SerializeField] private bool buzzTriggers = false;

        [Tooltip("How strong the trigger vibration is.")]
        [SerializeField, Range(0f, 1f)] private float triggerBuzzAmplitude = 0.5f;

        [Tooltip("How fast the triggers vibrate, in pulses per second. Low = slow thumps, high = fine buzz.")]
        [SerializeField, Range(1f, 255f)] private float triggerBuzzFrequency = 40f;

        public float Duration => duration;
        public bool UseUnscaledTime => useUnscaledTime;
        public bool MutesDrivingRumble => muteDrivingRumble;
        public bool FlashesLightBar => flashLightBar;
        public Color LightBarFlashColor => lightBarFlashColor;
        public bool BuzzesTriggers => buzzTriggers;
        public float TriggerBuzzAmplitude => triggerBuzzAmplitude;
        public float TriggerBuzzFrequency => triggerBuzzFrequency;

        public float EvaluateLow(float _timeShare)
        {
            return Mathf.Clamp01(heavyMotorLeft.Evaluate(_timeShare)) * strength;
        }

        public float EvaluateHigh(float _timeShare)
        {
            return Mathf.Clamp01(lightMotorRight.Evaluate(_timeShare)) * strength;
        }

#if UNITY_EDITOR
        /// <summary>Editor only: RumbleAudioEnvelopeBaker writes curves that follow a sound's loudness.</summary>
        public void SetBakedCurves(AnimationCurve _heavy, AnimationCurve _light, float _duration)
        {
            heavyMotorLeft = _heavy;
            lightMotorRight = _light;
            duration = _duration;
        }
#endif
    }
}
