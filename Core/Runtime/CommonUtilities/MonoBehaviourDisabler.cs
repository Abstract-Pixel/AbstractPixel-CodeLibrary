using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    public class MonoBehaviourDisabler : MonoBehaviour
    {
        [Header("Target Configuration")]
        [SerializeField, Tooltip("The list of MonoBehaviours that will be disabled.")]
        private List<MonoBehaviour> targetBehaviors = new List<MonoBehaviour>();

        [Header("Timing Settings")]
        [SerializeField, Min(0f)]
        private float initialDelay = 0f;

        [SerializeField, Tooltip("If true, the delay ignores Time.timeScale and compensates for lag spikes/breakpoints. Crucial if syncing with Audio.")]
        private bool useUnscaledTime = false;

        [Header("Initialization")]
        [SerializeField]
        private bool playOnAwake = false;

        private Coroutine activeDisableRoutine;

        private void Start()
        {
            if (playOnAwake)
            {
                BeginDisableSequence();
            }
        }

        /// <summary>
        /// Public API to be called via UnityEvents (Buttons, Timelines) or other C# scripts.
        /// Starts the sequence with the delay configured in the Inspector.
        /// </summary>
        public void BeginDisableSequence()
        {
            if (activeDisableRoutine != null)
            {
                StopCoroutine(activeDisableRoutine);
            }

            activeDisableRoutine = StartCoroutine(DisableRoutine(initialDelay));
        }

        /// <summary>
        /// Public API to bypass the Inspector delay and disable immediately.
        /// </summary>
        public void DisableImmediate()
        {
            if (activeDisableRoutine != null)
            {
                StopCoroutine(activeDisableRoutine);
                activeDisableRoutine = null;
            }

            ExecuteDisable();
        }

        private IEnumerator DisableRoutine(float _delay)
        {
            if (_delay > 0f)
            {
                // A manual yield return null loop is fundamentally more robust than WaitForSeconds
                // when dealing with edge-case desynchronization, frame-drops, and debugging.
                float elapsed = 0f;

                while (elapsed < _delay)
                {
                    // Unscaled time forces the timer to catch up based on absolute real-world time passed.
                    // Scaled time (deltaTime) respects game logic, slow-mo, and pauses.
                    elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    yield return null;
                }
            }

            ExecuteDisable();
            activeDisableRoutine = null;
        }

        private void ExecuteDisable()
        {
            for (int i = 0; i < targetBehaviors.Count; i++)
            {
                MonoBehaviour target = targetBehaviors[i];

                if (target != null)
                {
                    target.enabled = false;
                }
            }
        }
    }
}