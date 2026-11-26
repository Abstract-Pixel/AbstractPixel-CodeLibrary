using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
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
