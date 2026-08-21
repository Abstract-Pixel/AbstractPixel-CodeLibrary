using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Ami.BroAudio;

namespace AbstractPixel.GameManagement.Audio
{
    public class GameStateVolumeController : MonoBehaviour
    {
        [Header("State Mappings")]
        [SerializeField] private List<StateVolumeConfig> stateConfigurations = new List<StateVolumeConfig>();

        private Dictionary<AudioChannelIdentifier, ChannelStateTracker> activeChannels = new Dictionary<AudioChannelIdentifier, ChannelStateTracker>();

        // Substate Preservation Tracking
        private HashSet<StateSO> pendingUnregisters = new HashSet<StateSO>();
        private HashSet<StateSO> preservedParentStates = new HashSet<StateSO>();
        private bool isUnregisterRoutineRunning = false;

        private const float EXTERNAL_MUTATION_TOLERANCE = 0.01f;
        private const float MINIMUM_LINEAR_VOLUME = 0.0001f;
        private const float INSTANT_SNAP_SPEED = 1000f;

        #region Internal Data Structures

        private struct AudioChannelIdentifier : IEquatable<AudioChannelIdentifier>
        {
            public AudioMixer Mixer;
            public string Parameter;
            public BroAudioType BroType;

            public AudioChannelIdentifier(AudioMixer _mixer, string _parameter, BroAudioType _broType)
            {
                Mixer = _mixer;
                Parameter = _parameter;
                BroType = _broType;
            }

            public bool Equals(AudioChannelIdentifier _other)
            {
                return Mixer == _other.Mixer && Parameter == _other.Parameter && BroType == _other.BroType;
            }

            public override bool Equals(object _obj)
            {
                return _obj is AudioChannelIdentifier other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (Mixer != null ? Mixer.GetHashCode() : 0);
                    hash = hash * 23 + (Parameter != null ? Parameter.GetHashCode() : 0);
                    hash = hash * 23 + BroType.GetHashCode();
                    return hash;
                }
            }
        }

        private class ChannelStateTracker
        {
            public Dictionary<StateSO, MixerVolumeTarget> ActiveStates = new Dictionary<StateSO, MixerVolumeTarget>();
            public Coroutine MonitorRoutine;
            public float CurrentAppliedMultiplier = 1f;
            public float ExpectedLinearVolume = -1f;
        }

        #endregion

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
            isUnregisterRoutineRunning = false;

            foreach (KeyValuePair<AudioChannelIdentifier, ChannelStateTracker> keyValuePair in activeChannels)
            {
                AudioChannelIdentifier channel = keyValuePair.Key;
                ChannelStateTracker tracker = keyValuePair.Value;

                float baselineLinear = 1f;
                if (tracker.CurrentAppliedMultiplier > MINIMUM_LINEAR_VOLUME)
                {
                    baselineLinear = tracker.ExpectedLinearVolume / tracker.CurrentAppliedMultiplier;
                }

                ApplyVolume(channel.Mixer, channel.Parameter, channel.BroType, baselineLinear);
            }

            activeChannels.Clear();
            pendingUnregisters.Clear();
            preservedParentStates.Clear();
        }

        #endregion

        #region State Event Handlers & Substate Logic

        private void HandleStateRegistered(StateSO _stateData)
        {
            if (_stateData == null) return;

            if (_stateData.IsSubState)
            {
                // The Registry pushes the highest priority active state to history before registering a substate.
                // We perfectly mirror that logic here by finding the highest priority state in our unregister buffer
                // and locking it in as a preserved parent state.
                StateSO highestPriorityPending = null;
                int maxPriority = -1;

                foreach (StateSO pendingState in pendingUnregisters)
                {
                    if (pendingState.Priority > maxPriority)
                    {
                        maxPriority = pendingState.Priority;
                        highestPriorityPending = pendingState;
                    }
                }

                if (highestPriorityPending != null)
                {
                    preservedParentStates.Add(highestPriorityPending);
                    pendingUnregisters.Remove(highestPriorityPending);
                }
            }
            else
            {
                // Quick toggle prevention: if a state is unregistered and re-registered instantly, cancel the unregister.
                pendingUnregisters.Remove(_stateData);
            }

            RegisterVolumeTargets(_stateData);
        }

        private void HandleStateUnregistered(StateSO _stateData)
        {
            if (_stateData == null) return;

            pendingUnregisters.Add(_stateData);

            if (isUnregisterRoutineRunning == false)
            {
                StartCoroutine(ProcessPendingUnregistersRoutine());
            }
        }

        private void HandleStateRestored(StateSO _stateData)
        {
            if (_stateData == null) return;

            // The state is returning from the background history stack, it is no longer just preserved.
            preservedParentStates.Remove(_stateData);

            RegisterVolumeTargets(_stateData);
        }

        private IEnumerator ProcessPendingUnregistersRoutine()
        {
            isUnregisterRoutineRunning = true;

            // Wait until the absolute end of the frame to ensure all synchronous Registry events have fired.
            yield return new WaitForEndOfFrame();

            foreach (StateSO stateToRemove in pendingUnregisters)
            {
                if (preservedParentStates.Contains(stateToRemove))
                {
                    continue; // State was pushed into the history stack, do not revert audio.
                }

                UnregisterVolumeTargets(stateToRemove);
            }

            pendingUnregisters.Clear();
            isUnregisterRoutineRunning = false;
        }

        #endregion

        #region Volume Target Registration

        private void RegisterVolumeTargets(StateSO _stateData)
        {
            StateVolumeConfig config = FindConfig(_stateData);
            if (config == null) return;

            foreach (MixerVolumeTarget target in config.VolumeTargets)
            {
                if (target.TargetGroup == null || target.TargetGroup.audioMixer == null) continue;

                AudioChannelIdentifier channelId = new AudioChannelIdentifier(target.TargetGroup.audioMixer, target.ExposedVolumeParameter, target.TargetBroAudioType);

                if (activeChannels.ContainsKey(channelId) == false)
                {
                    activeChannels[channelId] = new ChannelStateTracker();
                }

                ChannelStateTracker tracker = activeChannels[channelId];
                tracker.ActiveStates[_stateData] = target;

                if (tracker.MonitorRoutine == null)
                {
                    tracker.MonitorRoutine = StartCoroutine(VolumeMonitorRoutine(channelId, tracker));
                }
            }
        }

        private void UnregisterVolumeTargets(StateSO _stateData)
        {
            StateVolumeConfig config = FindConfig(_stateData);
            if (config == null || config.RevertOnUnregister == false) return;

            foreach (MixerVolumeTarget target in config.VolumeTargets)
            {
                if (target.TargetGroup == null || target.TargetGroup.audioMixer == null) continue;

                AudioChannelIdentifier channelId = new AudioChannelIdentifier(target.TargetGroup.audioMixer, target.ExposedVolumeParameter, target.TargetBroAudioType);

                if (activeChannels.TryGetValue(channelId, out ChannelStateTracker tracker))
                {
                    tracker.ActiveStates.Remove(_stateData);
                }
            }
        }

        #endregion

        #region Thermostat Monitoring Engine (Unscaled Time)

        private IEnumerator VolumeMonitorRoutine(AudioChannelIdentifier _channelId, ChannelStateTracker _tracker)
        {
            float actualLinear = GetLinearFromMixer(_channelId.Mixer, _channelId.Parameter);
            float baselineLinear = actualLinear;

            _tracker.ExpectedLinearVolume = actualLinear;
            _tracker.CurrentAppliedMultiplier = 1f;

            float currentLerpSpeed = INSTANT_SNAP_SPEED;

            while (_tracker.ActiveStates.Count > 0 || Mathf.Abs(_tracker.CurrentAppliedMultiplier - 1f) > 0.001f)
            {
                actualLinear = GetLinearFromMixer(_channelId.Mixer, _channelId.Parameter);

                // EXTERNAL MUTATION DETECTION
                if (Mathf.Abs(actualLinear - _tracker.ExpectedLinearVolume) > EXTERNAL_MUTATION_TOLERANCE)
                {
                    baselineLinear = actualLinear;
                }

                // CALCULATE TARGET MULTIPLIER & SPEED
                float targetMultiplier = 1f;

                if (_tracker.ActiveStates.Count > 0)
                {
                    float maxLerpDuration = 0f;
                    foreach (KeyValuePair<StateSO, MixerVolumeTarget> keyValuePair in _tracker.ActiveStates)
                    {
                        targetMultiplier *= keyValuePair.Value.TargetVolumeMultiplier;
                        if (keyValuePair.Value.LerpDuration > maxLerpDuration)
                        {
                            maxLerpDuration = keyValuePair.Value.LerpDuration;
                        }
                    }
                    currentLerpSpeed = maxLerpDuration > 0f ? 1f / maxLerpDuration : INSTANT_SNAP_SPEED;
                }

                // APPLY SMOOTH MULTIPLIER
                if (currentLerpSpeed >= INSTANT_SNAP_SPEED)
                {
                    _tracker.CurrentAppliedMultiplier = targetMultiplier;
                }
                else
                {
                    _tracker.CurrentAppliedMultiplier = Mathf.MoveTowards(_tracker.CurrentAppliedMultiplier, targetMultiplier, Time.unscaledDeltaTime * currentLerpSpeed);
                }

                // CALCULATE & APPLY EXPECTED VOLUME
                _tracker.ExpectedLinearVolume = baselineLinear * _tracker.CurrentAppliedMultiplier;
                ApplyVolume(_channelId.Mixer, _channelId.Parameter, _channelId.BroType, _tracker.ExpectedLinearVolume);

                yield return null;
            }

            // FINAL RESTORE
            ApplyVolume(_channelId.Mixer, _channelId.Parameter, _channelId.BroType, baselineLinear);

            _tracker.MonitorRoutine = null;
            activeChannels.Remove(_channelId);
        }

        #endregion

        #region Helpers & Audio Math

        private StateVolumeConfig FindConfig(StateSO _stateData)
        {
            for (int i = 0; i < stateConfigurations.Count; i++)
            {
                if (stateConfigurations[i].TargetState == _stateData)
                {
                    return stateConfigurations[i];
                }
            }
            return null;
        }

        private float GetLinearFromMixer(AudioMixer _mixer, string _parameter)
        {
            if (_mixer.GetFloat(_parameter, out float decibels))
            {
                return Mathf.Pow(10f, decibels / 20f);
            }
            return 1f;
        }

        private void ApplyVolume(AudioMixer _mixer, string _parameter, BroAudioType _broType, float _linearVolume)
        {
            float safeLinear = Mathf.Max(MINIMUM_LINEAR_VOLUME, _linearVolume);
            float decibels = 20f * Mathf.Log10(safeLinear);

            _mixer.SetFloat(_parameter, decibels);

            if ((int)_broType != 0)
            {
                BroAudio.SetVolume(_broType, safeLinear, 0f);
            }
        }

        #endregion
    }
}