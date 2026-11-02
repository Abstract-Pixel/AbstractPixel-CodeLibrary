using System.Text;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// In-game rumble debug panel (Editor and development builds only). The toggle key (F9) cycles:
    /// short check (a WORKING / WAIT / NOT WORKING verdict, Steam Input on/off, fallback active/none, and what the pad
    /// should show) -> full details -> hidden.
    /// The details list everything feeding the controller right now, so conflicts are easy to spot:
    /// - every playing one-shot pattern: name, strength, current heavy/light value, time left, and its flags
    ///   (mutes driving rumble, flashes the light bar, buzzes triggers);
    /// - every registered debug source (e.g. the car's continuous driving rumble) with its own breakdown;
    /// - the final mix: requested continuous, how much patterns mute it, what was mixed and what reached the pad.
    /// Hidden, it only checks one key per frame and draws nothing.
    /// </summary>
    public class RumbleDebugOverlay : MonoBehaviour
    {
        private const float PanelWidth = 680f;
        private const float Margin = 12f;

        [Tooltip("Key that cycles the panel: short check -> full details -> hidden.")]
        [SerializeField] private Key toggleKey = Key.F9;

        [Tooltip("Show the panel as soon as the game starts.")]
        [SerializeField] private bool startVisible = false;

        [Tooltip("Text size of the panel.")]
        [SerializeField, Range(10, 28)] private int fontSize = 15;

        [Tooltip("Key that shows / hides the raw DualSense report bytes (Unity backend).")]
        [SerializeField] private Key bytesKey = Key.F10;

        [Tooltip("Key that runs the Bluetooth self-test (CRC, layout, command bytes) and shows the result.")]
        [SerializeField] private Key selfTestKey = Key.F11;

        private readonly StringBuilder text = new StringBuilder(2048);
        private readonly byte[] reportBytes = new byte[DualSenseReportDump.BluetoothReportSize];
        private string selfTestResult = string.Empty;
        private GUIStyle panelStyle;
        private Texture2D panelBackground;
        private bool isVisible;
        private bool isShowingDetails;
        private bool showBytes;

        /// <summary>Showing it from code opens the full details (what capture probes want).</summary>
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                isShowingDetails = value;
            }
        }

        public bool ShowBytes
        {
            get { return showBytes; }
            set { showBytes = value; }
        }

        /// <summary>Runs the Bluetooth self-test and keeps its report for the panel.</summary>
        public void RunSelfTest()
        {
            StringBuilder report = new StringBuilder(512);
            DualSenseBluetoothSelfTest.Run(report);
            selfTestResult = report.ToString();
            Debug.Log("[Rumble] " + selfTestResult);
        }

        private void Awake()
        {
            // Debug tool only: in a release build the component switches itself off and costs nothing.
            if (Debug.isDebugBuild == false)
            {
                enabled = false;
                return;
            }

            isVisible = startVisible;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            bool isTogglePressed = keyboard[toggleKey].wasPressedThisFrame;
            if (isTogglePressed)
            {
                CycleView();
            }

            if (isVisible == false)
            {
                return;
            }

            if (keyboard[bytesKey].wasPressedThisFrame)
            {
                showBytes = showBytes == false;
            }

            if (keyboard[selfTestKey].wasPressedThisFrame)
            {
                RunSelfTest();
            }
        }

        /// <summary>Hidden -> short check -> full details -> hidden.</summary>
        private void CycleView()
        {
            if (isVisible == false)
            {
                isVisible = true;
                isShowingDetails = false;
                return;
            }

            if (isShowingDetails == false)
            {
                isShowingDetails = true;
                return;
            }

            isVisible = false;
            isShowingDetails = false;
        }

        private void OnGUI()
        {
            if (isVisible == false)
            {
                return;
            }

            bool hasService = ServiceLocator.TryGet(out RumbleService rumble);
            if (hasService == false)
            {
                return;
            }

            EnsureStyle();
            BuildText(rumble);
            GUIContent content = new GUIContent(text.ToString());
            float height = panelStyle.CalcHeight(content, PanelWidth);
            Rect panelRect = new Rect(Margin, Margin, PanelWidth, height);
            GUI.Box(panelRect, content, panelStyle);
        }

        private void BuildText(RumbleService _rumble)
        {
            text.Clear();
            RumblePadTypeLine.AppendShort(_rumble, text);
            RumblePadCheck.AppendVerdict(_rumble, text);
            RumbleSteamLines.AppendShort(_rumble, text);
            RumblePadCheck.AppendExpectedOnPad(_rumble, text);
            if (isShowingDetails == false)
            {
                text.Append("\n(").Append(toggleKey).Append(" again: details for a bug report)");
                return;
            }

            text.Append("\n<b>DETAILS</b>  (").Append(toggleKey).Append(" hides)\n");
            RumblePadTypeLine.AppendDetails(_rumble, text);
            RumbleSteamLines.AppendDetails(_rumble, text);
            AppendBackend(_rumble);
            AppendDualSenseReport(_rumble);
            AppendSilence(_rumble);
            text.Append('\n');
            AppendPatterns(_rumble);
            text.Append('\n');
            AppendDebugSources(_rumble);
            text.Append('\n');
            AppendMix(_rumble);
            if (selfTestResult.Length > 0)
            {
                text.Append('\n').Append(selfTestResult);
            }
            else
            {
                text.Append("\n(").Append(selfTestKey).Append(" runs the Bluetooth self-test)\n");
            }
        }

        /// <summary>What the bound pad can do, and what was switched off (the backend rule is in RumbleSteamLines).</summary>
        private void AppendBackend(RumbleService _rumble)
        {
            text.Append("Caps: ").Append(_rumble.ActiveCapabilities);
            if (_rumble.DebugKilledCapabilities != RumbleCapabilities.None)
            {
                text.Append("   <color=#ff8080>failed: ").Append(_rumble.DebugKilledCapabilities).Append("</color>");
            }

            text.Append('\n');
            text.Append("Light bar ").Append(ColorHex(_rumble.DebugLightBar));
            text.Append("   player LEDs ").Append(LedDots(_rumble.RequestedPlayerLeds, _rumble.HasPlayerLedsRequest)).Append('\n');
            AppendHints(_rumble);
        }

        /// <summary>Plain-language warnings for the setups that silently break the light bar or LEDs.</summary>
        private void AppendHints(RumbleService _rumble)
        {
            bool hasLightBar = (_rumble.ActiveCapabilities & RumbleCapabilities.LightBar) != 0;
            bool isPadBound = _rumble.DebugDevice != "none";
            if (isPadBound && hasLightBar == false)
            {
                text.Append("<color=#ffd060>No light bar on this pad. A DualSense that shows up as an Xbox/XInput pad means Steam Input is wrapping it: use the Steam backend, or turn Steam Input off for the game.</color>\n");
            }

            bool hasSteamLightBar = _rumble.DebugBackend.StartsWith("Steam") && hasLightBar;
            if (hasSteamLightBar)
            {
                text.Append("<color=#a0a0a0>Steam: player LEDs have no Steam API; light bar re-sent every 2 s.</color>\n");
            }
        }

        /// <summary>Unity backend on a DualSense: the raw report as it goes to the driver, so USB/Bluetooth can be checked.</summary>
        private void AppendDualSenseReport(RumbleService _rumble)
        {
            DualSenseRawWriter writer = _rumble.DebugDualSenseWriter;
            if (writer == null)
            {
                return;
            }

            text.Append("<b>DualSense raw report</b>  sending ").Append(writer.Transport);
            text.Append(" (guessed ").Append(writer.GuessedTransport).Append(" from descriptor output size ").Append(writer.DescriptorOutputReportSize).Append(")\n");
            text.Append("  sent ").Append(writer.WritesSent).Append("  refused ").Append(writer.WritesRefused);
            text.Append("  in a row ").Append(writer.RefusedInARow).Append("  last result ").Append(writer.LastResult);
            if (writer.HasSwitchedFormat)
            {
                text.Append("  <color=#ffd060>format switched</color>");
            }

            if (writer.IsUnusable)
            {
                text.Append("  <color=#ff8080>UNUSABLE</color>");
            }

            text.Append('\n');
            if (showBytes == false)
            {
                text.Append("  (").Append(bytesKey).Append(" shows the bytes)\n");
                return;
            }

            AppendReportBytes(writer);
        }

        private void AppendReportBytes(DualSenseRawWriter _writer)
        {
            if (_writer.Transport == DualSenseTransport.Bluetooth)
            {
                DualSenseReportDump.FillBluetooth(_writer.LastState, _writer.LastSequence, reportBytes);
                text.Append("  Bluetooth 0x31 (CRC in 74..77):");
                DualSenseReportDump.AppendHex(text, reportBytes, DualSenseReportDump.BluetoothReportSize);
                return;
            }

            DualSenseReportDump.FillUsb(_writer.LastState, reportBytes);
            text.Append("  USB 0x02:");
            DualSenseReportDump.AppendHex(text, reportBytes, DualSenseReportDump.UsbReportSize);
        }

        private static string LedDots(byte _mask, bool _isRequested)
        {
            if (_isRequested == false)
            {
                return "not driven";
            }

            char[] dots = new char[5];
            for (int bit = 0; bit < 5; bit++)
            {
                bool isLit = (_mask & (1 << bit)) != 0;
                dots[bit] = '.';
                if (isLit)
                {
                    dots[bit] = 'o';
                }
            }

            return new string(dots);
        }

        private static string ColorHex(Color _color)
        {
            return RumbleColorName.Hex(_color);
        }

        private void AppendSilence(RumbleService _rumble)
        {
            text.Append("Silence: game ").Append(OnOff(_rumble.IsGameSilenced));
            text.Append(" (driving + triggers off, one-shots still play)   focus ").Append(OnOff(_rumble.IsFocusSilenced));
            text.Append(" (all motors off)\n");
        }

        private void AppendPatterns(RumbleService _rumble)
        {
            text.Append("<b>One-shot patterns</b>  (").Append(_rumble.PatternSlotCount).Append(" slots; the strongest value wins per motor)\n");
            int playingCount = 0;
            for (int slot = 0; slot < _rumble.PatternSlotCount; slot++)
            {
                bool isPlaying = _rumble.TryReadPattern(slot, out RumbleActivePattern active);
                if (isPlaying == false)
                {
                    continue;
                }

                playingCount++;
                AppendPattern(slot, active);
            }

            if (playingCount == 0)
            {
                text.Append("  none\n");
            }
        }

        private void AppendPattern(int _slot, RumbleActivePattern _active)
        {
            text.Append("  [").Append(_slot).Append("] <b>").Append(_active.Pattern.name).Append("</b>");
            text.Append("  str ").Append(_active.Strength.ToString("0.00"));
            text.Append("  heavy ").Append(_active.CurrentLow.ToString("0.00"));
            text.Append("  light ").Append(_active.CurrentHigh.ToString("0.00"));
            text.Append("  left ").Append(_active.Remaining.ToString("0.00")).Append("s / ").Append(_active.Pattern.Duration.ToString("0.00")).Append('s');
            AppendPatternFlags(_active.Pattern);
            text.Append('\n');
        }

        private void AppendPatternFlags(RumblePattern _pattern)
        {
            if (_pattern.MutesDrivingRumble)
            {
                text.Append("  MUTES-DRIVING");
            }

            if (_pattern.FlashesLightBar)
            {
                text.Append("  LIGHT-BAR");
            }

            if (_pattern.BuzzesTriggers)
            {
                text.Append("  TRIGGER-BUZZ");
            }
        }

        private void AppendDebugSources(RumbleService _rumble)
        {
            text.Append("<b>Continuous sources</b>\n");
            if (_rumble.DebugSourceCount == 0)
            {
                text.Append("  none registered\n");
                return;
            }

            for (int index = 0; index < _rumble.DebugSourceCount; index++)
            {
                IRumbleDebugSource source = _rumble.GetDebugSource(index);
                if (source == null)
                {
                    continue;
                }

                text.Append("  <b>").Append(source.DebugName).Append("</b>  ").Append(source.ReadDebugLine()).Append('\n');
            }
        }

        private void AppendMix(RumbleService _rumble)
        {
            text.Append("<b>Mix</b>  (heavy / light)\n");
            text.Append("  continuous requested ").Append(Pair(_rumble.RequestedContinuousLow, _rumble.RequestedContinuousHigh));
            text.Append("   muted by patterns ").Append((_rumble.PatternDuckAmount * 100f).ToString("0")).Append("%\n");
            text.Append("  mixed ").Append(Pair(_rumble.MixedLow, _rumble.MixedHigh));
            text.Append("   sent to pad ").Append(Pair(_rumble.SentLow, _rumble.SentHigh)).Append("  (after backend boost + player slider)\n");
            text.Append("  triggers requested  L2 ").Append(DescribeTrigger(_rumble.RequestedBrake));
            text.Append("   R2 ").Append(DescribeTrigger(_rumble.RequestedThrottle)).Append('\n');
        }

        private static string Pair(float _low, float _high)
        {
            return _low.ToString("0.00") + " / " + _high.ToString("0.00");
        }

        private static string DescribeTrigger(TriggerEffect _effect)
        {
            if (_effect.IsEffectivelyOff)
            {
                return "off";
            }

            return _effect.Mode + " " + _effect.Strength.ToString("0.00");
        }

        private static string OnOff(bool _value)
        {
            if (_value)
            {
                return "ON";
            }

            return "off";
        }

        private void EnsureStyle()
        {
            // A scene change runs Resources.UnloadUnusedAssets, which frees a runtime texture that only a GUIStyle
            // holds: the panel then loses its background. Rebuild if that happened (the flag below prevents it).
            bool hasBackground = panelBackground != null;
            if (panelStyle != null && hasBackground)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.alignment = TextAnchor.UpperLeft;
            panelStyle.richText = true;
            panelStyle.wordWrap = true;
            panelStyle.fontSize = fontSize;
            panelStyle.padding = new RectOffset(10, 10, 8, 8);
            panelStyle.normal.textColor = Color.white;
            panelBackground = new Texture2D(1, 1);
            panelBackground.hideFlags = HideFlags.DontUnloadUnusedAsset;
            panelBackground.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.78f));
            panelBackground.Apply();
            panelStyle.normal.background = panelBackground;
        }

        private void OnDestroy()
        {
            if (panelBackground != null)
            {
                Destroy(panelBackground);
            }
        }
    }
}
