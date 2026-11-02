using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// A hardware backend. Every method returns false on failure and never throws.
    /// Only RumbleService calls these.
    /// </summary>
    public interface IRumbleOutput
    {
        /// <summary>Shown in the debug fields, e.g. "Steam Input (PS5)" or "Unity (XInput)".</summary>
        string DisplayName { get; }

        RumbleCapabilities Capabilities { get; }

        /// <summary>False means this backend can't serve the device.</summary>
        bool TryBind(InputDevice _activeDevice);

        /// <summary>Motor speeds 0..1 each.</summary>
        bool SetMotors(float _low, float _high);

        bool SetLightBar(Color _color);

        bool SetTriggers(TriggerEffect _brake, TriggerEffect _throttle);

        /// <summary>Called once at the end of each service tick, for backends that batch writes. False = real failure.</summary>
        bool Flush();

        /// <summary>Motors 0, triggers Off, light bar restored.</summary>
        bool Reset();

        /// <summary>Forget the bound device without writing to it (it was unplugged).</summary>
        void Unbind();

        /// <summary>Final cleanup on quit (free unmanaged memory).</summary>
        void Release();
    }
}
