using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    [DisallowMultipleComponent]
    public class AutoSpatialNavigation : MonoBehaviour
    {
        [Header("Spatial Calibration")]
        [Tooltip("Maximum vertical screen-pixel distance to cluster buttons into the same horizontal row.")]
        [SerializeField] private float verticalRowThresholdPixels = 30.0f;

        [Header("Navigation Rules")]
        [Tooltip("If true, pressing Up on the top row loops to the bottom, and Down on the bottom row loops to the top.")]
        [SerializeField] private bool loopNavigation = false;

        [Tooltip("Frequency in seconds to poll for dynamically enabled, disabled, or instantiated UI elements.")]
        [SerializeField] private float evaluationRate = 0.25f;

        private List<Selectable> trackedSelectables = new List<Selectable>();
        private List<bool> trackedInteractableStates = new List<bool>();
        private List<bool> trackedActiveStates = new List<bool>();
        private int cachedChildCount = -1;

        private Canvas cachedCanvas;
        private Camera targetCamera;
        private Coroutine evaluationCoroutine;

        private void Awake()
        {
            ResolveCanvasAndCamera();
        }

        private void OnEnable()
        {
            ResolveCanvasAndCamera();
            InitializeStateTracking();
            BuildNavigation();

            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
            }
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

        private void ResolveCanvasAndCamera()
        {
            if (cachedCanvas == null)
            {
                cachedCanvas = GetComponentInParent<Canvas>();
            }

            if (cachedCanvas != null && cachedCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetCamera = cachedCanvas.worldCamera != null ? cachedCanvas.worldCamera : Camera.main;
            }
            else
            {
                targetCamera = null;
            }
        }

        private void InitializeStateTracking()
        {
            cachedChildCount = transform.childCount;

            trackedSelectables.Clear();
            trackedInteractableStates.Clear();
            trackedActiveStates.Clear();

            Selectable[] allChildSelectables = GetComponentsInChildren<Selectable>(true);
            for (int index = 0; index < allChildSelectables.Length; ++index)
            {
                Selectable selectable = allChildSelectables[index];
                if (selectable != null)
                {
                    trackedSelectables.Add(selectable);
                    trackedInteractableStates.Add(selectable.interactable);
                    trackedActiveStates.Add(selectable.gameObject.activeInHierarchy);
                }
            }
        }

        private IEnumerator StateEvaluationRoutine()
        {
            WaitForSecondsRealtime waitInstruction = new WaitForSecondsRealtime(evaluationRate);

            while (true)
            {
                yield return waitInstruction;

                bool structureChanged = transform.childCount != cachedChildCount;
                if (structureChanged)
                {
                    InitializeStateTracking();
                    BuildNavigation();
                    continue;
                }

                bool statesChanged = false;
                for (int index = 0; index < trackedSelectables.Count; ++index)
                {
                    Selectable selectable = trackedSelectables[index];
                    if (selectable == null)
                    {
                        statesChanged = true;
                        break;
                    }

                    bool currentInteractable = selectable.interactable;
                    bool currentActive = selectable.gameObject.activeInHierarchy;

                    if (currentInteractable != trackedInteractableStates[index] || currentActive != trackedActiveStates[index])
                    {
                        trackedInteractableStates[index] = currentInteractable;
                        trackedActiveStates[index] = currentActive;
                        statesChanged = true;
                    }
                }

                if (statesChanged)
                {
                    BuildNavigation();
                }
            }
        }

        public void BuildNavigation()
        {
            ResolveCanvasAndCamera();

            List<SpatialUIRow> rows = ClusterSelectablesIntoRows();
            int rowCount = rows.Count;

            if (rowCount == 0)
            {
                return;
            }

            for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
            {
                SpatialUIRow currentRow = rows[rowIndex];
                int columnCount = currentRow.Elements.Count;

                for (int colIndex = 0; colIndex < columnCount; ++colIndex)
                {
                    Selectable currentSelectable = currentRow.Elements[colIndex];
                    float currentScreenX = RectTransformUtility.WorldToScreenPoint(targetCamera, currentSelectable.transform.position).x;

                    Navigation customNavigation = new Navigation();
                    customNavigation.mode = Navigation.Mode.Explicit;

                    // Horizontal Navigation (Left / Right within current row)
                    if (colIndex > 0)
                    {
                        customNavigation.selectOnLeft = currentRow.Elements[colIndex - 1];
                    }

                    if (colIndex < columnCount - 1)
                    {
                        customNavigation.selectOnRight = currentRow.Elements[colIndex + 1];
                    }

                    // Vertical Navigation UP (Closest X in row above)
                    if (rowIndex > 0)
                    {
                        SpatialUIRow rowAbove = rows[rowIndex - 1];
                        customNavigation.selectOnUp = rowAbove.FindClosestElementByX(currentScreenX, targetCamera);
                    }
                    else if (loopNavigation && rowCount > 1)
                    {
                        SpatialUIRow bottomRow = rows[rowCount - 1];
                        customNavigation.selectOnUp = bottomRow.FindClosestElementByX(currentScreenX, targetCamera);
                    }

                    // Vertical Navigation DOWN (Closest X in row below)
                    if (rowIndex < rowCount - 1)
                    {
                        SpatialUIRow rowBelow = rows[rowIndex + 1];
                        customNavigation.selectOnDown = rowBelow.FindClosestElementByX(currentScreenX, targetCamera);
                    }
                    else if (loopNavigation && rowCount > 1)
                    {
                        SpatialUIRow topRow = rows[0];
                        customNavigation.selectOnDown = topRow.FindClosestElementByX(currentScreenX, targetCamera);
                    }

                    currentSelectable.navigation = customNavigation;
                }
            }
        }

        private List<SpatialUIRow> ClusterSelectablesIntoRows()
        {
            List<Selectable> validSelectables = new List<Selectable>();
            Selectable[] allSelectables = GetComponentsInChildren<Selectable>(false);

            for (int index = 0; index < allSelectables.Length; ++index)
            {
                Selectable selectable = allSelectables[index];
                if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.interactable)
                {
                    validSelectables.Add(selectable);
                }
            }

            // Sort top-to-bottom in screen space (Y descending)
            validSelectables.Sort((_first, _second) =>
            {
                float firstY = RectTransformUtility.WorldToScreenPoint(targetCamera, _first.transform.position).y;
                float secondY = RectTransformUtility.WorldToScreenPoint(targetCamera, _second.transform.position).y;
                return secondY.CompareTo(firstY);
            });

            List<SpatialUIRow> rows = new List<SpatialUIRow>();

            for (int index = 0; index < validSelectables.Count; ++index)
            {
                Selectable selectable = validSelectables[index];
                float screenPositionY = RectTransformUtility.WorldToScreenPoint(targetCamera, selectable.transform.position).y;

                bool placedInExistingRow = false;
                for (int rowIndex = 0; rowIndex < rows.Count; ++rowIndex)
                {
                    SpatialUIRow row = rows[rowIndex];
                    if (Mathf.Abs(row.AveragePositionY - screenPositionY) <= verticalRowThresholdPixels)
                    {
                        row.AddElement(selectable, screenPositionY);
                        placedInExistingRow = true;
                        break;
                    }
                }

                if (!placedInExistingRow)
                {
                    rows.Add(new SpatialUIRow(screenPositionY, selectable));
                }
            }

            for (int rowIndex = 0; rowIndex < rows.Count; ++rowIndex)
            {
                rows[rowIndex].SortLeftToRight(targetCamera);
            }

            return rows;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (verticalRowThresholdPixels < 1.0f)
            {
                verticalRowThresholdPixels = 1.0f;
            }

            if (evaluationRate < 0.05f)
            {
                evaluationRate = 0.05f;
            }
        }
#endif
    }
}