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

        /// <summary>One line of the shortcut list: the keys, as bound now, and what they do.</summary>
        public struct Row
        {
            public string Keys;
            public string Action;
        }

        /// <summary>Every gesture and hotkey, in the order the inspector and the Overlay list them.</summary>
        public static List<Row> Rows()
        {
            List<Row> rows = new List<Row>();
            rows.Add(new Row { Keys = "Shift+click", Action = "Add" });
            rows.Add(new Row { Keys = "Ctrl+Shift+click", Action = "Add on allowed ground only" });
            rows.Add(new Row { Keys = "Click", Action = "Select" });
            rows.Add(new Row { Keys = "Shift+click a preview", Action = "Add to selection" });
            rows.Add(new Row { Keys = BindingOf(DuplicateId), Action = "Duplicate" });
            rows.Add(new Row { Keys = BindingOf(RemoveId) + " or " + BindingOf(RemoveWithDeleteId), Action = "Remove" });
            rows.Add(new Row { Keys = BindingOf(FrameId), Action = "Frame" });
            rows.Add(new Row { Keys = ClearKeys(), Action = "Clear selection" });
            return rows;
        }

        public const string CtrlLine = "Ctrl is always stricter: it snaps while dragging and adds only on allowed ground.";

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

        private static string BindingOf(string _id)
        {
            ShortcutBinding binding = ShortcutManager.instance.GetShortcutBinding(_id);
            string text = binding.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return "(unbound)";
            }
            return text;
        }

        [Shortcut(DuplicateId, typeof(PositionsSelectedContext), KeyCode.D, ShortcutModifiers.Shift)]
        private static void Duplicate()
        {
            SpawnPointEditing.DuplicateSelected();
        }

        // Delete, not Blender's X: X is Unity's Toggle Pivot Orientation, which the position handle uses.
        [Shortcut(RemoveId, typeof(PositionsSelectedContext), KeyCode.Delete)]
        private static void Remove()
        {
            SpawnPointEditing.RemoveSelected();
        }

        [Shortcut(RemoveWithDeleteId, typeof(PositionsSelectedContext), KeyCode.Backspace)]
        private static void RemoveWithDelete()
        {
            SpawnPointEditing.RemoveSelected();
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
