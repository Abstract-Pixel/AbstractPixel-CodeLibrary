using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AbstractPixel.GameManagement.Audio
{
    public class GameStateVolumeController : MonoBehaviour
    {
        [Header("State Mappings")]
        [SerializeField] private List<StateVolumeConfig> stateConfigurations = new List<StateVolumeConfig>();

        // Key: Tuple of the specific AudioMixer AND the string parameter. 
        private Dictionary<(AudioMixer, string), VolumeTransition> activeTransitions = new Dictionary<(AudioMixer, string), VolumeTransition>();

        // Caching lists to prevent Garbage Collection allocations during Update
        private List<(AudioMixer, string)> keysToProcess = new List<(AudioMixer, string)>();
        private List<(AudioMixer, string)> completedTransitions = new List<(AudioMixer, string)>();

        private struct VolumeTransition
        {
            public AudioMixer TargetMixer;
            public string ParameterName;
            public float StartVolumeDecibels;
            public float TargetVolumeDecibels;
            public float ElapsedTime;
            public float Duration;
        }

        private void OnEnable()
        {
            GameStateRegistry.OnStateRegistered += HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
        }

        private void OnDisable()
        {
            GameStateRegistry.OnStateRegistered -= HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
        }

        private void Update()
        {
            if (activeTransitions.Count == 0) return;

            ProcessActiveTransitions();
            CleanupCompletedTransitions();
        }

        private void ProcessActiveTransitions()
        {
            completedTransitions.Clear();
            keysToProcess.Clear();

            //Extract keys safely to prevent "Collection was modified" exceptions
            foreach ((AudioMixer, string) key in activeTransitions.Keys)
            {
                keysToProcess.Add(key);
            }

            foreach ((AudioMixer, string) key in keysToProcess)
            {
                VolumeTransition transitionData = activeTransitions[key];

                transitionData.ElapsedTime += Time.unscaledDeltaTime;

                float progress = transitionData.ElapsedTime / transitionData.Duration;
                float clampedProgress = Mathf.Clamp01(progress);

                float newVolumeDecibels = Mathf.Lerp(transitionData.StartVolumeDecibels, transitionData.TargetVolumeDecibels, clampedProgress);

                transitionData.TargetMixer.SetFloat(transitionData.ParameterName, newVolumeDecibels);

                if (clampedProgress >= 1f)
                {
                    completedTransitions.Add(key);
                }
                else
                {
                    activeTransitions[key] = transitionData;
                }
            }
        }

        private void CleanupCompletedTransitions()
        {
            foreach ((AudioMixer, string) completedKey in completedTransitions)
            {
                activeTransitions.Remove(completedKey);
            }
        }

        private void HandleStateRegistered(StateSO _stateData)
        {
            StateVolumeConfig activeConfig = FindConfigForState(_stateData);
            if (activeConfig != null)
            {
                ApplyVolumeConfig(activeConfig);
            }
        }

        private void HandleStateUnregistered(StateSO _stateData)
        {
            StateVolumeConfig activeConfig = FindConfigForState(_stateData);
            if (activeConfig != null && activeConfig.RevertOnUnregister)
            {
                RevertVolumeConfig(activeConfig);
            }
        }


        private void ApplyVolumeConfig(StateVolumeConfig _config)
        {
            foreach (MixerVolumeTarget volumeTarget in _config.VolumeTargets)
            {
                if (volumeTarget.TargetGroup == null) continue;

                AudioMixer targetMixer = volumeTarget.TargetGroup.audioMixer;
                string parameterName = volumeTarget.ExposedVolumeParameter;
                (AudioMixer, string) compositeKey = (targetMixer, parameterName);

                if (!targetMixer.GetFloat(parameterName, out float currentDecibels)) continue;

                if (!_config.CachedOriginalVolumes.ContainsKey(compositeKey))
                {
                    _config.CachedOriginalVolumes[compositeKey] = currentDecibels;
                }

                float targetDecibels = CalculateTargetDecibels(currentDecibels, volumeTarget.TargetVolumeMultiplier);

                StartTransition(targetMixer, parameterName, currentDecibels, targetDecibels, volumeTarget.LerpDuration);
            }
        }

        private void RevertVolumeConfig(StateVolumeConfig _config)
        {
            foreach (MixerVolumeTarget volumeTarget in _config.VolumeTargets)
            {
                if (volumeTarget.TargetGroup == null) continue;

                AudioMixer targetMixer = volumeTarget.TargetGroup.audioMixer;
                string parameterName = volumeTarget.ExposedVolumeParameter;
                (AudioMixer, string) compositeKey = (targetMixer, parameterName);

                if (_config.CachedOriginalVolumes.TryGetValue(compositeKey, out float cachedOriginalDecibels))
                {
                    if (targetMixer.GetFloat(parameterName, out float currentDecibels))
                    {
                        StartTransition(targetMixer, parameterName, currentDecibels, cachedOriginalDecibels, volumeTarget.LerpDuration);
                    }

                    _config.CachedOriginalVolumes.Remove(compositeKey);
                }
            }
        }

        // ====================================================================
        // UTILITIES & AUDIO MATH
        // ====================================================================

        private StateVolumeConfig FindConfigForState(StateSO _stateData)
        {
            foreach (StateVolumeConfig config in stateConfigurations)
            {
                if (config.TargetState == _stateData) return config;
            }
            return null;
        }

        private void StartTransition(AudioMixer _mixer, string _parameterName, float _startDb, float _targetDb, float _duration)
        {
            (AudioMixer, string) compositeKey = (_mixer, _parameterName);

            if (_duration <= 0f)
            {
                _mixer.SetFloat(_parameterName, _targetDb);
                activeTransitions.Remove(compositeKey);
                return;
            }

            VolumeTransition newTransition = new VolumeTransition
            {
                TargetMixer = _mixer,
                ParameterName = _parameterName,
                StartVolumeDecibels = _startDb,
                TargetVolumeDecibels = _targetDb,
                ElapsedTime = 0f,
                Duration = _duration
            };

            activeTransitions[compositeKey] = newTransition;
        }

        /// <summary>
        /// Converts the current Decibels to a Linear format, applies the multiplier, and converts back to Decibels.
        /// </summary>
        private float CalculateTargetDecibels(float _currentDecibels, float _multiplier)
        {
            // Convert current DB to a linear 0 to 1 scale
            float currentLinear = Mathf.Pow(10f, _currentDecibels / 20f);

            // Apply multiplier
            float targetLinear = currentLinear * _multiplier;

            // Clamp to a tiny fraction to prevent Mathf.Log10(0) returning Negative Infinity
            float clampedLinear = Mathf.Max(0.0001f, targetLinear);

            // Convert back to Decibels
            return 20f * Mathf.Log10(clampedLinear);
        }
    }
}