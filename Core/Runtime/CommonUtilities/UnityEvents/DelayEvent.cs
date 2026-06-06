using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.Core
{
    public enum EventInitializationMode
    {
        Manual,
        Awake,
        Start,
        OnEnable
    }

    [AddComponentMenu("Events/Delay Event")]
    public class DelayEvent : MonoBehaviour
    {
        [Header("Timing Settings")]
        [Min(0f)]
        public float DelayDuration = 2f;

        [Tooltip("If true, the delay will loop infinitely, firing the event every X seconds.")]
        public bool CanRepeat = false;

        [Tooltip("If true, the timer will ignore Time.timeScale (game pauses).")]
        public bool UseUnscaledTime = false;

        [Header("Execution Settings")]
        public EventInitializationMode InitializationMode = EventInitializationMode.Start;

        [Header("Events")]
        [Tooltip("Assign front-end Editor events here.")]
        public UnityEvent OnEventTriggered;

        // Back-end code hook for C# scripts
        public event Action OnDelayEventTriggered;

        private Coroutine activeDelayRoutine;

        private void Awake()
        {
            if (InitializationMode == EventInitializationMode.Awake)
            {
                BeginDelay();
            }
        }

        private void Start()
        {
            if (InitializationMode == EventInitializationMode.Start)
            {
                BeginDelay();
            }
        }

        private void OnEnable()
        {
            if (InitializationMode == EventInitializationMode.OnEnable)
            {
                BeginDelay();
            }
        }

        private void OnDisable()
        {
            StopDelay();
        }

        /// <summary>
        /// Public API to start the delay sequence manually.
        /// </summary>
        public void BeginDelay()
        {
            StopDelay();
            activeDelayRoutine = StartCoroutine(DelaySequenceRoutine());
        }

        /// <summary>
        /// Public API to cancel the delay before it triggers.
        /// </summary>
        public void StopDelay()
        {
            if (activeDelayRoutine != null)
            {
                StopCoroutine(activeDelayRoutine);
                activeDelayRoutine = null;
            }
        }

        private IEnumerator DelaySequenceRoutine()
        {
            do
            {
                float elapsedTime = 0f;

                // Manual accumulation loop is used instead of WaitForSeconds to guarantee immunity 
                // to Editor breakpoints and extreme lag spikes.
                while (elapsedTime < DelayDuration)
                {
                    yield return null;
                    elapsedTime += UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                }

                OnEventTriggered?.Invoke();
                OnDelayEventTriggered?.Invoke();

            } while (CanRepeat);

            activeDelayRoutine = null;
        }
    }
}