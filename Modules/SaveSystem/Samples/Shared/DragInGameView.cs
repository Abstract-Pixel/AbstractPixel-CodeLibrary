using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AbstractPixel.SaveSystem.Samples
{
    /// <summary>
    /// Lets a player drag this object with the mouse in the Game view, so a sample can show that its position is saved.
    /// It needs a Collider. It is only a sample helper: saving the position is Save Transform's job, not this script's.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DragInGameView : MonoBehaviour
    {
        Collider ownCollider;
        bool isDragging;
        Vector3 grabOffset;
        Plane dragPlane;

        public bool IsDragging => isDragging;

        void Awake()
        {
            ownCollider = GetComponent<Collider>();
        }

        void Update()
        {
            Mouse mouse = Mouse.current;
            Camera viewCamera = Camera.main;
            if (mouse == null || viewCamera == null)
            {
                return;
            }

            Ray pointerRay = viewCamera.ScreenPointToRay(mouse.position.ReadValue());
            // A click on a button must not also pick up the object behind it.
            bool isOverInterface = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (mouse.leftButton.wasPressedThisFrame && !isOverInterface)
            {
                StartDragging(pointerRay, viewCamera.transform.forward);
            }
            if (!mouse.leftButton.isPressed)
            {
                StopDragging();
            }
            DragAlong(pointerRay);
        }

        /// <summary>Picks the object up if the pointer ray hits it. The object then moves across a plane facing the camera.</summary>
        public void StartDragging(Ray pointerRay, Vector3 cameraForward)
        {
            if (!ownCollider.Raycast(pointerRay, out RaycastHit hit, float.MaxValue))
            {
                return;
            }
            dragPlane = new Plane(-cameraForward, hit.point);
            grabOffset = transform.position - hit.point;
            isDragging = true;
        }

        /// <summary>Moves the picked-up object to where the pointer ray crosses its drag plane.</summary>
        public void DragAlong(Ray pointerRay)
        {
            if (isDragging && dragPlane.Raycast(pointerRay, out float distance))
            {
                transform.position = pointerRay.GetPoint(distance) + grabOffset;
            }
        }

        public void StopDragging()
        {
            isDragging = false;
        }
    }
}
