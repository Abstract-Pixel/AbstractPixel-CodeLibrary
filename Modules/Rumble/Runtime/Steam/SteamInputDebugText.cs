using System;
using System.Text;
using Steamworks;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The Steam Input part of the F9 details and the bind log: API state, whether Steam launched this process,
    /// every pad Steam lists (type, XInput slot, which one is bound) and the game's opt-in mask.
    /// Plain text (no rich-text tags) so the same lines work in Player.log. Debug panel and bind time only.
    /// </summary>
    public static class SteamInputDebugText
    {
        private const string SteamTypePrefix = "k_ESteamInputType_";

        public static void Append(SteamPadList _pads, InputHandle_t _boundHandle, bool _isInitialised, StringBuilder _text)
        {
            if (_isInitialised == false)
            {
                _text.Append("  SteamInput.Init: not done or failed (Steam not running?)\n");
                return;
            }

            try
            {
                AppendProcessState(_text);
                AppendPads(_pads, _boundHandle, _text);
                AppendOptInMask(_text);
            }
            catch (Exception exception)
            {
                _text.Append("  Steam API error: ").Append(exception.Message).Append('\n');
            }
        }

        /// <summary>
        /// The overlay is injected only into processes Steam launched. Without it Steam also hands no pads to the
        /// game, so "overlay off" + "lists 0 pads" means "start the game from Steam".
        /// </summary>
        private static void AppendProcessState(StringBuilder _text)
        {
            bool isOverlayOn = SteamUtils.IsOverlayEnabled();
            bool isOnDeck = SteamUtils.IsSteamRunningOnSteamDeck();
            _text.Append("  SteamInput.Init ok.  Overlay ").Append(OnOff(isOverlayOn));
            if (isOverlayOn == false)
            {
                _text.Append(" (process probably not launched by Steam, or overlay disabled)");
            }

            _text.Append("   Steam Deck ").Append(OnOff(isOnDeck)).Append('\n');
        }

        private static void AppendPads(SteamPadList _pads, InputHandle_t _boundHandle, StringBuilder _text)
        {
            _text.Append("  Steam lists ").Append(_pads.Count).Append(" pad(s) to this game");
            if (_pads.Count == 0)
            {
                _text.Append(" (= Steam Input is off for the connected pads, or Steam did not launch the game)\n");
                return;
            }

            _text.Append('\n');
            for (int index = 0; index < _pads.Count; index++)
            {
                InputHandle_t handle = _pads.HandleAt(index);
                ESteamInputType inputType = SteamInput.GetInputTypeForHandle(handle);
                int xinputSlot = SteamInput.GetGamepadIndexForController(handle);
                _text.Append("    [").Append(index).Append("] ").Append(ShortType(inputType));
                _text.Append("  XInput slot ").Append(xinputSlot);
                if (handle.m_InputHandle == _boundHandle.m_InputHandle)
                {
                    _text.Append("  <- BOUND");
                }

                _text.Append('\n');
            }
        }

        /// <summary>
        /// GetSessionInputConfigurationSettings: the game's Steamworks default plus the player's per-game override.
        /// It does NOT include Steam's global per-family settings, so it is shown as information only and never
        /// decides the backend (deciding with it was the Nov 2026 regression).
        /// </summary>
        private static void AppendOptInMask(StringBuilder _text)
        {
            int mask = SteamInput.GetSessionInputConfigurationSettings();
            _text.Append("  Game opt-in mask 0x").Append(mask.ToString("X"));
            _text.Append(" (PlayStation ").Append(OnOff(HasBit(mask, ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Playstation)));
            _text.Append(", Xbox ").Append(OnOff(HasBit(mask, ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Xbox)));
            _text.Append(", Generic ").Append(OnOff(HasBit(mask, ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Generic)));
            _text.Append(", Switch ").Append(OnOff(HasBit(mask, ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Switch)));
            _text.Append(")  info only: the pads Steam lists are what counts\n");
        }

        public static string ShortType(ESteamInputType _inputType)
        {
            string fullName = _inputType.ToString();
            bool hasPrefix = fullName.StartsWith(SteamTypePrefix, StringComparison.Ordinal);
            if (hasPrefix == false)
            {
                return fullName;
            }

            return fullName.Substring(SteamTypePrefix.Length);
        }

        private static bool HasBit(int _mask, ESteamInputConfigurationEnableType _bit)
        {
            return (_mask & (int)_bit) != 0;
        }

        private static string OnOff(bool _value)
        {
            if (_value)
            {
                return "on";
            }

            return "off";
        }
    }
}
