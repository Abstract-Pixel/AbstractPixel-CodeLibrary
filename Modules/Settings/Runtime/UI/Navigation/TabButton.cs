using AbstractPixel.Core;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(Button), typeof(AutoPressOnSelect))]
    public class TabButton : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)] private Button tabButton;

        [Header("Tab Visuals")]
        [Tooltip("The Image or Text component that will change color.")]
        [SerializeField] private Graphic targetGraphic;

        [SerializeField] private Color activeTabColor = new Color(1f, 1f, 1f, 1f); // e.g., Bright White
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f, 1f); // e.g., Greyed out

        private void OnValidate()
        {
            if (tabButton == null) tabButton = GetComponent<Button>();
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
        }

        public void SetTabButtonActionOnSelected(Action onSelectedAction)
        {
            UnityAction actionWrapper = () => onSelectedAction.Invoke();
            tabButton.onClick.AddListener(actionWrapper);
        }

        public void SetVisualState(bool isActiveTab)
        {
            if (targetGraphic != null)
            {
                targetGraphic.color = isActiveTab ? activeTabColor : inactiveTabColor;
            }
        }
    }
}