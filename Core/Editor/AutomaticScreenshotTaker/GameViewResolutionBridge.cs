using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ScreenshotTool.Editor
{
    public static class GameViewResolutionBridge
    {
        private const string GAME_VIEW_TYPE_NAME = "UnityEditor.GameView";
        private const string PRESET_NAME_PREFIX = "ScreenshotDeck_Native";

        public static bool TrySetGameViewResolution(int _width, int _height, out int _previousSizeIndex)
        {
            _previousSizeIndex = -1;

            Type gameViewType = typeof(UnityEditor.Editor).Assembly.GetType(GAME_VIEW_TYPE_NAME);
            if (gameViewType == null)
            {
                return false;
            }

            EditorWindow gameViewWindow = EditorWindow.GetWindow(gameViewType, false, null, false);
            if (gameViewWindow == null)
            {
                return false;
            }

            PropertyInfo selectedSizeIndexProperty = gameViewType.GetProperty(
                "selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (selectedSizeIndexProperty != null)
            {
                object rawIndexValue = selectedSizeIndexProperty.GetValue(gameViewWindow, null);
                if (rawIndexValue is int currentIndex)
                {
                    _previousSizeIndex = currentIndex;
                }
            }

            MethodInfo setCustomResolutionMethod = gameViewType.GetMethod(
                "SetCustomResolution",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (setCustomResolutionMethod != null)
            {
                setCustomResolutionMethod.Invoke(
                    gameViewWindow,
                    new object[] { new Vector2(_width, _height), PRESET_NAME_PREFIX }
                );

                gameViewWindow.Repaint();
                return true;
            }

            return false;
        }

        public static void RestoreGameViewResolution(int _previousSizeIndex)
        {
            if (_previousSizeIndex < 0)
            {
                return;
            }

            Type gameViewType = typeof(UnityEditor.Editor).Assembly.GetType(GAME_VIEW_TYPE_NAME);
            if (gameViewType == null)
            {
                return;
            }

            EditorWindow gameViewWindow = EditorWindow.GetWindow(gameViewType, false, null, false);
            if (gameViewWindow == null)
            {
                return;
            }

            PropertyInfo selectedSizeIndexProperty = gameViewType.GetProperty(
                "selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (selectedSizeIndexProperty != null)
            {
                selectedSizeIndexProperty.SetValue(gameViewWindow, _previousSizeIndex, null);
                gameViewWindow.Repaint();
            }
        }
    }
}