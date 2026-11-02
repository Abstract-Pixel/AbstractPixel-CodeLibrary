using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The top of the F9 panel, for testers: one verdict line (WORKING / WAIT / NOT WORKING and what to do; Bluetooth
    /// failures in orange as "· BLUETOOTH"), then what the pad itself should be showing right now. If the pad doesn't
    /// match, the output isn't reaching it.
    /// Debug panel only (OnGUI); allocates a few strings per draw.
    /// </summary>
    public static class RumblePadCheck
    {
        private const string Good = "#70ff70";
        private const string Wait = "#ffd060";
        private const string Bad = "#ff7070";
        // Bluetooth failures get their own orange so they stand out from every other verdict.
        private const string BluetoothBad = "#ff8a3d";
        private const int RefusalsWorthWarning = 8;
        private const float MotorOffThreshold = 0.01f;

        public static void AppendVerdict(RumbleService _rumble, StringBuilder _text)
        {
            string bluetoothProblem = RumbleBluetoothCheck.FindProblem(_rumble);
            if (bluetoothProblem != null)
            {
                AppendLine(_text, BluetoothBad, "NOT WORKING · BLUETOOTH", bluetoothProblem);
                return;
            }

            string problem = FindProblem(_rumble);
            if (problem != null)
            {
                AppendLine(_text, Bad, "NOT WORKING", problem);
                return;
            }

            string bluetoothWait = RumbleBluetoothCheck.FindWait(_rumble);
            if (bluetoothWait != null)
            {
                AppendLine(_text, BluetoothBad, "WAIT · BLUETOOTH", bluetoothWait);
                return;
            }

            string waitReason = FindWaitReason(_rumble);
            if (waitReason != null)
            {
                AppendLine(_text, Wait, "WAIT", waitReason);
                return;
            }

            AppendLine(_text, Good, "WORKING", DescribeWorking(_rumble));
        }

        public static void AppendExpectedOnPad(RumbleService _rumble, StringBuilder _text)
        {
            _text.Append("\n<b>THE PAD SHOULD SHOW NOW</b>  (if it doesn't match, it's not working)\n");
            AppendLightBar(_rumble, _text);
            AppendPlayerLeds(_rumble, _text);
            AppendRumble(_rumble, _text);
            AppendTriggers(_rumble, _text);
        }

        // ---------- Verdict ----------

        /// <summary>The first thing that stops output reaching the pad, in plain words with the fix; null if none.</summary>
        private static string FindProblem(RumbleService _rumble)
        {
            if (_rumble.DebugDevice == "none")
            {
                return "no controller found. Connect a pad and press a button on it. (Paired over Bluetooth but not "
                    + "found? Windows hasn't connected it: press the pad's home button to wake it, or re-pair it.)";
            }

            if (_rumble.DebugAppliedMode == RumbleBackendMode.Off)
            {
                return "backend mode is Off (RumbleOutputProfile or -rumble-backend=off).";
            }

            if (_rumble.DebugBackend == "none")
            {
                return "no backend could take this pad (" + _rumble.DebugBackendReason + ").";
            }

            // Bluetooth cases (pad on Bluetooth that gets nothing, refusals over Bluetooth) are found first by
            // RumbleBluetoothCheck; these are the same failures on USB or an unknown connection.
            UnityInputSystemRumbleOutput unityOutput = _rumble.DebugUnityOutput;
            if (unityOutput != null && unityOutput.HasGivenUp)
            {
                return "the pad refused every send; output stopped. Turn the pad off and on. If it repeats, close "
                    + "DS4Windows / DSX or use a USB cable.";
            }

            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null && writer.IsUnusable)
            {
                return "the DualSense refused our reports in every format: rumble only, no light bar / LEDs / triggers.";
            }

            if (_rumble.DebugKilledCapabilities != RumbleCapabilities.None)
            {
                return _rumble.DebugKilledCapabilities + " failed on this pad and was switched off. The rest still works.";
            }

            return null;
        }

        /// <summary>A normal, temporary state where the pad is not showing our output yet; null if none.</summary>
        private static string FindWaitReason(RumbleService _rumble)
        {
            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null)
            {
                float holdLeft = writer.BluetoothLightHoldSecondsLeft;
                if (holdLeft > 0f)
                {
                    return "Bluetooth pad is playing its own connection light. The game's colour starts in "
                        + holdLeft.ToString("0") + " s (rumble already works).";
                }

                if (writer.RefusedInARow >= RefusalsWorthWarning)
                {
                    return "the pad is refusing reports right now (" + writer.RefusedInARow + " in a row, trying "
                        + "another format). If this stays for more than 5 s, screenshot the panel.";
                }
            }

            if (_rumble.IsFocusSilenced)
            {
                return "game window is not focused: motors are off on purpose. Click into the game.";
            }

            return null;
        }

        private static string DescribeWorking(RumbleService _rumble)
        {
            bool isSteam = _rumble.DebugBackend.StartsWith("Steam");
            if (isSteam)
            {
                return "Steam owns the pad and takes our output (Steam can't report failures, so check the pad below).";
            }

            bool hasLightBar = (_rumble.ActiveCapabilities & RumbleCapabilities.LightBar) != 0;
            if (hasLightBar == false)
            {
                return "rumble only: this pad has no light bar. (A PlayStation pad here means Steam Input or DS4Windows "
                    + "is disguising it as an Xbox pad.)";
            }

            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null)
            {
                return "the DualSense accepts our reports (" + writer.Transport + ", " + writer.WritesSent + " sent, "
                    + writer.WritesRefused + " refused).";
            }

            return "the pad accepts what we send.";
        }

        private static void AppendLine(StringBuilder _text, string _color, string _label, string _detail)
        {
            _text.Append("<b><color=").Append(_color).Append('>').Append(_label).Append("</color></b>  ");
            _text.Append(_detail).Append('\n');
        }

        // ---------- What the pad should show ----------

        private static void AppendLightBar(RumbleService _rumble, StringBuilder _text)
        {
            _text.Append("  Light bar:  ");
            bool hasLightBar = (_rumble.ActiveCapabilities & RumbleCapabilities.LightBar) != 0;
            if (hasLightBar == false)
            {
                _text.Append("not controlled on this pad\n");
                return;
            }

            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null && writer.BluetoothLightHoldSecondsLeft > 0f)
            {
                _text.Append("the pad's own blue connection light (ours comes next)\n");
                return;
            }

            string hex = RumbleColorName.Hex(_rumble.DebugLightBar);
            _text.Append("<color=").Append(hex).Append(">██████</color>  <b>").Append(RumbleColorName.Describe(_rumble.DebugLightBar));
            _text.Append("</b>  ").Append(hex).Append("   (breathing states move between dim and bright)\n");
        }

        private static void AppendPlayerLeds(RumbleService _rumble, StringBuilder _text)
        {
            bool hasLightBar = (_rumble.ActiveCapabilities & RumbleCapabilities.LightBar) != 0;
            bool hasPlayerLeds = (_rumble.ActiveCapabilities & RumbleCapabilities.PlayerLeds) != 0;
            if (hasLightBar == false && hasPlayerLeds == false)
            {
                return;
            }

            _text.Append("  Player LEDs:  ");
            if (hasPlayerLeds == false)
            {
                _text.Append("not controlled by the game here (Steam has no LED call; the pad keeps its own)\n");
                return;
            }

            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer != null && writer.BluetoothLightHoldSecondsLeft > 0f)
            {
                _text.Append("unchanged until the connection light ends\n");
                return;
            }

            int litCount = CountLit(_rumble);
            if (litCount == 0)
            {
                _text.Append("<b>all off</b>\n");
                return;
            }

            _text.Append("<b>").Append(litCount).Append(" of 5 lit</b>\n");
        }

        private static void AppendRumble(RumbleService _rumble, StringBuilder _text)
        {
            _text.Append("  Rumble:  ");
            if (_rumble.IsFocusSilenced)
            {
                _text.Append("<b>off</b> (window not focused)\n");
                return;
            }

            bool isOff = _rumble.SentLow < MotorOffThreshold && _rumble.SentHigh < MotorOffThreshold;
            if (isOff)
            {
                _text.Append("<b>off</b>\n");
                return;
            }

            _text.Append("<b>on</b>  heavy ").Append(_rumble.SentLow.ToString("0.00"));
            _text.Append("  light ").Append(_rumble.SentHigh.ToString("0.00")).Append('\n');
        }

        private static void AppendTriggers(RumbleService _rumble, StringBuilder _text)
        {
            bool hasTriggers = (_rumble.ActiveCapabilities & RumbleCapabilities.AdaptiveTriggers) != 0;
            if (hasTriggers == false)
            {
                return;
            }

            _text.Append("  Triggers:  ");
            if (_rumble.IsGameSilenced)
            {
                _text.Append("<b>free</b> (paused / menu / finished)\n");
                return;
            }

            bool isBrakeOff = _rumble.RequestedBrake.IsEffectivelyOff;
            bool isThrottleOff = _rumble.RequestedThrottle.IsEffectivelyOff;
            if (isBrakeOff && isThrottleOff)
            {
                _text.Append("<b>free</b>\n");
                return;
            }

            _text.Append("<b>L2 ").Append(DescribeTrigger(_rumble.RequestedBrake)).Append(", R2 ");
            _text.Append(DescribeTrigger(_rumble.RequestedThrottle)).Append("</b>\n");
        }

        private static string DescribeTrigger(TriggerEffect _effect)
        {
            if (_effect.IsEffectivelyOff)
            {
                return "free";
            }

            return _effect.Mode.ToString().ToLowerInvariant();
        }

        private static int CountLit(RumbleService _rumble)
        {
            if (_rumble.HasPlayerLedsRequest == false)
            {
                return 0;
            }

            int litCount = 0;
            for (int bit = 0; bit < 5; bit++)
            {
                bool isLit = (_rumble.RequestedPlayerLeds & (1 << bit)) != 0;
                if (isLit)
                {
                    litCount++;
                }
            }

            return litCount;
        }
    }
}
