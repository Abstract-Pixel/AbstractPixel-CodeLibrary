using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>The input action maps the machine turns on and off. Implement it for a custom input setup.</summary>
    public interface IInputSource
    {
        /// <summary>Turns on the maps whose ID or name is listed and turns off every other map of this source.</summary>
        void EnableOnly(IReadOnlyList<InputActionMap> _maps);
    }
}
