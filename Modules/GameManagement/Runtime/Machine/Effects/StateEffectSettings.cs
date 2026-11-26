using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>What a state controls: time scale, cursor, HUD and input maps. Each one can follow the parent.</summary>
    [Serializable]
    public class StateEffectSettings
    {
        public EffectSource TimeScaleSource = EffectSource.SameAsParent;
        [Range(0f, 1f)] public float TimeScale = 1f;
        [Tooltip("Realtime seconds to ease from the time scale at apply time to this one. 0 = instant.")]
        [Min(0f)] public float TimeScaleEaseSeconds = 0f;
        [Tooltip("Optional shape of the ease, 0 to 1 over its seconds. Empty = straight line (the old behaviour).")]
        public AnimationCurve TimeScaleEaseCurve = new AnimationCurve();

        public EffectSource CursorSource = EffectSource.SameAsParent;
        public GameCursorMode Cursor = GameCursorMode.Free;

        public EffectSource HudSource = EffectSource.SameAsParent;
        public bool HudVisible = false;

        public EffectSource InputSource = EffectSource.SameAsParent;
        [Tooltip("IDs of the input action maps this state turns on. Every other map of the input source is turned off.")]
        [InputMapId] public List<string> InputMapIds = new List<string>();
    }
}
