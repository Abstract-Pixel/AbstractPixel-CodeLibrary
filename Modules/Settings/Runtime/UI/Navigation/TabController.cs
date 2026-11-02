using AbstractPixel.Core;
using System;
using System.Collections;
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
        bool isInitialized;
        private TabData pendingTabData;
        private Coroutine pendingSwitchRoutine;
        private void OnEnable()
        {
            previousTabControllerAction.action.Enable();
            nextTabControllerAction.action.Enable();

            previousTabControllerAction.action.performed += HandlePreviousTab;
            nextTabControllerAction.action.performed += HandleNextTab;

            if (allTabsList.Count == 0) return;

            foreach (TabData tab in allTabsList)
            {
                TabData tabData = tab;
                if (!isInitialized)
                {
                    tab.TabButton.SetTabButtonActionOnSelected(() => OnTabSelected(tabData));
                }

                tab.TabPanel.HidePanelImmediate(true);
                tab.TabButton.SetVisualState(false);
            }

            currentTabData = allTabsList[startTabIndex];
            currentTabData.TabPanel.ShowPanel();
            currentTabData.TabButton.SetVisualState(true);
            isInitialized = true;
        }

        private void OnDisable()
        {
            previousTabControllerAction.action.performed -= HandlePreviousTab;
            nextTabControllerAction.action.performed -= HandleNextTab;

            // A switch still waiting for its frame is dropped with the menu.
            pendingSwitchRoutine = null;
            pendingTabData = null;
        }

        /// <summary>
        /// Switches to the tab one frame later. A tab button presses itself when the D-pad selects it
        /// (AutoPressOnSelect), so this runs inside Unity's selection callback; showing the panel there
        /// makes its navigation scripts select from inside that callback, which Unity rejects
        /// ("Attempting to select ... while already selecting an object").
        /// </summary>
        public void OnTabSelected(TabData _selectedTabData)
        {
            if (_selectedTabData == currentTabData && pendingTabData == null) return;

            pendingTabData = _selectedTabData;
            if (pendingSwitchRoutine == null)
            {
                pendingSwitchRoutine = StartCoroutine(SwitchTabNextFrame());
            }
        }

        private IEnumerator SwitchTabNextFrame()
        {
            yield return null;
            pendingSwitchRoutine = null;
            TabData targetTabData = pendingTabData;
            pendingTabData = null;
            SwitchTab(targetTabData);
        }

        private void SwitchTab(TabData _selectedTabData)
        {
            if (_selectedTabData == null || _selectedTabData == currentTabData) return;

            if (currentTabData != null)
            {
                currentTabData.TabPanel.HidePanel(true);
                currentTabData.TabButton.SetVisualState(false);
            }

            // ShowPanel also selects the panel's first option for a controller (TabPanel.SetFirstElementSelected).
            currentTabData = _selectedTabData;
            currentTabData.TabPanel.ShowPanel();
            currentTabData.TabButton.SetVisualState(true);
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