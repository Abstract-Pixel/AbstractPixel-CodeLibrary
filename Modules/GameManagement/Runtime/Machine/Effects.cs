using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Where a state's effect comes from.</summary>
    public enum EffectSource
    {
        /// <summary>Use the value of the state below (the parent). On a main state this keeps the last applied value.</summary>
        SameAsParent,
        /// <summary>Use this state's own value.</summary>
        Own
    }

    /// <summary>The cursor while a state is on top. Replaces the old two cursor bools.</summary>
    public enum GameCursorMode
    {
        Free,
        LockedHidden,
        LockedVisible,
        FreeHidden
    }

    /// <summary>What a state controls: time scale, cursor, HUD and input maps. Each one can follow the parent.</summary>
    [Serializable]
    public class StateEffectSettings
    {
        public EffectSource TimeScaleSource = EffectSource.SameAsParent;
        [Range(0f, 1f)] public float TimeScale = 1f;
        [Tooltip("Realtime seconds to ease from the time scale at apply time to this one. 0 = instant.")]
        [Min(0f)] public float TimeScaleEaseSeconds = 0f;

        public EffectSource CursorSource = EffectSource.SameAsParent;
        public GameCursorMode Cursor = GameCursorMode.Free;

        public EffectSource HudSource = EffectSource.SameAsParent;
        public bool HudVisible = false;

        public EffectSource InputSource = EffectSource.SameAsParent;
        [Tooltip("IDs of the input action maps this state turns on. Every other map of the input source is turned off.")]
        public List<string> InputMapIds = new List<string>();
    }

    /// <summary>The effects the stack wants. A null field means "leave it as it is".</summary>
    public struct GameEffects : IEquatable<GameEffects>
    {
        public float? TimeScale;
        public float TimeScaleEaseSeconds;
        public GameCursorMode? Cursor;
        public bool? HudVisible;
        public IReadOnlyList<string> InputMapIds;

        public bool Equals(GameEffects _other)
        {
            return TimeScale == _other.TimeScale
                && TimeScaleEaseSeconds == _other.TimeScaleEaseSeconds
                && Cursor == _other.Cursor
                && HudVisible == _other.HudVisible
                && SameMaps(InputMapIds, _other.InputMapIds);
        }

        public override bool Equals(object _other)
        {
            return _other is GameEffects effects && Equals(effects);
        }

        public override int GetHashCode()
        {
            return (TimeScale, Cursor, HudVisible).GetHashCode();
        }

        public override string ToString()
        {
            string maps = InputMapIds == null ? "keep" : string.Join("+", InputMapIds);
            return $"time {Show(TimeScale)}, cursor {Show(Cursor)}, HUD {Show(HudVisible)}, input {maps}";
        }

        private static string Show<T>(T? _value) where T : struct
        {
            return _value.HasValue ? _value.Value.ToString() : "keep";
        }

        private static bool SameMaps(IReadOnlyList<string> _first, IReadOnlyList<string> _second)
        {
            if (_first == null || _second == null)
            {
                return _first == _second;
            }
            if (_first.Count != _second.Count)
            {
                return false;
            }
            for (int i = 0; i < _first.Count; i++)
            {
                if (_first[i] != _second[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>Applies effects to the game. The Unity sink (Session 3) writes time, cursor, HUD and input; tests record.</summary>
    public interface IGameEffectSink
    {
        /// <summary>Apply what changed. Null fields stay as they are; time eases from its value at this moment.</summary>
        void Apply(GameEffects _effects);
    }

    /// <summary>Realtime seconds for execution delays.</summary>
    public interface IGameClock
    {
        double Now { get; }
    }

    /// <summary>Unscaled realtime since startup.</summary>
    public class RealtimeGameClock : IGameClock
    {
        public double Now
        {
            get { return Time.realtimeSinceStartupAsDouble; }
        }
    }

    /// <summary>Works out the wanted effects from a stack, top state first. Pure.</summary>
    public static class EffectResolver
    {
        public static GameEffects Resolve(IReadOnlyList<StateSO> _stack, NoStateTimeScale _noStateTime)
        {
            GameEffects wanted = new GameEffects();
            if (_stack.Count == 0)
            {
                if (_noStateTime == NoStateTimeScale.SetToOne)
                {
                    wanted.TimeScale = 1f;
                }
                return wanted;
            }

            StateEffectSettings timeOwner = FindOwner(_stack, _effects => _effects.TimeScaleSource);
            if (timeOwner != null)
            {
                wanted.TimeScale = timeOwner.TimeScale;
                wanted.TimeScaleEaseSeconds = timeOwner.TimeScaleEaseSeconds;
            }

            StateEffectSettings cursorOwner = FindOwner(_stack, _effects => _effects.CursorSource);
            if (cursorOwner != null)
            {
                wanted.Cursor = cursorOwner.Cursor;
            }

            StateEffectSettings hudOwner = FindOwner(_stack, _effects => _effects.HudSource);
            if (hudOwner != null)
            {
                wanted.HudVisible = hudOwner.HudVisible;
            }

            StateEffectSettings inputOwner = FindOwner(_stack, _effects => _effects.InputSource);
            if (inputOwner != null)
            {
                wanted.InputMapIds = inputOwner.InputMapIds.ToArray();
            }
            return wanted;
        }

        private static StateEffectSettings FindOwner(IReadOnlyList<StateSO> _stack, Func<StateEffectSettings, EffectSource> _sourceOf)
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                StateEffectSettings effects = _stack[i].Effects;
                if (_sourceOf(effects) == EffectSource.Own)
                {
                    return effects;
                }
            }
            return null;
        }
    }
}
