using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The Bluetooth part of the F9 verdict: a pad that is (or just was) there over Bluetooth but whose link or
    /// reports are failing. Each finding is a plain sentence with the fix; RumblePadCheck shows it as
    /// "NOT WORKING · BLUETOOTH" or "WAIT · BLUETOOTH". Signals, most reliable first:
    /// - the bound Bluetooth pad was removed by Windows (link lost);
    /// - the pad refuses our reports (raw writer / Unity backend counters);
    /// - a PlayStation pad on Bluetooth gets nothing from the Unity backend (known limit);
    /// - several drops in the last minute (unstable link);
    /// - no input report for a while (only a hint: a resting pad may send nothing, so it asks for a button press).
    /// Debug panel only (OnGUI).
    /// </summary>
    public static class RumbleBluetoothCheck
    {
        private const float RecentDropSeconds = 30f;
        private const int UnstableDropCount = 2;
        private const float SilentSeconds = 5f;
        private const int RefusalsWorthWarning = 8;

        /// <summary>A Bluetooth failure that stops output; null if none.</summary>
        public static string FindProblem(RumbleService _rumble)
        {
            RumbleBluetoothHealth health = _rumble.DebugBluetoothHealth;
            bool hasNoPad = _rumble.DebugDevice == "none";
            if (hasNoPad && health.TotalDrops > 0 && health.SecondsSinceLastDrop < RecentDropSeconds)
            {
                return "the " + health.LastDroppedModel + " on Bluetooth disconnected " + health.SecondsSinceLastDrop.ToString("0")
                    + " s ago. If its light is still on, Windows lost the link: turn the pad off and on, or re-pair it in "
                    + "Windows Bluetooth settings. A USB cable always works.";
            }

            bool isOnBluetooth = RumblePadTypeLine.ReadLiveConnection(_rumble) == RumbleConnection.Bluetooth;
            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (isOnBluetooth && writer != null && writer.IsUnusable)
            {
                return "the pad is connected over Bluetooth but refused every report we sent (both formats). Turn it off and "
                    + "on; if it repeats, use USB and send screenshots of F9 details and F10.";
            }

            UnityInputSystemRumbleOutput unityOutput = _rumble.DebugUnityOutput;
            if (unityOutput != null && unityOutput.IsSkippingBluetoothPad)
            {
                return "the " + _rumble.DebugPadIdentity.Model + " is connected over Bluetooth, but the Unity backend can't "
                    + "send to it, so nothing is sent (known limit for DualShock 4). Turn Steam Input on, or use USB.";
            }

            if (isOnBluetooth && unityOutput != null && unityOutput.HasGivenUp)
            {
                return "the pad is connected over Bluetooth but refused every send; output stopped. Turn it off and on, "
                    + "close DS4Windows / DSX, or use USB.";
            }

            return null;
        }

        /// <summary>A Bluetooth warning that may clear by itself; null if none.</summary>
        public static string FindWait(RumbleService _rumble)
        {
            bool isOnBluetooth = RumblePadTypeLine.ReadLiveConnection(_rumble) == RumbleConnection.Bluetooth;
            if (isOnBluetooth == false)
            {
                return null;
            }

            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null && writer.RefusedInARow >= RefusalsWorthWarning)
            {
                return "connected, but the pad is refusing our Bluetooth reports (" + writer.RefusedInARow + " in a row, "
                    + "trying another size / format). Still here after 5 s = failed: screenshot F9 details.";
            }

            RumbleBluetoothHealth health = _rumble.DebugBluetoothHealth;
            int recentDrops = health.DropsInLastMinute;
            if (recentDrops >= UnstableDropCount)
            {
                return "the link dropped " + recentDrops + " times in the last minute (unstable). Move closer to the "
                    + "PC / Bluetooth adapter, away from USB 3 ports, or use USB.";
            }

            float silentFor = health.SecondsWithoutInput;
            if (silentFor >= SilentSeconds)
            {
                return "no data from the pad for " + silentFor.ToString("0") + " s. Press any button: if this stays while "
                    + "you press, the link is stuck. Turn the pad off and on, or use USB.";
            }

            return null;
        }

        /// <summary>The raw numbers behind the Bluetooth verdict, for the details view.</summary>
        public static void AppendDetails(RumbleService _rumble, StringBuilder _text)
        {
            RumbleBluetoothHealth health = _rumble.DebugBluetoothHealth;
            _text.Append("  Bluetooth link: drops this session ").Append(health.TotalDrops);
            _text.Append(", in the last minute ").Append(health.DropsInLastMinute);
            if (health.TotalDrops > 0)
            {
                _text.Append(" (last: ").Append(health.LastDroppedModel).Append(", ");
                _text.Append(health.SecondsSinceLastDrop.ToString("0")).Append(" s ago)");
            }

            _text.Append(";  last input from the pad ").Append(health.SecondsWithoutInput.ToString("0.0")).Append(" s ago\n");
            _text.Append("  Bluetooth flag: ");
            string problem = FindProblem(_rumble);
            if (problem != null)
            {
                _text.Append("<color=#ff7a3d>FAILING</color> - ").Append(problem).Append('\n');
                return;
            }

            string warning = FindWait(_rumble);
            if (warning != null)
            {
                _text.Append("<color=#ffd060>WARNING</color> - ").Append(warning).Append('\n');
                return;
            }

            _text.Append("none\n");
        }
    }
}
