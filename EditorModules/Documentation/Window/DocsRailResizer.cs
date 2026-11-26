using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>A drag handle on the left edge of the "On this page" rail that sets its width.</summary>
    /// <remarks>
    /// - The width is kept in EditorPrefs, so it survives reloads and new windows.
    /// - The width stays between MinWidth and MaxWidth, so the page column never disappears.
    /// </remarks>
    public class DocsRailResizer
    {
        public const float MinWidth = 180f;
        public const float MaxWidth = 520f;
        public const float DefaultWidth = 280f;
        private const string WidthKey = "AbstractPixel.SceneDocs.RailWidth";

        private readonly VisualElement rail;
        private readonly VisualElement handle = new VisualElement();
        private bool isDragging;
        private float dragStartMouseX;
        private float dragStartWidth;

        public DocsRailResizer(VisualElement _rail)
        {
            rail = _rail;
            handle.AddToClassList("docs-rail-handle");
            handle.tooltip = "Drag to resize";
            rail.Add(handle);
            ApplyWidth(EditorPrefs.GetFloat(WidthKey, DefaultWidth));

            handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent _event)
        {
            if (_event.button != 0)
            {
                return;
            }
            isDragging = true;
            dragStartMouseX = _event.position.x;
            dragStartWidth = rail.resolvedStyle.width;
            handle.CapturePointer(_event.pointerId);
            _event.StopPropagation();
        }

        // The rail sits on the right, so dragging left makes it wider.
        private void OnPointerMove(PointerMoveEvent _event)
        {
            if (!isDragging)
            {
                return;
            }
            float movedBy = _event.position.x - dragStartMouseX;
            float newWidth = dragStartWidth - movedBy;
            ApplyWidth(newWidth);
        }

        private void OnPointerUp(PointerUpEvent _event)
        {
            if (!isDragging)
            {
                return;
            }
            isDragging = false;
            handle.ReleasePointer(_event.pointerId);
            EditorPrefs.SetFloat(WidthKey, rail.resolvedStyle.width);
        }

        private void ApplyWidth(float _width)
        {
            float clampedWidth = Mathf.Clamp(_width, MinWidth, MaxWidth);
            rail.style.width = clampedWidth;
        }
    }
}
