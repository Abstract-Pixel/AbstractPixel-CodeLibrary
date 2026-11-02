using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The Steam Input part of the F9 panel.
    /// Short form (always shown): "Steam Input: ON / OFF / NOT USED / NOT AVAILABLE / FAILED" and "Fallback: ACTIVE /
    /// none", each with its reason. Details: the mode and backend rule, Steam's own view (API state, every pad it
    /// lists, XInput slots, the game's opt-in mask) and every pad Unity sees with its connection.
    /// Debug panel only (OnGUI); allocates a few strings per draw.
    /// </summary>
    public static class RumbleSteamLines
    {
        private const string OnColor = "#70ff70";
        private const string OffColor = "#ffd060";
        private const string NeutralColor = "#a8c4e0";
        private const string FailColor = "#ff7070";

        public static void AppendShort(RumbleService _rumble, StringBuilder _text)
        {
            RumbleSteamReport report = _rumble.DebugSteamReport;
            _text.Append("<b>Steam Input:</b>  <color=").Append(ReadStateColor(report.State)).Append('>');
            _text.Append(report.SteamInputLine).Append("</color>\n");

            _text.Append("<b>Fallback:</b>  ");
            if (report.IsFallbackActive)
            {
                _text.Append("<color=").Append(OffColor).Append('>').Append(report.FallbackLine).Append("</color>\n");
                return;
            }

            _text.Append(report.FallbackLine).Append('\n');
        }

        public static void AppendDetails(RumbleService _rumble, StringBuilder _text)
        {
            RumbleSteamReport report = _rumble.DebugSteamReport;
            _text.Append("<b>Backend</b>  mode ").Append(report.Mode);
            if (report.IsAutoResolvedForEditor)
            {
                _text.Append(" (profile Auto = Unity in the Editor)");
            }

            _text.Append("  ->  ").Append(_rumble.DebugBackend).Append('\n');
            _text.Append("  rule: ").Append(_rumble.DebugBackendReason).Append('\n');
            AppendSteamApi(_rumble, report, _text);
            _text.Append("<b>Pads Unity sees</b>\n");
            _rumble.DebugConnectedPads.AppendTo(_text, _rumble.DebugBoundDevice);
        }

        private static void AppendSteamApi(RumbleService _rumble, RumbleSteamReport _report, StringBuilder _text)
        {
            _text.Append("<b>Steam Input API</b>  ");
            IRumbleBackendStatus steamStatus = _rumble.DebugSteamStatus;
            if (steamStatus == null)
            {
                _text.Append("no Steam backend in this build\n");
                return;
            }

            // Unity mode never touches Steam, so the panel doesn't either.
            if (_report.State == SteamInputState.NotChecked)
            {
                _text.Append("not asked (").Append(_report.SteamInputLine).Append(")\n");
                return;
            }

            _text.Append("last answer: ").Append(steamStatus.StatusLine).Append('\n');
            steamStatus.AppendDebugDetails(_text);
        }

        private static string ReadStateColor(SteamInputState _state)
        {
            switch (_state)
            {
                case SteamInputState.Serving:
                    return OnColor;
                case SteamInputState.FailedForThisPad:
                case SteamInputState.NotRunning:
                    return FailColor;
                case SteamInputState.NotChecked:
                case SteamInputState.NotInstalled:
                    return NeutralColor;
                default:
                    return OffColor;
            }
        }
    }
}
