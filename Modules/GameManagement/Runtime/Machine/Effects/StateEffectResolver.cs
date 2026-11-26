using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>Works out the wanted effects from a stack, top state first. Pure.</summary>
    public static class StateEffectResolver
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
                wanted.TimeScaleEaseCurve = timeOwner.TimeScaleEaseCurve;
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
                wanted.InputMaps = MapsOf(inputOwner.InputMaps);
            }
            return wanted;
        }

        // Each listed action stands for its map; a map listed twice is turned on once.
        private static InputActionMap[] MapsOf(List<InputActionReference> _actions)
        {
            List<InputActionMap> maps = new List<InputActionMap>();
            foreach (InputActionReference reference in _actions)
            {
                InputActionMap map = reference != null && reference.action != null ? reference.action.actionMap : null;
                if (map != null && !maps.Contains(map))
                {
                    maps.Add(map);
                }
            }
            return maps.ToArray();
        }

        private static StateEffectSettings FindOwner(IReadOnlyList<StateSO> _stack, Func<StateEffectSettings, StateEffectSource> _sourceOf)
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                StateEffectSettings effects = _stack[i].Effects;
                if (_sourceOf(effects) == StateEffectSource.Own)
                {
                    return effects;
                }
            }
            return null;
        }
    }
}
