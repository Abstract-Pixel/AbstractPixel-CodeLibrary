using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ADD THIS

namespace AbstractPixel.Settings
{
    public class TabController : MonoBehaviour
    {
        [SerializeField] private List<TabData> allTabsList = new List<TabData>();
        [SerializeField] private int startTabIndex = 0;

        [SerializeField, ReadOnly] private TabData currentTabData;

        [Header("Controller Navigation (Decoupled)")]
        [Tooltip("Configure the binding for the Previous Tab (e.g., <Gamepad>/leftShoulder)")]
        [SerializeField] private InputActionReference previousTabControllerAction;
        [SerializeField] private InputActionReference nextTabControllerAction;

        private void OnEnable()
        {
            previousTabControllerAction.action.Enable();
            nextTabControllerAction.action.Enable();

            previousTabControllerAction.action.performed += HandlePreviousTab;
            nextTabControllerAction.action.performed += HandleNextTab;
        }

        private void OnDisable()
        {
            previousTabControllerAction.action.performed -= HandlePreviousTab;
            nextTabControllerAction.action.performed -= HandleNextTab;
        }

        private void Start()
        {
            if (allTabsList.Count == 0) return;

            foreach (TabData tab in allTabsList)
            {
                TabData tabData = tab;
                tab.TabButton.SetTabButtonActionOnSelected(() => OnTabSelected(tabData));

                tab.TabPanel.GetComponent<CanvasGroup>().alpha = 0;
                tab.TabPanel.HidePanel();
                tab.TabButton.SetVisualState(false);
            }

            currentTabData = allTabsList[startTabIndex];
            currentTabData.TabPanel.ShowPanel();
            currentTabData.TabButton.SetVisualState(true);
        }

        public void OnTabSelected(TabData _selectedTabData)
        {
            if (_selectedTabData == currentTabData) return;

            if (currentTabData != null)
            {
                currentTabData.TabPanel.HidePanel();
                currentTabData.TabButton.SetVisualState(false);
            }

            currentTabData = _selectedTabData;
            currentTabData.TabPanel.ShowPanel();
            currentTabData.TabButton.SetVisualState(true);

            // Snap the controller focus to the first setting in the new tab!
            currentTabData.TabPanel.SetFirstElementSelected();
        }

        // =========================================================
        // INPUT ACTION CALLBACKS
        // =========================================================

        private void HandlePreviousTab(InputAction.CallbackContext context)
        {
            if (allTabsList.Count <= 1) return;

            int currentIndex = allTabsList.IndexOf(currentTabData);
            int newIndex = currentIndex - 1;

            // Loop back to the end if we go past the first tab
            if (newIndex < 0)
            {
                newIndex = allTabsList.Count - 1;
            }

            OnTabSelected(allTabsList[newIndex]);
        }

        private void HandleNextTab(InputAction.CallbackContext context)
        {
            if (allTabsList.Count <= 1) return;

            int currentIndex = allTabsList.IndexOf(currentTabData);
            int newIndex = currentIndex + 1;

            // Loop back to the beginning if we go past the last tab
            if (newIndex >= allTabsList.Count)
            {
                newIndex = 0;
            }

            OnTabSelected(allTabsList[newIndex]);
        }

        [Serializable]
        public class TabData
        {
            [SerializeField, ReadOnly(true)] private string name;
            [SerializeField] public TabButton TabButton;
            [SerializeField] public TabPanel TabPanel;
        }
    }
}