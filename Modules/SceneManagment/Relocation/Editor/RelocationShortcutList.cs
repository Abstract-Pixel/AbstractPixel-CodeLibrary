using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The spawn point controls, for the inspector and the Overlay: a numbered list read from the Shortcut Manager, the
    /// row for what a click would do now lit up, the current mode under it, and the one rule worth remembering (Ctrl is
    /// always stricter) set apart. It follows the Scene System page's look.
    /// </summary>
    internal sealed class RelocationShortcutList : VisualElement
    {
        private readonly List<VisualElement> rowElements = new List<VisualElement>();
        private readonly List<SpawnPointShortcuts.Row> rows = new List<SpawnPointShortcuts.Row>();
        private readonly Label modeLine;
        private SpawnPointToolMode shownMode = (SpawnPointToolMode)(-1);

        private static Color MutedText
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.33f, 0.33f, 0.33f); }
        }

        private static Color LitBackground
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.30f, 0.75f, 0.36f, 0.16f) : new Color(0.20f, 0.50f, 0.26f, 0.14f); }
        }

        /// <param name="_keysWidth">Width of the keys column: wide in the inspector, narrow in the Overlay.</param>
        public RelocationShortcutList(float _keysWidth)
        {
            style.marginTop = 4;
            Label title = new Label("Controls in the Scene view");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 3;
            Add(title);

            int number = 1;
            foreach (SpawnPointShortcuts.Row row in SpawnPointShortcuts.Rows())
            {
                VisualElement line = Line(number, row, _keysWidth);
                rowElements.Add(line);
                rows.Add(row);
                Add(line);
                number++;
            }

            Add(StrictNote());

            VisualElement separator = SceneSystemUI.Separator();
            separator.style.width = Length.Percent(100);
            separator.style.height = 1;
            separator.style.marginTop = 6;
            separator.style.marginBottom = 4;
            Add(separator);
            modeLine = new Label();
            modeLine.enableRichText = true;
            modeLine.style.color = SceneSystemUI.DefaultAccent;
            modeLine.style.unityFontStyleAndWeight = FontStyle.Bold;
            modeLine.style.whiteSpace = WhiteSpace.Normal;
            Add(modeLine);

            schedule.Execute(ShowCurrentMode).Every(100);
            ShowCurrentMode();
        }

        // Lights the rows for the current mode and names it under the list.
        private void ShowCurrentMode()
        {
            SpawnPointToolMode mode = SpawnPointToolState.Mode;
            if (mode == shownMode)
            {
                return;
            }
            shownMode = mode;
            for (int index = 0; index < rowElements.Count; index++)
            {
                bool lit = !rows[index].NeverLit && rows[index].LitIn == mode;
                rowElements[index].style.backgroundColor = lit ? LitBackground : new Color(0f, 0f, 0f, 0f);
                rowElements[index].style.borderLeftColor = lit ? SceneSystemUI.DefaultAccent : new Color(0f, 0f, 0f, 0f);
                foreach (Label label in rowElements[index].Query<Label>().ToList())
                {
                    label.style.color = lit ? StyleKeyword.Null : (StyleColor)MutedText;
                }
            }
            modeLine.text = "Now: " + SpawnPointShortcuts.ModeLine(mode);
        }

        private static VisualElement Line(int _number, SpawnPointShortcuts.Row _row, float _keysWidth)
        {
            VisualElement line = SceneSystemUI.Row();
            line.style.paddingTop = 1;
            line.style.paddingBottom = 1;
            line.style.paddingLeft = 4;
            line.style.borderLeftWidth = 3;
            line.style.borderTopLeftRadius = 2;
            line.style.borderBottomLeftRadius = 2;

            Label number = new Label(_number + ".");
            number.style.width = 18;
            number.style.flexShrink = 0;
            line.Add(number);

            Label keys = new Label(_row.Keys);
            keys.style.width = _keysWidth;
            keys.style.flexShrink = 0;
            keys.style.unityFontStyleAndWeight = FontStyle.Bold;
            line.Add(keys);

            Label action = new Label(_row.Action);
            action.style.flexShrink = 1;
            action.style.whiteSpace = WhiteSpace.Normal;
            line.Add(action);
            return line;
        }

        // The one rule that holds for every control, set apart so it is not missed.
        private static VisualElement StrictNote()
        {
            Label note = new Label("<b>Ctrl is always stricter:</b> it snaps while you drag, and adds only on allowed ground.");
            note.enableRichText = true;
            note.style.whiteSpace = WhiteSpace.Normal;
            note.style.marginTop = 6;
            note.style.paddingLeft = 6;
            note.style.paddingTop = 3;
            note.style.paddingBottom = 3;
            note.style.borderLeftWidth = 3;
            note.style.borderLeftColor = SceneSystemUI.Yellow;
            note.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.96f, 0.74f, 0.19f, 0.10f) : new Color(0.96f, 0.74f, 0.19f, 0.18f);
            return note;
        }
    }
}
