using UnityEngine;
using TMPro;
using System.Text;
using System;
using Random = UnityEngine.Random;

namespace AbstractPixel.Core
{
    [RequireComponent(typeof(TMP_Text))]
    public partial class TMPGlitchTextEffect : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)] TMP_Text referencedTmpText;
        [SerializeField] private bool playOnEnable = true;

        [Header("Reveal Settings")]
        [Tooltip("MinimumTime in seconds to wait before revealing the next character.")]
        [SerializeField] private float minTimePerCharacterReveal = 0.05f;
        [Tooltip("MinimumTime in seconds to wait before revealing the next character.")]
        [SerializeField] private float maxTimePerCharacterReveal = 0.1f;

        [Header("Glitch Settings")]
        [Tooltip("Maximum time a character will glitch before settling on the correct letter.")]
        [SerializeField] private float maxGlitchDuration = 0.8f;

        [Tooltip("Minimum time between character changes while it is glitching.")]
        [SerializeField] private float minCharacterChangeDuration = 0.02f;
        [Tooltip("Maximum time between character changes while it is glitching.")]
        [SerializeField] private float maxCharacterChangeDuration = 0.08f;

        [Tooltip("The pool of characters it will randomly pick from while glitching.")]
        [SerializeField] private string glitchCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";

        // Internal state variables
        private GlitchCharacterState[] characterStates;
        private string originalText;
        private int currentCharacterRevealIndex;
        private float timeUntilNextReveal;
        private bool isEffectFinished;
        private bool hasEffectStarted;
        // We use a StringBuilder to efficiently rebuild the string every frame without creating garbage
        private StringBuilder textBuilder;

        // Life Cycle Events
        public Action OnCharacterInitialReveal = delegate { };
        public Action OnCharacterGlitch = delegate { };
        public Action OnCharacterSettled = delegate { };
        public Action OnEffectFinished = delegate { };

        private void OnEnable()
        {
            if (!playOnEnable)
            {
                return;
            }
            StartEffect();
        }
        private void OnDisable()
        {
            OnCharacterInitialReveal = delegate { };
            OnCharacterGlitch = delegate { };
            OnCharacterSettled = delegate { };
            OnEffectFinished = delegate { };
        }
        private void OnValidate()
        {
            if (referencedTmpText == null)
            {
                referencedTmpText = GetComponent<TMP_Text>();
            }
        }
     
        /// <summary>
        /// Initializes the effect, reads the current text, and sets up the character states.
        /// </summary>
        public void StartEffect()
        {
            if(string.IsNullOrEmpty(originalText) )
            {
                originalText = referencedTmpText.text;
            }
           
            referencedTmpText.text = "";
            textBuilder = new StringBuilder();
            // Create our array to hold the state for each character
            characterStates = new GlitchCharacterState[originalText.Length];

            for (int i = 0; i < originalText.Length; i++)
            {
                GlitchCharacterState newCharacterState = new GlitchCharacterState();
                newCharacterState.TargetCharacter = originalText[i];
                newCharacterState.IsRevealed = false;
                newCharacterState.IsSettledToTargetChar = false;
                characterStates[i] = newCharacterState;
            }
            // 4. Reset our timers and indices
            currentCharacterRevealIndex = 0;
            timeUntilNextReveal = Random.Range(minTimePerCharacterReveal, maxTimePerCharacterReveal);
            isEffectFinished = false;
            hasEffectStarted = true;
        }

        private void Update()
        {
            if (isEffectFinished == true)
            {
                return;
            }
            if (hasEffectStarted)
            {
                HandleInitialCharacterReveal();
                ProcessGlitchingForRevealedCharacters();
                UpdateDisplayedText();
            }
        }

        /// <summary>
        /// Checks if it is time to spawn the next character in the sequence.
        /// </summary>
        private void HandleInitialCharacterReveal()
        {
            // If we have already revealed all characters, do nothing
            if (currentCharacterRevealIndex >= originalText.Length)
            {
                return;
            }

            timeUntilNextReveal -= Time.unscaledDeltaTime;

            if (timeUntilNextReveal <= 0f)
            {
                // Get the current character we want to reveal
                GlitchCharacterState charState = characterStates[currentCharacterRevealIndex];
                charState.IsRevealed = true;
                OnCharacterInitialReveal?.Invoke();

                // If the character is a space or line break, we don't want it to glitch.
                // We just settle it immediately so it looks natural.
                if (charState.TargetCharacter == ' ' || charState.TargetCharacter == '\n')
                {
                    charState.IsSettledToTargetChar = true;
                    charState.CurrentDisplayCharacter = charState.TargetCharacter;
                }
                else
                {
                    // Otherwise, start the glitching process for this character
                    charState.IsSettledToTargetChar = false;
                    charState.TimeUntilSettled = maxGlitchDuration;

                    charState.CurrentDisplayCharacter = GetRandomGlitchCharacter();
                    charState.TimeUntilNextRandomChange = GetRandomChangeInterval();
                }

                currentCharacterRevealIndex++;
                timeUntilNextReveal = Random.Range(minTimePerCharacterReveal, maxTimePerCharacterReveal);
            }
        }

        /// <summary>
        /// Loops through all revealed characters and updates their glitching states.
        /// </summary>
        private void ProcessGlitchingForRevealedCharacters()
        {
            bool areAllCharactersSettled = true;

            for (int i = 0; i < characterStates.Length; i++)
            {
                GlitchCharacterState charState = characterStates[i];

                if (charState.IsRevealed == false)
                {
                    areAllCharactersSettled = false;
                    continue;
                }

                if (charState.IsSettledToTargetChar == true)
                {
                    continue;
                }

                // If we reach here, the character is currently glitching
                areAllCharactersSettled = false;

                charState.TimeUntilSettled -= Time.unscaledDeltaTime;

                if (charState.TimeUntilSettled <= 0f)
                {
                    // The glitch duration is over, lock it to the final correct character
                    charState.IsSettledToTargetChar = true;
                    charState.CurrentDisplayCharacter = charState.TargetCharacter;
                    OnCharacterSettled?.Invoke();
                }
                else
                {
                    // 2. It is still glitching, so check if it is time to change the random letter
                    charState.TimeUntilNextRandomChange -= Time.unscaledDeltaTime;
                    if (charState.TimeUntilNextRandomChange <= 0f)
                    {
                        charState.CurrentDisplayCharacter = GetRandomGlitchCharacter();
                        charState.TimeUntilNextRandomChange = GetRandomChangeInterval();
                        OnCharacterGlitch?.Invoke();
                    }
                }
            }

            if (areAllCharactersSettled == true && currentCharacterRevealIndex >= originalText.Length)
            {
                isEffectFinished = true;
                OnEffectFinished?.Invoke();
            }
        }

        /// <summary>
        /// Rebuilds the final string based on the current state of all characters and pushes it to TextMeshPro.
        /// </summary>
        private void UpdateDisplayedText()
        {
            // Clear the text builder from the previous frame
            textBuilder.Clear();

            for (int i = 0; i < characterStates.Length; i++)
            {
                GlitchCharacterState charState = characterStates[i];

                if (charState.IsRevealed == true)
                {
                    textBuilder.Append(charState.CurrentDisplayCharacter);
                }
            }
            referencedTmpText.text = textBuilder.ToString();
        }

        // --- Helper Methods --- //
        private float GetRandomChangeInterval()
        {
            return Random.Range(minCharacterChangeDuration, maxCharacterChangeDuration);
        }

        private char GetRandomGlitchCharacter()
        {
            int randomIndex = Random.Range(0, glitchCharacters.Length);
            return glitchCharacters[randomIndex];
        }
    }
}
