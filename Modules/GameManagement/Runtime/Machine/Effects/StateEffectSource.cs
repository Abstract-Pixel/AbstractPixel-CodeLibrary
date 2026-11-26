using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Where a state's effect comes from.</summary>
    public enum StateEffectSource
    {
        /// <summary>Use the value of the state below (the parent). On a main state this keeps the last applied value.</summary>
        SameAsParent,
        /// <summary>Use this state's own value.</summary>
        Own
    }
}
