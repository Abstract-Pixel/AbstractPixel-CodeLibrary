using System;
using System.Text;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Main backend through Steamworks ISteamInput. Binds the Steam controller in the player's hands (SteamPadList
    /// picks it). Every native call re-checks Steam readiness, uses only handles from this session, and never sends
    /// to handle 0.
    /// When to use Steam: whenever Steam lists the pad to this game. Steam only hands a game the pads it drives with
    /// Steam Input, so a listed pad IS a Steam Input pad, over USB or Bluetooth alike (Steam handles the transport).
    /// The game's opt-in mask (GetSessionInputConfigurationSettings) is shown in the F9 details only: it does not
    /// include Steam's global per-family settings and wrongly said "off" for a pad Steam was driving.
    /// Player LEDs: ISteamInput has no call for them (only SetLEDColor for the light bar), so they are not offered here.
    /// </summary>
    public class SteamInputRumbleOutput : IRumbleOutput, IRumbleBackendStatus
    {
        private readonly Func<bool> isSteamReady;
        private readonly SteamPadList pads = new SteamPadList();
        private readonly DualSenseTriggerParamWriter triggerWriter = new DualSenseTriggerParamWriter();

        private bool isSteamInputInitialised;
        private bool hasLoggedBindFailure;
        private InputHandle_t boundHandle;
        private string displayName = "Steam Input (none)";
        private string statusLine = "not tried";
        private SteamInputState lastBindState = SteamInputState.NotChecked;
        private RumbleCapabilities capabilities = RumbleCapabilities.None;
        private bool isBoundPadBoosted;

        public SteamInputRumbleOutput(Func<bool> _isSteamReady)
        {
            isSteamReady = _isSteamReady;
        }

        public string DisplayName => displayName;
        public RumbleCapabilities Capabilities => capabilities;

        private bool IsBound => boundHandle.m_InputHandle != 0;

        // ---------- IRumbleBackendStatus ----------

        public bool IsAvailable
        {
            get
            {
                try
                {
                    return EnsureSteamInput();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>False once Steam no longer lists the bound handle (pad unplugged, or reconnected with a new handle).</summary>
        public bool IsBoundPadStillConnected
        {
            get
            {
                if (CanCall() == false)
                {
                    return false;
                }

                try
                {
                    pads.Refresh();
                    return pads.Contains(boundHandle);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public SteamInputState LastBindState => lastBindState;
        public bool IsBoundPadBoosted => isBoundPadBoosted;
        public int ListedPadCount => pads.Count;
        public string StatusLine => statusLine;

        public void AppendDebugDetails(StringBuilder _text)
        {
            if (ReadSteamReady() == false)
            {
                _text.Append("  Steam is not running (SteamAPI not initialised)\n");
                return;
            }

            SteamInputDebugText.Append(pads, boundHandle, isSteamInputInitialised, _text);
        }

        public bool TryBind(InputDevice _activeDevice)
        {
            Unbind();
            try
            {
                if (EnsureSteamInput() == false)
                {
                    SetBindResult(SteamInputState.NotRunning, "Steam not running or SteamInput.Init failed");
                    return false;
                }

                SteamInput.RunFrame();
                pads.Refresh();
                if (pads.Count == 0)
                {
                    SetBindResult(SteamInputState.NoPadListed, "Steam lists no pad to this game");
                    return false;
                }

                int handleIndex = pads.FindIndexFor(_activeDevice);
                if (handleIndex < 0)
                {
                    SetBindResult(SteamInputState.PadNotMatched, "Steam lists " + pads.Count + " pads; can't tell which one is in your hands");
                    return false;
                }

                InputHandle_t handle = pads.HandleAt(handleIndex);
                ESteamInputType inputType = SteamInput.GetInputTypeForHandle(handle);
                RumbleCapabilities typeCapabilities = MapCapabilities(inputType);
                if (typeCapabilities == RumbleCapabilities.None)
                {
                    SetBindResult(SteamInputState.PadTypeNotHandled, "Steam leaves " + SteamInputDebugText.ShortType(inputType) + " to Unity");
                    return false;
                }

                boundHandle = handle;
                capabilities = typeCapabilities;
                isBoundPadBoosted = IsXboxType(inputType) == false;
                displayName = "Steam Input (" + inputType + ")";
                if ((capabilities & RumbleCapabilities.AdaptiveTriggers) != 0)
                {
                    triggerWriter.Allocate();
                }

                SetBindResult(SteamInputState.Serving, "bound " + SteamInputDebugText.ShortType(inputType));
                return true;
            }
            catch (Exception exception)
            {
                // Bind is retried about once a second, so a lasting Steam problem is logged only once.
                if (hasLoggedBindFailure == false)
                {
                    hasLoggedBindFailure = true;
                    Debug.LogWarning("[Rumble] Steam Input bind failed: " + exception.Message);
                }

                Unbind();
                SetBindResult(SteamInputState.NotRunning, "Steam Input call failed: " + exception.Message);
                return false;
            }
        }

        public bool SetMotors(float _low, float _high)
        {
            if (CanCall() == false)
            {
                return false;
            }

            try
            {
                ushort left = (ushort)(Mathf.Clamp01(_low) * ushort.MaxValue);
                ushort right = (ushort)(Mathf.Clamp01(_high) * ushort.MaxValue);
                SteamInput.TriggerVibration(boundHandle, left, right);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Steam motor write failed: " + exception.Message);
                return false;
            }
        }

        public bool SetLightBar(Color _color)
        {
            if (CanCall() == false)
            {
                return false;
            }

            try
            {
                Color32 color = _color;
                SteamInput.SetLEDColor(boundHandle, color.r, color.g, color.b, (uint)ESteamInputLEDFlag.k_ESteamInputLEDFlag_SetColor);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Steam light bar write failed: " + exception.Message);
                return false;
            }
        }

        /// <summary>No Steam API for the player-indicator dots (ISteamInput only has SetLEDColor), so never supported.</summary>
        public bool SetPlayerLeds(byte _litMask)
        {
            return false;
        }

        public bool SetTriggers(TriggerEffect _brake, TriggerEffect _throttle)
        {
            if (CanCall() == false || triggerWriter.IsAllocated == false)
            {
                return false;
            }

            try
            {
                bool isWritten = triggerWriter.Write(_brake, _throttle);
                if (isWritten == false)
                {
                    return false;
                }

                SteamInput.SetDualSenseTriggerEffect(boundHandle, triggerWriter.Buffer);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Steam trigger write failed: " + exception.Message);
                return false;
            }
        }

        /// <summary>Steam writes go out immediately, so there is nothing to batch.</summary>
        public bool Flush()
        {
            return true;
        }

        public bool Reset()
        {
            if (CanCall() == false)
            {
                return true;
            }

            try
            {
                SteamInput.TriggerVibration(boundHandle, 0, 0);
                if ((capabilities & RumbleCapabilities.LightBar) != 0)
                {
                    SteamInput.SetLEDColor(boundHandle, 0, 0, 0, (uint)ESteamInputLEDFlag.k_ESteamInputLEDFlag_RestoreUserDefault);
                }

                if (triggerWriter.IsAllocated)
                {
                    triggerWriter.Write(TriggerEffect.Off, TriggerEffect.Off);
                    SteamInput.SetDualSenseTriggerEffect(boundHandle, triggerWriter.Buffer);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Steam reset failed: " + exception.Message);
                return false;
            }
        }

        /// <summary>Forgets the handle. The last bind result stays, so the panel can still say why Steam is not used.</summary>
        public void Unbind()
        {
            boundHandle = new InputHandle_t(0);
            capabilities = RumbleCapabilities.None;
            isBoundPadBoosted = false;
            displayName = "Steam Input (none)";
        }

        public void Release()
        {
            Reset();
            Unbind();
            triggerWriter.Release();
        }

        private void SetBindResult(SteamInputState _state, string _statusLine)
        {
            lastBindState = _state;
            statusLine = _statusLine;
        }

        private bool CanCall()
        {
            if (IsBound == false || isSteamInputInitialised == false)
            {
                return false;
            }

            return ReadSteamReady();
        }

        private bool EnsureSteamInput()
        {
            if (ReadSteamReady() == false)
            {
                return false;
            }

            if (isSteamInputInitialised == false)
            {
                isSteamInputInitialised = SteamInput.Init(false);
            }

            return isSteamInputInitialised;
        }

        private bool ReadSteamReady()
        {
            if (isSteamReady == null)
            {
                return false;
            }

            return isSteamReady();
        }

        private static bool IsXboxType(ESteamInputType _inputType)
        {
            bool isXbox360 = _inputType == ESteamInputType.k_ESteamInputType_XBox360Controller;
            bool isXboxOne = _inputType == ESteamInputType.k_ESteamInputType_XBoxOneController;
            return isXbox360 || isXboxOne;
        }

        /// <summary>
        /// What Steam drives per pad type. Xbox pads get rumble through Steam too: once Steam Input owns an Xbox pad
        /// (wired or Bluetooth) the game only talks to Steam's virtual pad, so Steam is the direct route and it picks
        /// the right pad when two are connected. Generic and Switch pads return None and stay on Unity.
        /// </summary>
        private static RumbleCapabilities MapCapabilities(ESteamInputType _inputType)
        {
            switch (_inputType)
            {
                case ESteamInputType.k_ESteamInputType_PS5Controller:
                    return RumbleCapabilities.Rumble | RumbleCapabilities.LightBar | RumbleCapabilities.AdaptiveTriggers;
                case ESteamInputType.k_ESteamInputType_PS4Controller:
                    return RumbleCapabilities.Rumble | RumbleCapabilities.LightBar;
                case ESteamInputType.k_ESteamInputType_XBox360Controller:
                case ESteamInputType.k_ESteamInputType_XBoxOneController:
                case ESteamInputType.k_ESteamInputType_SteamDeckController:
                case ESteamInputType.k_ESteamInputType_SteamController:
                    return RumbleCapabilities.Rumble;
                default:
                    return RumbleCapabilities.None;
            }
        }
    }
}
