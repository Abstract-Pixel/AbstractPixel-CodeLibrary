using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The controller line of the F9 panel: the pad type in its colour (PlayStation blue, Xbox green, Steam greyish
    /// blue, generic grey), the model and how it is connected. Short form at the very top, long form in the details.
    /// Debug panel only (OnGUI).
    /// </summary>
    public static class RumblePadTypeLine
    {
        private const string PlayStationColor = "#4da3ff";
        private const string XboxColor = "#5cd65c";
        private const string SteamColor = "#8fa8c4";
        private const string GenericColor = "#a8a8a8";

        public static void AppendShort(RumbleService _rumble, StringBuilder _text)
        {
            RumblePadIdentity identity = _rumble.DebugPadIdentity;
            AppendKind(identity.Kind, _text);
            if (identity.Kind == RumblePadKind.None)
            {
                _text.Append("  connect a pad and press a button on it\n");
                return;
            }

            _text.Append("  ").Append(identity.Model).Append("  ·  ").Append(DescribeConnection(ReadLiveConnection(_rumble))).Append('\n');
        }

        public static void AppendDetails(RumbleService _rumble, StringBuilder _text)
        {
            RumblePadIdentity identity = _rumble.DebugPadIdentity;
            _text.Append("<b>Controller</b>  ");
            AppendKind(identity.Kind, _text);
            _text.Append("  ").Append(identity.Model).Append('\n');
            _text.Append("  connection ").Append(DescribeConnection(ReadLiveConnection(_rumble)));
            _text.Append("  (at bind: ").Append(identity.Connection).Append(", ").Append(identity.ConnectionSource).Append(")\n");
            _text.Append("  device '").Append(identity.DeviceName).Append("'  layout ").Append(identity.Layout);
            _text.Append("  product '").Append(identity.Product).Append("'  interface ").Append(identity.InterfaceName).Append('\n');
            RumbleBluetoothCheck.AppendDetails(_rumble, _text);
        }

        /// <summary>
        /// How the pad is connected right now. On a DualSense the raw writer knows best (it re-reads a descriptor
        /// that was empty at bind). Its descriptor reading is used, not the format it is trying after refusals.
        /// Otherwise the bind-time reading.
        /// </summary>
        public static RumbleConnection ReadLiveConnection(RumbleService _rumble)
        {
            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null && writer.DescriptorOutputReportSize > 0)
            {
                if (writer.GuessedTransport == DualSenseTransport.Bluetooth)
                {
                    return RumbleConnection.Bluetooth;
                }

                return RumbleConnection.Usb;
            }

            return _rumble.DebugPadIdentity.Connection;
        }

        private static void AppendKind(RumblePadKind _kind, StringBuilder _text)
        {
            _text.Append("<b><color=").Append(ReadKindColor(_kind)).Append('>').Append(ReadKindLabel(_kind)).Append("</color></b>");
        }

        private static string DescribeConnection(RumbleConnection _connection)
        {
            switch (_connection)
            {
                case RumbleConnection.Usb:
                    return "USB cable";
                case RumbleConnection.Bluetooth:
                    return "Bluetooth (wireless)";
                default:
                    return "connection not reported";
            }
        }

        private static string ReadKindLabel(RumblePadKind _kind)
        {
            switch (_kind)
            {
                case RumblePadKind.PlayStation:
                    return "PLAYSTATION";
                case RumblePadKind.Xbox:
                    return "XBOX";
                case RumblePadKind.Steam:
                    return "STEAM";
                case RumblePadKind.Generic:
                    return "GENERIC";
                default:
                    return "NO CONTROLLER";
            }
        }

        private static string ReadKindColor(RumblePadKind _kind)
        {
            switch (_kind)
            {
                case RumblePadKind.PlayStation:
                    return PlayStationColor;
                case RumblePadKind.Xbox:
                    return XboxColor;
                case RumblePadKind.Steam:
                    return SteamColor;
                default:
                    return GenericColor;
            }
        }
    }
}
