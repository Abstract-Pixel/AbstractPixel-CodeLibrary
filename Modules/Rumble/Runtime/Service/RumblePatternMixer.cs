using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Holds the one-shot patterns that are playing (4 fixed slots, no allocation) and samples them each tick.
    /// When the slots are full, the weakest pattern is replaced.
    /// </summary>
    public class RumblePatternMixer
    {
        private const int SlotCount = 4;

        private readonly RumblePattern[] slotPatterns = new RumblePattern[SlotCount];
        private readonly float[] slotStartTimes = new float[SlotCount];
        private readonly float[] slotStrengths = new float[SlotCount];

        // Results of the last Sample() call.
        public float Low { get; private set; }
        public float High { get; private set; }
        public float DuckAmount { get; private set; }
        public bool HasLightBarFlash { get; private set; }
        public Color LightBarFlashColor { get; private set; }
        public bool HasTriggerBuzz { get; private set; }
        public float TriggerBuzzAmplitude { get; private set; }
        public float TriggerBuzzFrequency { get; private set; }

        public bool IsAnyPlaying
        {
            get
            {
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    if (slotPatterns[slot] != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int SlotCountForDebug => SlotCount;

        /// <summary>Debug only: the pattern in one slot, if one is playing there. Allocation-free.</summary>
        public bool TryReadSlot(int _slot, out RumbleActivePattern _active)
        {
            _active = new RumbleActivePattern();
            if (_slot < 0 || _slot >= SlotCount)
            {
                return false;
            }

            RumblePattern pattern = slotPatterns[_slot];
            if (pattern == null)
            {
                return false;
            }

            float elapsed = ReadClock(pattern) - slotStartTimes[_slot];
            float timeShare = Mathf.Clamp01(elapsed / pattern.Duration);
            float strength = slotStrengths[_slot];
            _active.Pattern = pattern;
            _active.Strength = strength;
            _active.Elapsed = elapsed;
            _active.CurrentLow = pattern.EvaluateLow(timeShare) * strength;
            _active.CurrentHigh = pattern.EvaluateHigh(timeShare) * strength;
            return true;
        }

        public void Play(RumblePattern _pattern, float _strength)
        {
            if (_pattern == null || _strength <= 0f)
            {
                return;
            }

            int slot = FindFreeOrWeakestSlot();
            slotPatterns[slot] = _pattern;
            slotStrengths[slot] = _strength;
            slotStartTimes[slot] = ReadClock(_pattern);
        }

        public void StopAll()
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                slotPatterns[slot] = null;
            }

            ClearResults();
        }

        /// <summary>Samples every playing pattern, keeps the strongest value per output and frees finished slots.</summary>
        public void Sample()
        {
            ClearResults();

            for (int slot = 0; slot < SlotCount; slot++)
            {
                RumblePattern pattern = slotPatterns[slot];
                if (pattern == null)
                {
                    continue;
                }

                float elapsed = ReadClock(pattern) - slotStartTimes[slot];
                if (elapsed >= pattern.Duration)
                {
                    slotPatterns[slot] = null;
                    continue;
                }

                // A game-time pattern is frozen while Time.timeScale is 0 (pause): stay silent instead of holding its
                // current value on the motors, and carry on from the same point when the game resumes.
                bool isFrozenByPause = pattern.UseUnscaledTime == false && Time.timeScale <= 0f;
                if (isFrozenByPause)
                {
                    continue;
                }

                float timeShare = elapsed / pattern.Duration;
                AddPatternSample(pattern, timeShare, slotStrengths[slot]);
            }
        }

        private void AddPatternSample(RumblePattern _pattern, float _timeShare, float _strength)
        {
            float low = _pattern.EvaluateLow(_timeShare) * _strength;
            float high = _pattern.EvaluateHigh(_timeShare) * _strength;
            Low = Mathf.Max(Low, low);
            High = Mathf.Max(High, high);

            if (_pattern.MutesDrivingRumble)
            {
                float envelope = Mathf.Max(low, high);
                DuckAmount = Mathf.Max(DuckAmount, Mathf.Clamp01(envelope));
            }

            if (_pattern.FlashesLightBar)
            {
                HasLightBarFlash = true;
                LightBarFlashColor = _pattern.LightBarFlashColor;
            }

            if (_pattern.BuzzesTriggers)
            {
                HasTriggerBuzz = true;
                TriggerBuzzAmplitude = Mathf.Max(TriggerBuzzAmplitude, _pattern.TriggerBuzzAmplitude * _strength);
                TriggerBuzzFrequency = _pattern.TriggerBuzzFrequency;
            }
        }

        private int FindFreeOrWeakestSlot()
        {
            int weakestSlot = 0;
            float weakestStrength = float.MaxValue;

            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (slotPatterns[slot] == null)
                {
                    return slot;
                }

                if (slotStrengths[slot] < weakestStrength)
                {
                    weakestStrength = slotStrengths[slot];
                    weakestSlot = slot;
                }
            }

            return weakestSlot;
        }

        private void ClearResults()
        {
            Low = 0f;
            High = 0f;
            DuckAmount = 0f;
            HasLightBarFlash = false;
            HasTriggerBuzz = false;
            TriggerBuzzAmplitude = 0f;
        }

        private static float ReadClock(RumblePattern _pattern)
        {
            if (_pattern.UseUnscaledTime)
            {
                return Time.unscaledTime;
            }

            return Time.time;
        }
    }
}
