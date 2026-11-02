using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Plays a RumblePattern next to a sound: on enable (same frame as a SoundSource with play-on-enable),
    /// from a UnityEvent via Play(), or from code via the static PlayOnce().
    /// </summary>
    public class RumblePatternPlayer : MonoBehaviour
    {
        [Tooltip("The one-shot to play.")]
        [SerializeField] private RumblePattern pattern;

        [Tooltip("Play as soon as this object is enabled (put it on the same object as the sound).")]
        [SerializeField] private bool playOnEnable = true;

        [Tooltip("Strength multiplier for this player.")]
        [SerializeField, Range(0f, 1f)] private float strength = 1f;

        private void OnEnable()
        {
            if (playOnEnable == false)
            {
                return;
            }

            Play();
        }

        /// <summary>For UnityEvents.</summary>
        public void Play()
        {
            PlayOnce(pattern, strength);
        }

        /// <summary>Null-safe: does nothing if the pattern or the RumbleService is missing.</summary>
        public static void PlayOnce(RumblePattern _pattern, float _strength = 1f)
        {
            if (_pattern == null)
            {
                return;
            }

            bool hasService = ServiceLocator.TryGet(out RumbleService rumbleService);
            if (hasService == false || rumbleService == null)
            {
                return;
            }

            rumbleService.Play(_pattern, _strength);
        }
    }
}
