using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Fades the listed graphics to a tint while the button is hovered with the mouse or selected with a controller.
    /// The tint is multiplied on the CanvasRenderer, never written to Graphic.color, so it stays out of the way of
    /// any other script that animates or themes those colours. Works while timeScale is 0.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class ButtonHoverTint : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Tooltip("Do not list the Selectable's own Target Graphic if its transition is Color Tint: both would drive the same renderer.")]
        [SerializeField] private Graphic[] tintedGraphics;
        [SerializeField] private Color hoverTint = new Color(0.2f, 0.55f, 0.68f, 1f);
        [Min(0f)][SerializeField] private float fadeSeconds = 0.1f;

        private Selectable selectable;
        private bool isHovered;
        private bool isSelected;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            isHovered = false;
            isSelected = false;
            ApplyTint(Color.white, 0f);
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            isHovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            isHovered = false;
            Refresh();
        }

        public void OnSelect(BaseEventData _eventData)
        {
            isSelected = true;
            Refresh();
        }

        public void OnDeselect(BaseEventData _eventData)
        {
            isSelected = false;
            Refresh();
        }

        private void Refresh()
        {
            bool isHighlighted = (isHovered || isSelected) && selectable.IsInteractable();

            if (isHighlighted)
            {
                ApplyTint(hoverTint, fadeSeconds);
                return;
            }

            ApplyTint(Color.white, fadeSeconds);
        }

        private void ApplyTint(Color _tint, float _seconds)
        {
            for (int graphicIndex = 0; graphicIndex < tintedGraphics.Length; graphicIndex++)
            {
                Graphic graphic = tintedGraphics[graphicIndex];
                if (graphic != null)
                {
                    graphic.CrossFadeColor(_tint, _seconds, true, false);
                }
            }
        }
    }
}
