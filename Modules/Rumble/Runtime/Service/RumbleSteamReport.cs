namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The two answers a tester needs first: is Steam Input on for the pad, and is the game on its fallback (Unity
    /// while Steam was wanted)? Built from the selector and binder on demand (a struct, no allocation); the
    /// sentences allocate, so they are for the F9 panel and the bind-time Player.log lines only.
    /// </summary>
    public struct RumbleSteamReport
    {
        public SteamInputState State;
        public RumbleBackendMode Mode;
        public bool IsAutoResolvedForEditor;
        public bool IsFallbackActive;
        public bool IsPadBound;
        public int ListedPadCount;
        public string BackendReason;

        /// <summary>Steam Input ON / OFF / NOT USED / NOT AVAILABLE / FAILED, with the reason.</summary>
        public string SteamInputLine
        {
            get
            {
                switch (State)
                {
                    case SteamInputState.Serving:
                        return "ON - Steam Input drives this pad";
                    case SteamInputState.NotInstalled:
                        return "NOT AVAILABLE - no Steam backend in this build";
                    case SteamInputState.NotRunning:
                        return "NOT AVAILABLE - Steam is not running, or SteamInput.Init failed";
                    case SteamInputState.NoPadListed:
                        return "OFF for this pad - Steam runs but lists no pad to the game (Steam Input is off for it, "
                            + "or the game was not started from Steam)";
                    case SteamInputState.PadNotMatched:
                        return "ON, but Steam lists " + ListedPadCount + " pads and can't tell which one is in your hands";
                    case SteamInputState.PadTypeNotHandled:
                        return "ON for this pad, but its type is left to Unity (generic / Switch pad)";
                    case SteamInputState.FailedForThisPad:
                        return "FAILED - a Steam write failed for this pad; Unity serves it until the pad changes";
                    default:
                        return DescribeNotChecked();
                }
            }
        }

        /// <summary>Fallback ACTIVE (and why) or none (and why not).</summary>
        public string FallbackLine
        {
            get
            {
                if (IsFallbackActive)
                {
                    return "ACTIVE - Unity serves the pad because: " + ReadReasonWithoutPrefix();
                }

                if (State == SteamInputState.Serving)
                {
                    return "none (Steam serves the pad)";
                }

                if (IsPadBound == false)
                {
                    if (ListedPadCount > 0)
                    {
                        return "NOT POSSIBLE - Unity sees no controller (Steam Input holds it), so Unity has nothing to drive";
                    }

                    return "none (no pad bound)";
                }

                if (State == SteamInputState.NotChecked)
                {
                    return "none (Unity by choice: " + DescribeNotChecked() + ")";
                }

                return "none (Auto rule: Steam Input is not on for this pad, so Unity drives it)";
            }
        }

        /// <summary>One plain line for Player.log.</summary>
        public string LogLine
        {
            get
            {
                return "Steam Input: " + SteamInputLine + " | Fallback: " + FallbackLine + " | mode " + Mode;
            }
        }

        /// <summary>The selector's reason reads "Unity: why"; the fallback line already says Unity.</summary>
        private string ReadReasonWithoutPrefix()
        {
            const string UnityPrefix = "Unity: ";
            if (string.IsNullOrEmpty(BackendReason))
            {
                return "unknown";
            }

            if (BackendReason.StartsWith(UnityPrefix, System.StringComparison.Ordinal))
            {
                return BackendReason.Substring(UnityPrefix.Length);
            }

            return BackendReason;
        }

        private string DescribeNotChecked()
        {
            if (IsAutoResolvedForEditor)
            {
                return "NOT USED in the Editor (Auto = Unity here: Steam hands no pads to a process it did not launch)";
            }

            if (Mode == RumbleBackendMode.UnityInputSystemOnly)
            {
                return "NOT USED - mode Unity, Steam is never asked";
            }

            if (Mode == RumbleBackendMode.Off)
            {
                return "NOT USED - mode Off";
            }

            return "not checked (no controller)";
        }
    }
}
