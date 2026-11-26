using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>The effects the stack wants. A null field means "leave it as it is".</summary>
    public struct GameEffects : IEquatable<GameEffects>
    {
        public float? TimeScale;
        public float TimeScaleEaseSeconds;
        public UnityEngine.AnimationCurve TimeScaleEaseCurve;
        public GameCursorMode? Cursor;
        public bool? HudVisible;
        public IReadOnlyList<string> InputMapIds;

        public bool Equals(GameEffects _other)
        {
            return TimeScale == _other.TimeScale
                && TimeScaleEaseSeconds == _other.TimeScaleEaseSeconds
                && TimeScaleEaseCurve == _other.TimeScaleEaseCurve
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
}
