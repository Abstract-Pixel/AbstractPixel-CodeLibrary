using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Compares and words one effect of resolved game effects. Comparisons use the real values, never the text.</summary>
    internal static class StateEffectValues
    {
        public const string KeepsLast = "Keeps last";

        public static bool Same(StateEffectKind _kind, GameEffects _first, GameEffects _second)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    return _first.TimeScale == _second.TimeScale;
                case StateEffectKind.Cursor:
                    return _first.Cursor == _second.Cursor;
                case StateEffectKind.Hud:
                    return _first.HudVisible == _second.HudVisible;
                default:
                    return SameMapSet(_first.InputMaps, _second.InputMaps);
            }
        }

        /// <summary>False when no state in the stack sets this effect, so the game keeps the last value.</summary>
        public static bool IsSet(StateEffectKind _kind, GameEffects _effects)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    return _effects.TimeScale.HasValue;
                case StateEffectKind.Cursor:
                    return _effects.Cursor.HasValue;
                case StateEffectKind.Hud:
                    return _effects.HudVisible.HasValue;
                default:
                    return _effects.InputMaps != null;
            }
        }

        public static string Text(StateEffectKind _kind, GameEffects _effects)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    return _effects.TimeScale.HasValue ? _effects.TimeScale.Value.ToString("0.####", CultureInfo.InvariantCulture) : KeepsLast;
                case StateEffectKind.Cursor:
                    return _effects.Cursor.HasValue ? ObjectNames.NicifyVariableName(_effects.Cursor.Value.ToString()) : KeepsLast;
                case StateEffectKind.Hud:
                    if (!_effects.HudVisible.HasValue)
                    {
                        return KeepsLast;
                    }
                    return _effects.HudVisible.Value ? "Shown" : "Hidden";
                default:
                    if (_effects.InputMaps == null)
                    {
                        return KeepsLast;
                    }
                    return _effects.InputMaps.Count == 0 ? "All off" : string.Join(", ", MapNames(_effects.InputMaps));
            }
        }

        /// <summary>The value in full, for tooltips: input maps go one per line.</summary>
        public static string FullText(StateEffectKind _kind, GameEffects _effects)
        {
            if (_kind == StateEffectKind.Input && _effects.InputMaps != null && _effects.InputMaps.Count > 0)
            {
                return string.Join("\n", MapNames(_effects.InputMaps));
            }
            return Text(_kind, _effects);
        }

        // Which maps are on is all that counts, so order and repeats make no difference.
        private static bool SameMapSet(IReadOnlyList<InputActionMap> _first, IReadOnlyList<InputActionMap> _second)
        {
            if (_first == null || _second == null)
            {
                return _first == null && _second == null;
            }
            return new HashSet<InputActionMap>(_first).SetEquals(_second);
        }

        /// <summary>The map names in the order the Inspector shows them.</summary>
        public static IEnumerable<string> MapNames(IReadOnlyList<InputActionMap> _maps)
        {
            return _maps.Select(_map => _map.name).OrderBy(_name => _name);
        }
    }
}
