using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The input maps a state can pick: label is the map name, value is its ID.</summary>
    public static class InputMapChoices
    {
        /// <summary>The asset the settings point at: the chosen one, the project-wide actions, or null for None.</summary>
        public static InputActionAsset AssetFor(GameManagementSettings _settings)
        {
            if (_settings == null)
            {
                return null;
            }
            switch (_settings.InputSource)
            {
                case InputSourceKind.ChosenAsset:
                    return _settings.InputActions;
                case InputSourceKind.ProjectWideActions:
                    return InputSystem.actions;
                default:
                    return null;
            }
        }

        public static void List(InputActionAsset _asset, List<string> _labels, List<string> _ids)
        {
            _labels.Clear();
            _ids.Clear();
            if (_asset == null)
            {
                return;
            }
            foreach (InputActionMap map in _asset.actionMaps)
            {
                _labels.Add(map.name);
                _ids.Add(map.id.ToString());
            }
        }
    }
}
