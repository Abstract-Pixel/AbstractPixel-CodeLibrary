using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Every gamepad Unity sees (type, model, connection, XInput slot), so two pads, or a real DualSense next to
    /// Steam's virtual Xbox pad, can be told apart in the F9 details and Player.log. Rebuilt on device events only
    /// (it reads HID descriptors and allocates), never per frame.
    /// </summary>
    public class RumbleConnectedPads
    {
        private readonly List<RumblePadIdentity> identities = new List<RumblePadIdentity>(4);
        private readonly List<InputDevice> devices = new List<InputDevice>(4);
        private readonly List<int> xinputSlots = new List<int>(4);

        public int Count => identities.Count;

        public void Rebuild()
        {
            identities.Clear();
            devices.Clear();
            xinputSlots.Clear();
            for (int index = 0; index < Gamepad.all.Count; index++)
            {
                Gamepad pad = Gamepad.all[index];
                identities.Add(RumblePadIdentity.Read(pad, null));
                devices.Add(pad);
                xinputSlots.Add(XInputSlot.Read(pad));
            }
        }

        /// <summary>One line per pad; the bound one is marked. Plain text, so it also works in Player.log.</summary>
        public void AppendTo(StringBuilder _text, InputDevice _boundDevice)
        {
            if (identities.Count == 0)
            {
                _text.Append("  none\n");
                return;
            }

            for (int index = 0; index < identities.Count; index++)
            {
                RumblePadIdentity identity = identities[index];
                _text.Append("  ").Append(identity.Kind).Append(" - ").Append(identity.Model);
                _text.Append(", ").Append(DescribeConnection(identity.Connection));
                _text.Append(", layout ").Append(identity.Layout);
                if (xinputSlots[index] != XInputSlot.None)
                {
                    _text.Append(", XInput slot ").Append(xinputSlots[index]);
                }

                if (devices[index] == _boundDevice)
                {
                    _text.Append("  <- BOUND");
                }

                _text.Append('\n');
            }
        }

        private static string DescribeConnection(RumbleConnection _connection)
        {
            switch (_connection)
            {
                case RumbleConnection.Usb:
                    return "USB";
                case RumbleConnection.Bluetooth:
                    return "Bluetooth";
                default:
                    return "connection not reported";
            }
        }
    }
}
