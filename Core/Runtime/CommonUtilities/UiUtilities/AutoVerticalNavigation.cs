using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    public class AutoVerticalNavigation : MonoBehaviour
    {
        [Tooltip("If true, pressing UP on the first setting loops to the bottom. If false, it stays at the top.")]
        [SerializeField] private bool loopNavigation = false;

        [Tooltip("How often (in seconds) the script checks for disabled/enabled UI elements. 0.1 is recommended for instant feel.")]
        [SerializeField] private float evaluationRate = 0.5f;

        // State Tracking Variables
        private Selectable[] allSelectables;
        private bool[] previousInteractableStates;
        private bool[] previousUiActiveStates;

        private Coroutine evaluationCoroutine;

        private void OnEnable()
        {
            InitializeStateTracking();
            BuildNavigation();

            // Start the polling loop
            evaluationCoroutine = StartCoroutine(StateEvaluationRoutine());
        }

        private void OnDisable()
        {
            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
                evaluationCoroutine = null;
            }
        }

        private void InitializeStateTracking()
        {
            allSelectables = GetComponentsInChildren<Selectable>(true);

            previousInteractableStates = new bool[allSelectables.Length];
            previousUiActiveStates = new bool[allSelectables.Length];

            for (int i = 0; i < allSelectables.Length; i++)
            {
                previousInteractableStates[i] = allSelectables[i].interactable;
                previousUiActiveStates[i] = allSelectables[i].gameObject.activeInHierarchy;
            }
        }

        private IEnumerator StateEvaluationRoutine()
        {
            // Cache the wait instruction to prevent garbage collection (memory allocation) every loop
            WaitForSecondsRealtime waitInstruction = new WaitForSecondsRealtime(evaluationRate);

            while (true)
            {
                // Wait for the specified time before checking again
                yield return waitInstruction;

                if (allSelectables == null) continue;

                bool needsNavigationRebuild = false;

                for (int i = 0; i < allSelectables.Length; i++)
                {
                    if (allSelectables[i] == null) continue;

                    bool currentInteractable = allSelectables[i].interactable;
                    bool currentActive = allSelectables[i].gameObject.activeInHierarchy;

                    if (currentInteractable != previousInteractableStates[i] || currentActive != previousUiActiveStates[i])
                    {
                        previousInteractableStates[i] = currentInteractable;
                        previousUiActiveStates[i] = currentActive;

                        needsNavigationRebuild = true;
                    }
                }

                if (needsNavigationRebuild == true)
                {
                    BuildNavigation();
                }
            }
        }

        public void BuildNavigation()
        {
            List<Selectable> validSelectables = new List<Selectable>();

            foreach (Selectable selectableItem in allSelectables)
            {
                if (selectableItem != null && selectableItem.interactable == true && selectableItem.gameObject.activeInHierarchy == true)
                {
                    validSelectables.Add(selectableItem);
                }
            }

            if (validSelectables.Count <= 1) return;

            for (int i = 0; i < validSelectables.Count; i++)
            {
                Selectable currentItem = validSelectables[i];

                Navigation customNav = new Navigation();
                customNav.mode = Navigation.Mode.Explicit;

                // UP Navigation
                if (i > 0)
                {
                    customNav.selectOnUp = validSelectables[i - 1];
                }
                else if (loopNavigation == true)
                {
                    customNav.selectOnUp = validSelectables[validSelectables.Count - 1];
                }

                // DOWN Navigation
                if (i < validSelectables.Count - 1)
                {
                    customNav.selectOnDown = validSelectables[i + 1];
                }
                else if (loopNavigation == true)
                {
                    customNav.selectOnDown = validSelectables[0];
                }

                currentItem.navigation = customNav;
            }
        }
    }
}