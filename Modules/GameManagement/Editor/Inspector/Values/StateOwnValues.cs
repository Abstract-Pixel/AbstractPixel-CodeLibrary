using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Switches an effect between Own and the parent. Until the developer has picked an Own value, Own starts from parent 1's.</summary>
    internal static class StateOwnValues
    {
        public static StateEffectSource SourceOf(StateEffectSettings _effects, StateEffectKind _kind)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    return _effects.TimeScaleSource;
                case StateEffectKind.Cursor:
                    return _effects.CursorSource;
                case StateEffectKind.Hud:
                    return _effects.HudSource;
                default:
                    return _effects.InputSource;
            }
        }

        public static void SetSource(StateSO _state, StateEffectKind _kind, StateEffectSource _source)
        {
            StateEffectSettings effects = _state.Effects;
            if (SourceOf(effects, _kind) == _source)
            {
                return;
            }
            Undo.RecordObject(_state, "Change effect source");

            StateSO firstParent = _state.Parents.FirstOrDefault(_parent => _parent != null);
            StateSO owner = firstParent != null ? StateParentValues.ValueOwner(firstParent, _kind) : null;
            int picked = 1 << (int)_kind;
            if (_source == StateEffectSource.Own)
            {
                if ((effects.PickedOwnValues & picked) == 0 && owner != null)
                {
                    Copy(owner.Effects, effects, _kind);
                }
            }
            else if (!SameStoredValue(effects, owner != null ? owner.Effects : new StateEffectSettings(), _kind))
            {
                // A value that is not just the copy was picked by the developer, so it is never written over again.
                effects.PickedOwnValues |= picked;
            }

            SetSourceField(effects, _kind, _source);
            EditorUtility.SetDirty(_state);
        }

        private static void SetSourceField(StateEffectSettings _effects, StateEffectKind _kind, StateEffectSource _source)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    _effects.TimeScaleSource = _source;
                    break;
                case StateEffectKind.Cursor:
                    _effects.CursorSource = _source;
                    break;
                case StateEffectKind.Hud:
                    _effects.HudSource = _source;
                    break;
                default:
                    _effects.InputSource = _source;
                    break;
            }
        }

        private static void Copy(StateEffectSettings _from, StateEffectSettings _to, StateEffectKind _kind)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    _to.TimeScale = _from.TimeScale;
                    _to.TimeScaleEaseSeconds = _from.TimeScaleEaseSeconds;
                    _to.TimeScaleEaseCurve = new AnimationCurve(Keys(_from));
                    break;
                case StateEffectKind.Cursor:
                    _to.Cursor = _from.Cursor;
                    break;
                case StateEffectKind.Hud:
                    _to.HudVisible = _from.HudVisible;
                    break;
                default:
                    _to.InputMaps = new List<InputActionReference>(_from.InputMaps);
                    break;
            }
        }

        private static Keyframe[] Keys(StateEffectSettings _effects)
        {
            return _effects.TimeScaleEaseCurve != null ? _effects.TimeScaleEaseCurve.keys : new Keyframe[0];
        }

        private static bool SameStoredValue(StateEffectSettings _first, StateEffectSettings _second, StateEffectKind _kind)
        {
            switch (_kind)
            {
                case StateEffectKind.TimeScale:
                    return _first.TimeScale == _second.TimeScale && _first.TimeScaleEaseSeconds == _second.TimeScaleEaseSeconds
                        && Keys(_first).SequenceEqual(Keys(_second));
                case StateEffectKind.Cursor:
                    return _first.Cursor == _second.Cursor;
                case StateEffectKind.Hud:
                    return _first.HudVisible == _second.HudVisible;
                default:
                    return _first.InputMaps.SequenceEqual(_second.InputMaps);
            }
        }
    }
}
