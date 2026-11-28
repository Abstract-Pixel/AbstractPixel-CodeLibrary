using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Drag a row by its handle onto a sibling (another main row, or a sub-state in the same group) to take its place.</summary>
    internal class StateRowDrag
    {
        private readonly Action<int, int> moved;
        private readonly Dictionary<StateRowView, int> rows = new Dictionary<StateRowView, int>();
        private StateRowView dragged;
        private StateRowView target;

        public StateRowDrag(Action<int, int> _moved)
        {
            moved = _moved;
        }

        public void Register(StateRowView _row, int _catalogIndex)
        {
            rows[_row] = _catalogIndex;
            VisualElement handle = _row.Handle;
            handle.RegisterCallback<PointerDownEvent>(_event =>
            {
                if (_event.button != 0)
                {
                    return;
                }
                dragged = _row;
                handle.CapturePointer(_event.pointerId);
                _row.AddToClassList("gm-dragged");
                _event.StopPropagation();
            });
            handle.RegisterCallback<PointerMoveEvent>(_event =>
            {
                if (dragged == _row)
                {
                    SetTarget(FindTarget(_event.position));
                }
            });
            handle.RegisterCallback<PointerUpEvent>(_event =>
            {
                if (dragged != _row)
                {
                    return;
                }
                StateRowView dropOn = target;
                handle.ReleasePointer(_event.pointerId);
                Finish();
                if (dropOn != null)
                {
                    moved(_catalogIndex, rows[dropOn]);
                }
            });
            handle.RegisterCallback<PointerCaptureOutEvent>(_event => Finish());
        }

        private StateRowView FindTarget(Vector2 _position)
        {
            foreach (StateRowView row in rows.Keys)
            {
                // Siblings share a parent chain: sub-states share their group, main rows share the list.
                bool sibling = row != dragged && row.parent != null
                    && (row.parent == dragged.parent || (row.parent.ClassListContains("gm-line") && dragged.parent.ClassListContains("gm-line")));
                if (sibling && row.worldBound.Contains(_position))
                {
                    return row;
                }
            }
            return null;
        }

        private void SetTarget(StateRowView _target)
        {
            target?.RemoveFromClassList("gm-drop-target");
            target = _target;
            target?.AddToClassList("gm-drop-target");
        }

        private void Finish()
        {
            dragged?.RemoveFromClassList("gm-dragged");
            dragged = null;
            SetTarget(null);
        }
    }
}
