using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>A custom input source that remembers the maps it was asked to enable.</summary>
    public class RecordingInputSource : IInputSource
    {
        public readonly List<IReadOnlyList<InputActionMap>> Calls = new List<IReadOnlyList<InputActionMap>>();

        public void EnableOnly(IReadOnlyList<InputActionMap> _maps)
        {
            Calls.Add(_maps);
        }
    }
}
