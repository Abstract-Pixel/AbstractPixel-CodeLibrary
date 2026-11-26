using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>Switches maps in one <see cref="InputActionAsset"/>: a chosen asset, or the project-wide actions.</summary>
    public class InputActionAssetSource : IInputSource
    {
        private readonly InputActionAsset asset;

        public InputActionAssetSource(InputActionAsset _asset)
        {
            asset = _asset;
        }

        public InputActionAsset Asset
        {
            get { return asset; }
        }

        /// <summary>The project-wide actions (Project Settings &gt; Input System Package), or null when none are set.</summary>
        public static InputActionAssetSource ProjectWide()
        {
            if (InputSystem.actions == null)
            {
                Debug.LogWarning("[Game management] The input source is 'Project Wide Actions', but none are set. Assign them in Project Settings > Input System Package, or pick another input source.");
                return null;
            }
            return new InputActionAssetSource(InputSystem.actions);
        }

        public void EnableOnly(IReadOnlyList<InputActionMap> _maps)
        {
            foreach (InputActionMap map in asset.actionMaps)
            {
                if (Contains(_maps, map))
                {
                    map.Enable();
                }
                else
                {
                    map.Disable();
                }
            }
        }

        private static bool Contains(IReadOnlyList<InputActionMap> _maps, InputActionMap _map)
        {
            for (int i = 0; i < _maps.Count; i++)
            {
                if (_maps[i] == _map)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
