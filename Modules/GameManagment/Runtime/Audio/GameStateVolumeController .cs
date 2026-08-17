using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AbstractPixel.GameManagement.Audio
{
    public class GameStateVolumeController : MonoBehaviour
    {
        [Header("State Mappings")]
        [SerializeField] private List<StateVolumeConfig> stateConfigurations = new List<StateVolumeConfig>();

        // Tracks the original, untouched volume before any state modified it
        // Key: (AudioMixer, parameterName)
        private Dictionary<(AudioMixer, string), float> baseVolumes = new Dictionary<(AudioMixer, string), float>();

        // Tracks running fade routines so overlapping fades can be cancelled cleanly
        private Dictionary<(AudioMixer, string), Coroutine> activeFades = new Dictionary<(AudioMixer, string), Coroutine>();

        #region Unity Lifecycle & Event Subscriptions

        private void OnEnable()
        {
            GameStateRegistry.OnStateRegistered += HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
            GameStateRegistry.OnStateRestored += HandleStateRestored;
        }

        private void OnDisable()
        {
            GameStateRegistry.OnStateRegistered -= HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
            GameStateRegistry.OnStateRestored -= HandleStateRestored;

            StopAllCoroutines();
            activeFades.Clear();
            baseVolumes.Clear();
        }

        #endregion

        #region State Event Handlers

        private void HandleStateRegistered(StateSO stateData)
        {
            StateVolumeConfig config = FindConfig(stateData);
            if (config != null)
            {
                ApplyVolumeConfig(config);
            }
        }

        private void HandleStateUnregistered(StateSO stateData)
        {
            StateVolumeConfig config = FindConfig(stateData);
            if (config != null && config.RevertOnUnregister)
            {
                RevertVolumeConfig(config);
            }
        }

        private void HandleStateRestored(StateSO stateData)
        {
            // When returning from Settings back to Pause, re-apply Pause volume
            StateVolumeConfig config = FindConfig(stateData);
            if (config != null)
            {
                ApplyVolumeConfig(config);
            }
        }

        #endregion

        #region Volume Logic

        private void ApplyVolumeConfig(StateVolumeConfig config)
        {
            foreach (MixerVolumeTarget target in config.VolumeTargets)
            {
                if (target.TargetGroup == null) continue;

                AudioMixer mixer = target.TargetGroup.audioMixer;
                string parameter = target.ExposedVolumeParameter;
                var key = (mixer, parameter);

                // 1. Capture the true baseline volume only if we haven't stored it yet
                if (!baseVolumes.ContainsKey(key))
                {
                    if (mixer.GetFloat(parameter, out float originalDb))
                    {
                        baseVolumes[key] = originalDb;
                    }
                    else
                    {
                        continue;
                    }
                }

                // 2. Target is ALWAYS calculated from the BASELINE (never from the live ducked volume)
                float pristineBaseDb = baseVolumes[key];
                float targetDb = CalculateTargetDecibels(pristineBaseDb, target.TargetVolumeMultiplier);

                // 3. Smoothly fade to the target
                StartFade(mixer, parameter, targetDb, target.LerpDuration);
            }
        }

        private void RevertVolumeConfig(StateVolumeConfig config)
        {
            foreach (MixerVolumeTarget target in config.VolumeTargets)
            {
                if (target.TargetGroup == null) continue;

                AudioMixer mixer = target.TargetGroup.audioMixer;
                string parameter = target.ExposedVolumeParameter;
                var key = (mixer, parameter);

                // If we have a saved baseline, fade back to it and clear the baseline
                if (baseVolumes.TryGetValue(key, out float originalDb))
                {
                    StartFade(mixer, parameter, originalDb, target.LerpDuration);
                    baseVolumes.Remove(key);
                }
            }
        }

        #endregion

        #region Fade Engine (Unscaled Time)

        private void StartFade(AudioMixer mixer, string parameter, float targetDb, float duration)
        {
            var key = (mixer, parameter);

            // Stop any existing fade on this specific parameter
            if (activeFades.TryGetValue(key, out Coroutine runningFade) && runningFade != null)
            {
                StopCoroutine(runningFade);
            }

            // Snap immediately if duration is zero
            if (duration <= 0f)
            {
                mixer.SetFloat(parameter, targetDb);
                activeFades.Remove(key);
                return;
            }

            activeFades[key] = StartCoroutine(FadeRoutine(mixer, parameter, targetDb, duration));
        }

        private IEnumerator FadeRoutine(AudioMixer mixer, string parameter, float targetDb, float duration)
        {
            mixer.GetFloat(parameter, out float startDb);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // Works even when paused (Time.timeScale == 0)
                float progress = Mathf.Clamp01(elapsed / duration);

                float currentDb = Mathf.Lerp(startDb, targetDb, progress);
                mixer.SetFloat(parameter, currentDb);

                yield return null;
            }

            mixer.SetFloat(parameter, targetDb);
            activeFades.Remove((mixer, parameter));
        }

        #endregion

        #region Helpers & Audio Math

        private StateVolumeConfig FindConfig(StateSO stateData)
        {
            for (int i = 0; i < stateConfigurations.Count; i++)
            {
                if (stateConfigurations[i].TargetState == stateData)
                {
                    return stateConfigurations[i];
                }
            }
            return null;
        }

        private float CalculateTargetDecibels(float baseDecibels, float multiplier)
        {
            // Convert baseline decibels to linear (0..1)
            float baseLinear = Mathf.Pow(10f, baseDecibels / 20f);

            // Apply multiplier
            float targetLinear = baseLinear * multiplier;

            // Clamp to avoid -Infinity from Log10(0)
            float safeLinear = Mathf.Max(0.0001f, targetLinear);

            // Convert back to decibels
            return 20f * Mathf.Log10(safeLinear);
        }

        #endregion
    }
}