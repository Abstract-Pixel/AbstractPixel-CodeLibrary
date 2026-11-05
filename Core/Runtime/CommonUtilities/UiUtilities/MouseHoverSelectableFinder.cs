using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    // Finds the button under the mouse when the controller takes over. Allocates, so call it on input changes, not every frame.
    public static class MouseHoverSelectableFinder
    {
        // Only the top UI hit counts, so a button covered by another panel is not picked.
        public static Selectable FindUsableSelectable()
        {
            EventSystem currentEventSystem = EventSystem.current;
            Mouse currentMouse = Mouse.current;
            if (currentEventSystem == null || currentMouse == null)
            {
                return null;
            }

            PointerEventData pointerData = new PointerEventData(currentEventSystem);
            pointerData.position = currentMouse.position.ReadValue();

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            currentEventSystem.RaycastAll(pointerData, raycastResults);
            if (raycastResults.Count == 0)
            {
                return null;
            }

            Selectable hitSelectable = raycastResults[0].gameObject.GetComponentInParent<Selectable>();
            if (hitSelectable == null || !hitSelectable.IsInteractable())
            {
                return null;
            }

            return hitSelectable;
        }
    }
}
