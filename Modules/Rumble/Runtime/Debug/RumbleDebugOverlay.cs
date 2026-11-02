using System.Text;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// In-game rumble debug panel (Editor and development builds only). Press the toggle key (F9) to show it.
    /// Lists everything feeding the controller right now, so conflicts are easy to spot:
    /// - every playing one-shot pattern: name, strength, current heavy/light value, time left, and its flags
    ///   (mutes driving rumble, flashes the light bar, buzzes triggers);
    /// - every registered debug source (e.g. the car's continuous driving rumble) with its own breakdown;
    /// - the final mix: requested continuous, how much patterns mute it, what was mixed and what reached the pad.
    /// Hidden, it only checks one key per frame and draws nothing.
    /// </summary>
    public class RumbleDebugOverlay : MonoBehaviour
    {
        private const float PanelWidth = 560f;
        private const float Margin = 12f;

        [Tooltip("Key that shows / hides the panel.")]
        [SerializeField] private Key toggleKey = Key.F9;

        [Tooltip("Show the panel as soon as the game starts.")]
        [SerializeField] private bool startVisible = false;

        [Tooltip("Text size of the panel.")]
        [SerializeField, Range(10, 28)] private int fontSize = 15;

        private readonly StringBuilder text = new StringBuilder(1024);
        private GUIStyle panelStyle;
        private bool isVisible;

        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
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
                isVisible = isVisible == false;
            }
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
            text.Append("<b>RUMBLE DEBUG</b>  (").Append(toggleKey).Append(" hides)\n");
            text.Append("Pad: ").Append(_rumble.DebugDevice).Append("  ->  ").Append(_rumble.DebugBackend).Append('\n');
            AppendSilence(_rumble);
            text.Append('\n');
            AppendPatterns(_rumble);
            text.Append('\n');
            AppendDebugSources(_rumble);
            text.Append('\n');
            AppendMix(_rumble);
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
            if (panelStyle != null)
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
            Texture2D background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.78f));
            background.Apply();
            panelStyle.normal.background = background;
        }
    }
}
