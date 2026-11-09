using AbstractPixel.Core;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(Button), typeof(AutoPressOnSelect))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ReadOnly(true)] private Button tabButton;

        [Header("Tab Visuals")]
        [Tooltip("The Image or Text component that will change color.")]
        [SerializeField] private Graphic targetGraphic;

        [SerializeField] private Color activeTabColor = new Color(1f, 1f, 1f, 1f); // e.g., Bright White
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f); // e.g., Greyed out

        [Tooltip("Colour of an inactive tab while the mouse is over it. The active tab keeps its active colour.")]
        [SerializeField] private Color hoveredTabColor = new Color(0.72f, 0.69f, 0.55f, 1f);

        private bool isActiveTab;
        private bool isHovered;

        private void OnValidate()
        {
            if (tabButton == null) tabButton = GetComponent<Button>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
        }

        private void OnDisable()
        {
            isHovered = false;
            ApplyColor();
        }

        public void SetTabButtonActionOnSelected(Action onSelectedAction)
        {
            UnityAction actionWrapper = () => onSelectedAction.Invoke();
            tabButton.onClick.AddListener(actionWrapper);
        }

        public void SetVisualState(bool _isActiveTab)
        {
            isActiveTab = _isActiveTab;
            ApplyColor();
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            isHovered = true;
            ApplyColor();
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            isHovered = false;
            ApplyColor();
        }

        /// <summary>Active wins over hovered, so clicking a hovered tab shows the active colour at once.</summary>
        private void ApplyColor()
        {
            if (targetGraphic == null)
            {
                return;
            }

            if (isActiveTab)
            {
                targetGraphic.color = activeTabColor;
                return;
            }

            if (isHovered)
            {
                targetGraphic.color = hoveredTabColor;
                return;
            }

            targetGraphic.color = inactiveTabColor;
        }
    }
}
