using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class BoolSetting : BaseSetting<bool>
    {
        // No additional fields required. 
        // The UI will simply read the CurrentValue (true/false) to render a Toggle.
    }
}