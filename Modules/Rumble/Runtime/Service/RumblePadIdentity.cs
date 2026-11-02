using AbstractPixel.Core;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// What the bound controller is, in plain words: its type (PlayStation / Xbox / Steam / generic), its model
    /// ("DualSense (PS5)"), how it is connected and how we know. Built once per bind (it allocates a few strings),
    /// never per frame. The family comes from InputDeviceTracker; Steam wins when Steam Input drives the pad.
    /// </summary>
    public class RumblePadIdentity
    {
        private const string SteamTypePrefix = "k_ESteamInputType_";

        public static readonly RumblePadIdentity None = new RumblePadIdentity();

        public RumblePadKind Kind { get; private set; } = RumblePadKind.None;
        public string Model { get; private set; } = "no controller";
        public RumbleConnection Connection { get; private set; } = RumbleConnection.Unknown;
        /// <summary>Why the connection is what it says, e.g. "HID descriptor: output report 78 bytes".</summary>
        public string ConnectionSource { get; private set; } = "-";
        public string DeviceName { get; private set; } = "none";
        public string Layout { get; private set; } = "-";
        public string Product { get; private set; } = "-";
        public string InterfaceName { get; private set; } = "-";

        /// <summary>One plain line for Player.log.</summary>
        public string LogLine
        {
            get
            {
                return Kind + " - " + Model + ", connection " + Connection + " (" + ConnectionSource + ")";
            }
        }

        /// <summary>_backendName is the active backend's display name, e.g. "Steam Input (k_ESteamInputType_PS5Controller)".</summary>
        public static RumblePadIdentity Read(InputDevice _device, string _backendName)
        {
            RumblePadIdentity identity = new RumblePadIdentity();
            bool isSteamBackend = _backendName != null && _backendName.StartsWith("Steam");
            bool hasUnityPad = _device is Gamepad;
            if (isSteamBackend && hasUnityPad == false)
            {
                // Steam drives a pad that Unity can't see (Steam Input took it away from the game).
                identity.Kind = ReadSteamKind(_backendName);
                identity.Model = ReadSteamModel(_backendName) + " through Steam Input";
                identity.ConnectionSource = "Steam owns the pad; Unity can't see it, so the connection is not known";
                identity.DeviceName = "none in Unity";
                return identity;
            }

            if (_device == null)
            {
                return identity;
            }

            identity.DeviceName = _device.displayName;
            identity.Layout = _device.layout;
            identity.Product = _device.description.product;
            identity.InterfaceName = _device.description.interfaceName;
            identity.Kind = ReadKind(_device, _backendName);
            identity.Model = ReadModel(_device, identity.Kind, _backendName);
            identity.ReadConnection(_device);
            return identity;
        }

        private static RumblePadKind ReadKind(InputDevice _device, string _backendName)
        {
            // Through Steam the Unity device may be Steam's virtual Xbox pad, so the family comes from Steam's type.
            bool isSteamBackend = _backendName != null && _backendName.StartsWith("Steam");
            if (isSteamBackend)
            {
                return ReadSteamKind(_backendName);
            }

            DeviceFamily family = InputDeviceTracker.GetDeviceFamily(_device);
            switch (family)
            {
                case DeviceFamily.PlayStation:
                    return RumblePadKind.PlayStation;
                case DeviceFamily.Xbox:
                    return RumblePadKind.Xbox;
                case DeviceFamily.SteamDevice:
                    return RumblePadKind.Steam;
                case DeviceFamily.KeyboardMouse:
                    return RumblePadKind.None;
                default:
                    return RumblePadKind.Generic;
            }
        }

        private static string ReadModel(InputDevice _device, RumblePadKind _kind, string _backendName)
        {
            if (_kind == RumblePadKind.None)
            {
                return "keyboard / mouse (no controller in use)";
            }

            bool isSteamBackend = _backendName != null && _backendName.StartsWith("Steam");
            if (isSteamBackend)
            {
                return ReadSteamModel(_backendName) + " through Steam Input";
            }

            if (_device is DualSenseGamepadHID)
            {
                return "DualSense (PS5)";
            }

            if (_device is DualShock4GamepadHID)
            {
                return "DualShock 4 (PS4)";
            }

            if (_device is DualShock3GamepadHID)
            {
                return "DualShock 3 (PS3)";
            }

            if (_device is XInputController)
            {
                return "Xbox-style pad (XInput)";
            }

            string product = _device.description.product;
            if (string.IsNullOrEmpty(product))
            {
                return _device.displayName;
            }

            return product;
        }

        /// <summary>PlayStation / Xbox from Steam's pad type in the backend name; Steam for Deck, Steam Controller and the rest.</summary>
        private static RumblePadKind ReadSteamKind(string _backendName)
        {
            bool isPlayStation = _backendName.Contains("PS5Controller") || _backendName.Contains("PS4Controller")
                || _backendName.Contains("PS3Controller");
            if (isPlayStation)
            {
                return RumblePadKind.PlayStation;
            }

            bool isXbox = _backendName.Contains("XBox360Controller") || _backendName.Contains("XBoxOneController");
            if (isXbox)
            {
                return RumblePadKind.Xbox;
            }

            return RumblePadKind.Steam;
        }

        /// <summary>Steam's pad type from the backend name, in plain words ("PS5 pad", "Steam Deck").</summary>
        private static string ReadSteamModel(string _backendName)
        {
            if (_backendName == null)
            {
                return "pad";
            }

            int typeStart = _backendName.IndexOf(SteamTypePrefix, System.StringComparison.Ordinal);
            if (typeStart < 0)
            {
                return "pad";
            }

            string steamType = _backendName.Substring(typeStart + SteamTypePrefix.Length).TrimEnd(')');
            switch (steamType)
            {
                case "PS5Controller":
                    return "PS5 pad";
                case "PS4Controller":
                    return "PS4 pad";
                case "PS3Controller":
                    return "PS3 pad";
                case "XBox360Controller":
                case "XBoxOneController":
                    return "Xbox pad";
                case "SteamDeckController":
                    return "Steam Deck controls";
                case "SteamController":
                    return "Steam Controller";
                case "SwitchProController":
                    return "Switch Pro pad";
                default:
                    return steamType;
            }
        }

        /// <summary>
        /// PlayStation pads declare their longest output report in the HID descriptor: under 78 bytes on USB, 78 or
        /// more on Bluetooth. Other pads (XInput, Steam) don't tell us.
        /// </summary>
        private void ReadConnection(InputDevice _device)
        {
            bool isPlayStationHid = _device is DualShockGamepad;
            if (isPlayStationHid == false)
            {
                ConnectionSource = "Windows doesn't report it for this pad type";
                return;
            }

            int outputReportSize = DualSenseRawWriter.ReadDescriptorOutputReportSize(_device);
            if (outputReportSize <= 0)
            {
                ConnectionSource = "HID descriptor empty at bind (Windows not ready yet)";
                return;
            }

            ConnectionSource = "HID descriptor: output report " + outputReportSize + " bytes";
            Connection = RumbleConnection.Usb;
            if (outputReportSize >= DualSenseBluetoothOutputCommand.ReportSize)
            {
                Connection = RumbleConnection.Bluetooth;
            }
        }
    }
}
