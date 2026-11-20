using UnityEngine;
using Ami.BroAudio;

namespace AbstractPixel.Core
{
    [RequireComponent(typeof(TMPGlitchTextEffect))]
    public class TMPGlitchTextSoundFeedback : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the target glitch text effect component. Automatically fetched if unassigned.")]
        [SerializeField] private TMPGlitchTextEffect glitchTextEffect;

        [Header("Sound ID Configuration")]
        [Tooltip("Sound triggered when a character is initially spawned/revealed.")]
        [SerializeField] private SoundID characterInitialRevealSound;

        [Tooltip("Sound triggered each time a character randomized symbol changes during glitching.")]
        [SerializeField] private SoundID characterGlitchSound;

        [Tooltip("Sound triggered when an individual character finishes glitching and settles on its target character.")]
        [SerializeField] private SoundID characterSettledSound;

        [Tooltip("Sound triggered when all characters have completed glitching and the text sequence finishes.")]
        [SerializeField] private SoundID effectFinishedSound;

        private void Awake()
        {
            if (glitchTextEffect == null)
            {
                glitchTextEffect = GetComponent<TMPGlitchTextEffect>();
            }
        }

        private void OnEnable()
        {
            if (glitchTextEffect == null)
            {
                glitchTextEffect = GetComponent<TMPGlitchTextEffect>();
            }

            if (glitchTextEffect != null)
            {
                glitchTextEffect.OnCharacterInitialReveal += HandleCharacterInitialReveal;
                glitchTextEffect.OnCharacterGlitch += HandleCharacterGlitch;
                glitchTextEffect.OnCharacterSettled += HandleCharacterSettled;
                glitchTextEffect.OnEffectFinished += HandleEffectFinished;
            }
        }

        private void OnDisable()
        {
            if (glitchTextEffect != null)
            {
                glitchTextEffect.OnCharacterInitialReveal -= HandleCharacterInitialReveal;
                glitchTextEffect.OnCharacterGlitch -= HandleCharacterGlitch;
                glitchTextEffect.OnCharacterSettled -= HandleCharacterSettled;
                glitchTextEffect.OnEffectFinished -= HandleEffectFinished;
            }
        }

        private void HandleCharacterInitialReveal()
        {
            PlaySoundSafely(characterInitialRevealSound);
        }

        private void HandleCharacterGlitch()
        {
            PlaySoundSafely(characterGlitchSound);
        }

        private void HandleCharacterSettled()
        {
            PlaySoundSafely(characterSettledSound);
        }

        private void HandleEffectFinished()
        {
            PlaySoundSafely(effectFinishedSound);
        }

        private void PlaySoundSafely(SoundID _soundID)
        {
            if (_soundID.IsValid())
            {
                BroAudio.Play(_soundID);
            }
        }
    }
}