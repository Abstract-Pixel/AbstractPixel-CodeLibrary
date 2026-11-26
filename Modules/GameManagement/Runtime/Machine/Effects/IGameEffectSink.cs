using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Applies effects to the game. The Unity sink (Session 3) writes time, cursor, HUD and input; tests record.</summary>
    public interface IGameEffectSink
    {
        /// <summary>Apply what changed. Null fields stay as they are; time eases from its value at this moment.</summary>
        void Apply(GameEffects _effects);
    }
}
