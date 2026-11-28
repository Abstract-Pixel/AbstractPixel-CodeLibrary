using System;
using System.Linq;
using AbstractPixel.GameManagement.Debugging;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Gives the in-game debugger what only the Editor has: its assets by path, and the F8 shortcut.</summary>
    [InitializeOnLoad]
    internal static class DebuggerEditorHookup
    {
        private const string ShortcutId = "Abstract Pixel/Game Management/Show or Hide Debugger";
        private const string AssetsPath = "Packages/com.abstractpixel.library/Modules/GameManagement/Runtime/Debugger/Assets/DebuggerOverlayAssets.asset";

        static DebuggerEditorHookup()
        {
            DebuggerOverlay.EditorAssets = LoadAssets;
            DebuggerOverlay.EditorKey = CurrentKey;
            DebuggerOverlay.EditorKeyPressed = KeyPressedInGameView;
        }

        public static DebuggerOverlayAssets LoadAssets()
        {
            return AssetDatabase.LoadAssetAtPath<DebuggerOverlayAssets>(AssetsPath);
        }

        [Shortcut(ShortcutId, KeyCode.F8)]
        private static void ShowOrHide()
        {
            DebuggerOverlay overlay = DebuggerOverlay.Instance;
            // The overlay reads the key itself while the Game view has focus; answering here too would toggle twice.
            if (overlay != null && !IsGameViewFocused())
            {
                overlay.SetShown(!overlay.IsShown);
            }
        }

        // Unity runs no shortcut while the Game view has focus in Play mode, so the shortcut's key is read here instead.
        private static bool KeyPressedInGameView()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !IsGameViewFocused())
            {
                return false;
            }

            KeyCombination combination = ShortcutManager.instance.GetShortcutBinding(ShortcutId).keyCombinationSequence.FirstOrDefault();
            string keyName = combination.keyCode.ToString().Replace("Alpha", "Digit");
            if (combination.keyCode == KeyCode.None || !Enum.TryParse(keyName, true, out Key key) || key == Key.None)
            {
                return false;
            }
            return keyboard[key].wasPressedThisFrame
                && keyboard.shiftKey.isPressed == combination.shift
                && keyboard.altKey.isPressed == combination.alt
                && keyboard.ctrlKey.isPressed == combination.action;
        }

        private static bool IsGameViewFocused()
        {
            EditorWindow focused = EditorWindow.focusedWindow;
            if (!EditorApplication.isPlaying || focused == null)
            {
                return false;
            }
            string window = focused.GetType().Name;
            return window == "GameView" || window == "SimulatorWindow";
        }

        // Rebound or cleared in Edit > Shortcuts, so the overlay's hint asks each time it opens.
        private static string CurrentKey()
        {
            return ShortcutManager.instance.GetShortcutBinding(ShortcutId).ToString();
        }
    }
}
