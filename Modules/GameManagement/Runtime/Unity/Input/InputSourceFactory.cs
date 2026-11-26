using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Builds the input source the settings ask for. A custom source is given in code to <see cref="GameStateDriver.UseInputSource"/>.</summary>
    public static class InputSourceFactory
    {
        public static IInputSource FromSettings(GameManagementSettings _settings)
        {
            if (_settings == null)
            {
                return null;
            }
            switch (_settings.InputSource)
            {
                case InputSourceKind.ProjectWideActions:
                    return InputActionAssetSource.ProjectWide();
                case InputSourceKind.ChosenAsset:
                    if (_settings.InputActions == null)
                    {
                        Debug.LogWarning($"[Game management] '{_settings.name}' uses a chosen input asset, but none is assigned. Assign one in its Input Actions field, or set the input source to None.");
                        return null;
                    }
                    return new InputActionAssetSource(_settings.InputActions);
                default:
                    return null;
            }
        }
    }
}
