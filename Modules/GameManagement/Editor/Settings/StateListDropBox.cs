using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Makes the list accept State assets dragged from the Project window, with a blue overlay while they hover.</summary>
    internal static class StateListDropBox
    {
        public static void Add(VisualElement _box, Action<List<StateSO>> _dropped)
        {
            VisualElement overlay = new VisualElement();
            overlay.pickingMode = PickingMode.Ignore;
            overlay.AddToClassList("gm-drop-overlay");
            Label text = new Label("Add to the end of the list");
            text.pickingMode = PickingMode.Ignore;
            overlay.Add(text);
            _box.Add(overlay);

            _box.RegisterCallback<DragUpdatedEvent>(_event =>
            {
                // A drag over a row's own object field replaces that row, so the box leaves it to the field.
                if (Dragged().Count == 0 || IsOverField(_event.target as VisualElement))
                {
                    overlay.RemoveFromClassList("gm-shown");
                    return;
                }
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                overlay.AddToClassList("gm-shown");
            });
            _box.RegisterCallback<DragPerformEvent>(_event =>
            {
                overlay.RemoveFromClassList("gm-shown");
                List<StateSO> states = Dragged();
                if (states.Count == 0 || IsOverField(_event.target as VisualElement))
                {
                    return;
                }
                DragAndDrop.AcceptDrag();
                _dropped(states);
            });
            _box.RegisterCallback<DragLeaveEvent>(_event =>
            {
                if (_event.target == _box)
                {
                    overlay.RemoveFromClassList("gm-shown");
                }
            });
            _box.RegisterCallback<DragExitedEvent>(_event => overlay.RemoveFromClassList("gm-shown"));
        }

        private static List<StateSO> Dragged()
        {
            return DragAndDrop.objectReferences.OfType<StateSO>().ToList();
        }

        private static bool IsOverField(VisualElement _element)
        {
            while (_element != null)
            {
                if (_element.ClassListContains("gm-state-field"))
                {
                    return true;
                }
                _element = _element.parent;
            }
            return false;
        }
    }
}
