using System;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>Listens to a private copy of the show or hide action, so a state that switches its map off cannot silence it.</summary>
    internal class DebuggerToggleInput : IDisposable
    {
        public InputAction Listening { get; private set; }

        public DebuggerToggleInput(InputAction _action)
        {
            if (_action == null)
            {
                return;
            }
            Listening = _action.Clone();
            Listening.Enable();
        }

        public bool WasPerformed()
        {
            return Listening != null && Listening.WasPerformedThisFrame();
        }

        public void Dispose()
        {
            if (Listening == null)
            {
                return;
            }
            Listening.Disable();
            Listening.Dispose();
            Listening = null;
        }
    }
}
