using System;
using System.Collections.Generic;
using Ami.BroAudio;
using Ami.BroAudio.Data;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Plays BroAudio sounds when a state enters or leaves; looping sounds stop on the next change.</summary>
    [AddComponentMenu("AbstractPixel/Game Management/Game State Audio Trigger")]
    public class GameStateAudioTrigger : MonoBehaviour
    {
        [Serializable]
        public class StateAudioEntry
        {
            public StateSO State;

            [Tooltip("Optional sounds played together on activation, such as an entrance cue and a loop.")]
            public List<SoundID> ActivationSounds = new List<SoundID>();

            [Tooltip("Optional sounds played on deactivation. Leave empty when no exit sound is needed.")]
            public List<SoundID> DeactivationSounds = new List<SoundID>();
        }

        [SerializeField] private List<StateAudioEntry> stateAudio = new List<StateAudioEntry>();
        private readonly List<IAudioPlayer> loopingPlayers = new List<IAudioPlayer>();

        private void OnEnable()
        {
            GameStates.Changed += HandleGameStatesChanged;

            StateSO currentState = GameStates.Top;
            if (currentState != null)
            {
                HandleStateRegistered(currentState);
            }
        }

        private void OnDisable()
        {
            GameStates.Changed -= HandleGameStatesChanged;
            StopLoopingSounds();
        }

        private void HandleGameStatesChanged(TransitionRecord _record)
        {
            if (_record.Kind == TransitionKind.Entered)
            {
                HandleStateRegistered(_record.State);
            }
            else if (_record.Kind == TransitionKind.Left)
            {
                HandleStateUnregistered(_record.State);
            }
        }

        private void HandleStateRegistered(StateSO state)
        {
            // A nested state change can supersede this registration before our listener runs.
            if (!GameStates.IsActive(state))
            {
                return;
            }

            StopLoopingSounds();
            StateAudioEntry entry = FindEntry(state);
            if (entry != null)
            {
                PlaySounds(entry.ActivationSounds);
            }
        }

        private void HandleStateUnregistered(StateSO state)
        {
            StopLoopingSounds();
            StateAudioEntry entry = FindEntry(state);
            if (entry != null)
            {
                PlaySounds(entry.DeactivationSounds);
            }
        }

        private StateAudioEntry FindEntry(StateSO state)
        {
            if (state == null || stateAudio == null)
            {
                return null;
            }

            foreach (StateAudioEntry entry in stateAudio)
            {
                if (entry != null && entry.State == state)
                {
                    return entry;
                }
            }

            return null;
        }

        private void PlaySounds(List<SoundID> sounds)
        {
            if (sounds == null)
            {
                return;
            }

            foreach (SoundID sound in sounds)
            {
                if (!sound.IsValid())
                {
                    continue;
                }

                bool isLooping = BroAudio.TryGetEntityInfo(sound, out var info)
                    && info is IAudioEntity entity && entity.HasLoop(out _, out _);
                IAudioPlayer player = BroAudio.Play(sound);
                if (isLooping && player != null && player.IsActive)
                {
                    loopingPlayers.Add(player);
                }
            }
        }

        private void StopLoopingSounds()
        {
            foreach (IAudioPlayer player in loopingPlayers)
            {
                if (player.IsActive)
                {
                    player.Stop();
                }
            }

            loopingPlayers.Clear();
        }
    }
}
