using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The spawn point hotkeys, registered with Unity's Shortcut Manager so they can be rebound (Edit &gt; Shortcuts,
    /// "Abstract Pixel/Spawn Point"). They work in the Scene view while spawn point positions are selected, and only
    /// then win over Unity's own keys. Clicks are Scene view gestures and are listed with them.
    /// </summary>
    [InitializeOnLoad]
    internal static class SpawnPointShortcuts
    {
        public const string DuplicateId = "Abstract Pixel/Spawn Point/Duplicate positions";
        public const string RemoveId = "Abstract Pixel/Spawn Point/Remove positions";
        public const string RemoveWithDeleteId = "Abstract Pixel/Spawn Point/Remove positions (second key)";
        public const string FrameId = "Abstract Pixel/Spawn Point/Frame positions";
        public const string ClearId = "Abstract Pixel/Spawn Point/Clear position selection";

        /// <summary>Active while spawn point positions are selected and the Scene view has focus.</summary>
        private sealed class PositionsSelectedContext : IShortcutContext
        {
            public bool active
            {
                get { return SpawnPointPositionSelection.HasAny && EditorWindow.focusedWindow is SceneView; }
            }
        }

        private static readonly PositionsSelectedContext context = new PositionsSelectedContext();

        static SpawnPointShortcuts()
        {
            ShortcutManager.RegisterContext(context);
        }

        /// <summary>One line of the control list: the keys, as bound now, what they do, and the mode that lights it up.</summary>
        public struct Row
        {
            public string Keys;
            public string Action;
            public SpawnPointToolMode LitIn;
            /// <summary>Keys act at once, so their row never lights; only rows for a held modifier light up.</summary>
            public bool NeverLit;
        }

        /// <summary>Every gesture and hotkey, in the order the inspector and the Overlay list them.</summary>
        public static List<Row> Rows()
        {
            List<Row> rows = new List<Row>();
            rows.Add(new Row { Keys = "Shift + click ground", Action = "Add a position where you click", LitIn = SpawnPointToolMode.Adding });
            rows.Add(new Row { Keys = "Ctrl + Shift + click", Action = "Add, only on allowed ground", LitIn = SpawnPointToolMode.AddingStrict });
            rows.Add(new Row { Keys = "Click a preview", Action = "Select it; move or turn it with W / E", LitIn = SpawnPointToolMode.AddingToSelection, NeverLit = true });
            rows.Add(new Row { Keys = "Shift + click a preview", Action = "Add it to the selection, or take it out", LitIn = SpawnPointToolMode.AddingToSelection });
            rows.Add(new Row { Keys = BindingOf(DuplicateId), Action = "Duplicate the selected positions", LitIn = SpawnPointToolMode.Editing, NeverLit = true });
            rows.Add(new Row { Keys = BindingOf(RemoveId) + " / " + BindingOf(RemoveWithDeleteId), Action = "Remove the selected positions", LitIn = SpawnPointToolMode.Editing, NeverLit = true });
            rows.Add(new Row { Keys = BindingOf(FrameId), Action = "Frame the selected positions", LitIn = SpawnPointToolMode.Editing, NeverLit = true });
            rows.Add(new Row { Keys = ClearKeys(), Action = "Clear the position selection", LitIn = SpawnPointToolMode.Editing, NeverLit = true });
            return rows;
        }

        /// <summary>The Overlay's one short line for the current mode.</summary>
        public static string ShortModeLine(SpawnPointToolMode _mode)
        {
            switch (_mode)
            {
                case SpawnPointToolMode.Adding:
                    return "Adding";
                case SpawnPointToolMode.AddingStrict:
                    return "Adding, allowed ground only";
                case SpawnPointToolMode.AddingToSelection:
                    return "Adding to selection";
                case SpawnPointToolMode.Editing:
                    return "Editing selection";
                default:
                    return "Shift + click to add";
            }
        }

        /// <summary>The line under the list that names the current mode.</summary>
        public static string ModeLine(SpawnPointToolMode _mode)
        {
            switch (_mode)
            {
                case SpawnPointToolMode.Adding:
                    return "Adding: click ground to add a position";
                case SpawnPointToolMode.AddingStrict:
                    return "Adding on allowed ground only: red ground adds nothing";
                case SpawnPointToolMode.AddingToSelection:
                    return "Selecting more: click a preview to add it";
                case SpawnPointToolMode.Editing:
                    return "Editing the selection: drag the handle, or use keys 5 to 8";
                default:
                    return "Ready: hold Shift to add, click a preview to select";
            }
        }

        private static string ClearKeys()
        {
            ShortcutBinding extra = ShortcutManager.instance.GetShortcutBinding(ClearId);
            string extraText = extra.ToString();
            if (string.IsNullOrEmpty(extraText))
            {
                return "Esc";
            }
            return "Esc or " + extraText;
        }

        /// <summary>The key bound to a hotkey now, for tooltips.</summary>
        public static string KeyFor(string _id)
        {
            return BindingOf(_id);
        }

        private static string BindingOf(string _id)
        {
            ShortcutBinding binding = ShortcutManager.instance.GetShortcutBinding(_id);
            string text = binding.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return "(unbound)";
            }
            // One format for every key: "Shift + D", like "Shift + click".
            return text.Replace("+", " + ");
        }

        // Clutch shortcuts act on Begin only: holding the key down must not duplicate or remove once per key repeat.
        [ClutchShortcut(DuplicateId, typeof(PositionsSelectedContext), KeyCode.D, ShortcutModifiers.Shift)]
        internal static void Duplicate(ShortcutArguments _arguments)
        {
            if (_arguments.stage == ShortcutStage.Begin)
            {
                SpawnPointEditing.DuplicateSelected();
            }
        }

        // Delete, not Blender's X: X is Unity's Toggle Pivot Orientation, which the position handle uses.
        [ClutchShortcut(RemoveId, typeof(PositionsSelectedContext), KeyCode.Delete)]
        internal static void Remove(ShortcutArguments _arguments)
        {
            if (_arguments.stage == ShortcutStage.Begin)
            {
                SpawnPointEditing.RemoveSelected();
            }
        }

        [ClutchShortcut(RemoveWithDeleteId, typeof(PositionsSelectedContext), KeyCode.Backspace)]
        internal static void RemoveWithDelete(ShortcutArguments _arguments)
        {
            if (_arguments.stage == ShortcutStage.Begin)
            {
                SpawnPointEditing.RemoveSelected();
            }
        }

        [Shortcut(FrameId, typeof(PositionsSelectedContext), KeyCode.F)]
        private static void Frame()
        {
            SpawnPointEditing.FrameSelected();
        }

        // Unity's Shortcut Manager refuses Esc as a binding, so Esc itself is read by the Scene view tool; this entry
        // has no default and lets a user bind a second key.
        [Shortcut(ClearId, typeof(PositionsSelectedContext))]
        private static void ClearSelection()
        {
            SpawnPointPositionSelection.Clear();
        }
    }
}
