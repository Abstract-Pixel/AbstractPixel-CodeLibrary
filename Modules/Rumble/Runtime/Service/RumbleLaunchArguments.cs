using System;

namespace AbstractPixel.Rumble
{
    /// <summary>Reads "-rumble-backend=auto|steam|unity|off" once, so players can override the backend from Steam launch options.</summary>
    public static class RumbleLaunchArguments
    {
        private const string BackendArgumentPrefix = "-rumble-backend=";

        public static bool TryReadBackendOverride(out RumbleBackendMode _mode)
        {
            _mode = RumbleBackendMode.Auto;

            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string argument in arguments)
            {
                if (argument.StartsWith(BackendArgumentPrefix, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                string value = argument.Substring(BackendArgumentPrefix.Length).ToLowerInvariant();
                return TryParseValue(value, out _mode);
            }

            return false;
        }

        private static bool TryParseValue(string _value, out RumbleBackendMode _mode)
        {
            switch (_value)
            {
                case "auto":
                    _mode = RumbleBackendMode.Auto;
                    return true;
                case "steam":
                    _mode = RumbleBackendMode.SteamInputOnly;
                    return true;
                case "unity":
                    _mode = RumbleBackendMode.UnityInputSystemOnly;
                    return true;
                case "off":
                    _mode = RumbleBackendMode.Off;
                    return true;
                default:
                    _mode = RumbleBackendMode.Auto;
                    return false;
            }
        }
    }
}
