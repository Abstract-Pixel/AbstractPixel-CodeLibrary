using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    /// <summary>
    /// When this element is selected with a controller or keyboard, scrolls the ScrollRect above it just enough to show
    /// it. Put it on a TMP_Dropdown's template Item: every option is cloned from the item, so the open list follows the
    /// selection and its scrollbar moves with it. Mouse selections are ignored; the mouse scrolls the list itself.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScrollIntoViewOnSelect : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData _eventData)
        {
            bool selectedByPointer = _eventData is PointerEventData;
            if (selectedByPointer)
            {
                return;
            }

            ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect == null || scrollRect.content == null)
            {
                return;
            }

            ScrollToShow(scrollRect);
        }

        /// <summary>Moves the content vertically by the amount this item sticks out of the viewport, if any.</summary>
        private void ScrollToShow(ScrollRect _scrollRect)
        {
            // The dropdown builds its list this frame; lay it out first so the item rects are final.
            Canvas.ForceUpdateCanvases();

            RectTransform viewport = _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;
            Bounds itemInViewport = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, transform);
            Rect viewportRect = viewport.rect;

            float overflowAbove = itemInViewport.max.y - viewportRect.yMax;
            float overflowBelow = viewportRect.yMin - itemInViewport.min.y;

            float contentShift = 0f;
            if (overflowAbove > 0f)
            {
                contentShift = -overflowAbove;
            }
            else if (overflowBelow > 0f)
            {
                contentShift = overflowBelow;
            }

            if (Mathf.Approximately(contentShift, 0f))
            {
                return;
            }

            _scrollRect.StopMovement();
            RectTransform content = _scrollRect.content;
            content.anchoredPosition += new Vector2(0f, contentShift);
        }
    }
}
