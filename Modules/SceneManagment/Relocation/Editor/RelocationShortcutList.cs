using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The compact, muted list of spawn point keys, read from the Shortcut Manager, for the inspector and the Overlay.</summary>
    internal static class RelocationShortcutList
    {
        /// <param name="_keysWidth">Width of the keys column: wide in the inspector, narrow in the Overlay.</param>
        public static VisualElement Create(float _keysWidth)
        {
            VisualElement list = new VisualElement();
            list.style.marginTop = 6;
            foreach (SpawnPointShortcuts.Row row in SpawnPointShortcuts.Rows())
            {
                list.Add(Line(row.Keys, row.Action, _keysWidth));
            }
            Label ctrl = SceneSystemUI.Hint(SpawnPointShortcuts.CtrlLine);
            ctrl.style.marginTop = 3;
            list.Add(ctrl);
            return list;
        }

        private static VisualElement Line(string _keys, string _action, float _keysWidth)
        {
            VisualElement line = SceneSystemUI.Row();
            Label keys = SceneSystemUI.Hint(_keys);
            keys.style.width = _keysWidth;
            keys.style.flexShrink = 0;
            keys.style.unityFontStyleAndWeight = FontStyle.Bold;
            line.Add(keys);
            line.Add(SceneSystemUI.Hint(_action));
            return line;
        }
    }
}
