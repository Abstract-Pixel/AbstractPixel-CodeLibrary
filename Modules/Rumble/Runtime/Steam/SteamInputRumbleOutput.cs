using System;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Main backend through Steamworks ISteamInput. Binds the Steam controller in the player's hands: the only one
    /// connected, or the only one matching the active pad's type. Every native call re-checks Steam readiness,
    /// uses only handles from this session, and never sends to handle 0.
    /// Also reports its status (IRumbleBackendStatus): whether Steam is up, whether the player has Steam Input on for
    /// this pad's type, and whether the bound handle is still connected (it changes on reconnect, e.g. USB to Bluetooth).
    /// Player LEDs: ISteamInput has no call for them (only SetLEDColor for the light bar), so they are not offered here.
    /// </summary>
    public class SteamInputRumbleOutput : IRumbleOutput, IRumbleBackendStatus
    {
        private readonly Func<bool> isSteamReady;
        private readonly InputHandle_t[] connectedHandles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
        private readonly DualSenseTriggerParamWriter triggerWriter = new DualSenseTriggerParamWriter();

        private bool isSteamInputInitialised;
        private bool hasLoggedBindFailure;
        private InputHandle_t boundHandle;
        private string displayName = "Steam Input (none)";
        private string statusLine = "not tried";
        private RumbleCapabilities capabilities = RumbleCapabilities.None;

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

        /// <summary>
        /// Steam's own answer for this session: which controller families have Steam Input turned on (the player's
        /// per-game setting overrides the game's default). Steam Deck and Steam Controller always use Steam Input.
        /// </summary>
        public bool IsEnabledByPlayerForBoundPad
        {
            get
            {
                if (CanCall() == false)
                {
                    return false;
                }

                try
                {
                    ESteamInputType inputType = SteamInput.GetInputTypeForHandle(boundHandle);
                    int requiredBit = ReadConfigurationBit(inputType);
                    if (requiredBit == 0)
                    {
                        return true;
                    }

                    int enabledBits = SteamInput.GetSessionInputConfigurationSettings();
                    return (enabledBits & requiredBit) != 0;
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
                    int controllerCount = SteamInput.GetConnectedControllers(connectedHandles);
                    int count = Mathf.Min(controllerCount, connectedHandles.Length);
                    for (int index = 0; index < count; index++)
                    {
                        if (connectedHandles[index].m_InputHandle == boundHandle.m_InputHandle)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public string StatusLine => statusLine;

        public bool TryBind(InputDevice _activeDevice)
        {
            Unbind();
            try
            {
                if (EnsureSteamInput() == false)
                {
                    statusLine = "Steam not running or SteamInput.Init failed";
                    return false;
                }

                SteamInput.RunFrame();
                int controllerCount = SteamInput.GetConnectedControllers(connectedHandles);
                int handleIndex = FindHandleFor(_activeDevice, controllerCount);
                if (handleIndex < 0)
                {
                    statusLine = "Steam lists " + controllerCount + " pad(s); none matches the active pad";
                    return false;
                }

                ESteamInputType inputType = SteamInput.GetInputTypeForHandle(connectedHandles[handleIndex]);
                RumbleCapabilities typeCapabilities = MapCapabilities(inputType);
                if (typeCapabilities == RumbleCapabilities.None)
                {
                    statusLine = inputType + " is left to Unity";
                    return false;
                }

                boundHandle = connectedHandles[handleIndex];
                capabilities = typeCapabilities;
                displayName = "Steam Input (" + inputType + ")";
                if ((capabilities & RumbleCapabilities.AdaptiveTriggers) != 0)
                {
                    triggerWriter.Allocate();
                }

                statusLine = "bound " + inputType;
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

        public void Unbind()
        {
            boundHandle = new InputHandle_t(0);
            statusLine = "not bound";
            capabilities = RumbleCapabilities.None;
            displayName = "Steam Input (none)";
        }

        public void Release()
        {
            Reset();
            Unbind();
            triggerWriter.Release();
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

        /// <summary>
        /// Which Steam handle is the pad in the player's hands. One controller: that one. Several (e.g. a DualSense and
        /// an Xbox pad): the only one whose type matches the active Unity device. Ambiguous: -1, and Unity serves the pad.
        /// </summary>
        private int FindHandleFor(InputDevice _activeDevice, int _controllerCount)
        {
            int count = Mathf.Min(_controllerCount, connectedHandles.Length);
            if (count <= 0)
            {
                return -1;
            }

            if (count == 1)
            {
                return ReadValidIndex(0);
            }

            ESteamInputType wantedType = ReadSteamTypeOf(_activeDevice);
            if (wantedType == ESteamInputType.k_ESteamInputType_Unknown)
            {
                return -1;
            }

            int matchIndex = -1;
            for (int index = 0; index < count; index++)
            {
                ESteamInputType handleType = SteamInput.GetInputTypeForHandle(connectedHandles[index]);
                if (handleType != wantedType)
                {
                    continue;
                }

                // Two pads of the same type: we can't tell which one is in the player's hands.
                if (matchIndex >= 0)
                {
                    return -1;
                }

                matchIndex = index;
            }

            if (matchIndex < 0)
            {
                return -1;
            }

            return ReadValidIndex(matchIndex);
        }

        /// <summary>Handle 0 means "no controller" to Steam; never bind it.</summary>
        private int ReadValidIndex(int _index)
        {
            if (connectedHandles[_index].m_InputHandle == 0)
            {
                return -1;
            }

            return _index;
        }

        /// <summary>The Steam type matching a Unity PlayStation device; Unknown for anything else.</summary>
        private static ESteamInputType ReadSteamTypeOf(InputDevice _device)
        {
            if (_device is UnityEngine.InputSystem.DualShock.DualSenseGamepadHID)
            {
                return ESteamInputType.k_ESteamInputType_PS5Controller;
            }

            if (_device is UnityEngine.InputSystem.DualShock.DualShockGamepad)
            {
                return ESteamInputType.k_ESteamInputType_PS4Controller;
            }

            return ESteamInputType.k_ESteamInputType_Unknown;
        }

        /// <summary>The ESteamInputConfigurationEnableType bit that must be on for this pad type. 0 = always Steam (Deck, Steam Controller).</summary>
        private static int ReadConfigurationBit(ESteamInputType _inputType)
        {
            switch (_inputType)
            {
                case ESteamInputType.k_ESteamInputType_PS3Controller:
                case ESteamInputType.k_ESteamInputType_PS4Controller:
                case ESteamInputType.k_ESteamInputType_PS5Controller:
                    return (int)ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Playstation;
                case ESteamInputType.k_ESteamInputType_XBox360Controller:
                case ESteamInputType.k_ESteamInputType_XBoxOneController:
                    return (int)ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Xbox;
                case ESteamInputType.k_ESteamInputType_SwitchProController:
                case ESteamInputType.k_ESteamInputType_SwitchJoyConPair:
                case ESteamInputType.k_ESteamInputType_SwitchJoyConSingle:
                    return (int)ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Switch;
                case ESteamInputType.k_ESteamInputType_SteamDeckController:
                case ESteamInputType.k_ESteamInputType_SteamController:
                    return 0;
                default:
                    return (int)ESteamInputConfigurationEnableType.k_ESteamInputConfigurationEnableType_Generic;
            }
        }

        /// <summary>Xbox and unknown types return None so Unity's proven XInput path handles them.</summary>
        private static RumbleCapabilities MapCapabilities(ESteamInputType _inputType)
        {
            switch (_inputType)
            {
                case ESteamInputType.k_ESteamInputType_PS5Controller:
                    return RumbleCapabilities.Rumble | RumbleCapabilities.LightBar | RumbleCapabilities.AdaptiveTriggers;
                case ESteamInputType.k_ESteamInputType_PS4Controller:
                    return RumbleCapabilities.Rumble | RumbleCapabilities.LightBar;
                case ESteamInputType.k_ESteamInputType_SteamDeckController:
                case ESteamInputType.k_ESteamInputType_SteamController:
                    return RumbleCapabilities.Rumble;
                default:
                    return RumbleCapabilities.None;
            }
        }
    }
}
